using System;
using System.Linq;
using System.Collections.Generic;

public class CacheVariablesInstruction : Instruction
{
	private List<VariableUsageDescriptor> Usages { get; }
	private Node[] Roots { get; }
	private bool NonVolatileMode { get; }

	public CacheVariablesInstruction(Unit unit, Node[] roots, List<VariableUsageDescriptor> variables, bool non_volatile_mode) : base(unit)
	{
		Usages = variables;
		Roots = roots;
		NonVolatileMode = non_volatile_mode;
		Description = "Prepares the stored variables based on their usage";

		// Load all the variables before caching
		Usages.ForEach(u => u.Reference = References.GetVariable(unit, u.Variable));
	}

	/// <summary>
	/// Removes all usages which should not be cached
	/// </summary>
	private void Filter()
	{
		// Remove all readonly constants
		for (var i = Usages.Count - 1; i >= 0; i--)
		{
			var usage = Usages[i];

			// If there's no reference, the usage should be skipped and removed
			if (usage.Reference == null)
			{
				Usages.RemoveAt(i);
				continue;
			}

			// There's no need to move variables to registers that aren't edited in the body and are constants
			if (Roots.All(i => !usage.Variable.IsEditedInside(i)) && usage.Reference.IsConstant)
			{
				Usages.RemoveAt(i);
			}
		}

		// Removed linked variables since they will be handled by the branching system
		for (var i = Usages.Count - 1; i >= 0; i--)
		{
			var usage = Usages[i];

			// The current usage should be removed if it's linked to another variable since it would cause unnecessary move instructions
			if (Usages.GetRange(0, i).Any(u => u.Reference!.Equals(usage.Reference)))
			{
				Usages.RemoveAt(i);
			}
		}
	}

	private struct RegisterOccuptionInfo
	{
		public VariableUsageDescriptor Information;
		public Register Register;
	}

	private class State
	{
		private Unit Unit { get; set; }
		private List<Register> AvailableStandardRegisters { get; set; }
		private List<Register> AvailableMediaRegisters { get; set; }
		private List<RegisterOccuptionInfo> OccupiedStandardRegisters { get; set; } = new List<RegisterOccuptionInfo>();
		private List<RegisterOccuptionInfo> OccupiedMediaRegisters { get; set; } = new List<RegisterOccuptionInfo>();
		public List<VariableUsageDescriptor> RemainingVariables { get; private set; } = new List<VariableUsageDescriptor>();

		/// <summary>
		/// Occupies the specified register with the specified variable and stores its usage information
		/// </summary>
		private void Occupie(Register register, VariableUsageDescriptor usage)
		{
			if (register.IsMediaRegister)
			{
				AvailableMediaRegisters.Remove(register);
				OccupiedMediaRegisters.Add(new RegisterOccuptionInfo() { Information = usage, Register = register });
			}
			else
			{
				AvailableStandardRegisters.Remove(register);
				OccupiedStandardRegisters.Add(new RegisterOccuptionInfo() { Information = usage, Register = register });
			}
		}

		public State(Unit unit, List<VariableUsageDescriptor> usages, bool non_volatile_only)
		{
			Unit = unit;

			// Retrieve available registers matching the configured mode
			AvailableStandardRegisters = new List<Register>(non_volatile_only ? unit.NonVolatileRegisters : unit.NonReservedRegisters);
			AvailableMediaRegisters = non_volatile_only ? unit.MediaRegisters.Where(r => !r.IsVolatile).ToList() : new List<Register>(unit.MediaRegisters);

			// Pack all register together for simple iteration
			var registers = AvailableStandardRegisters.Concat(AvailableMediaRegisters).ToList();

			RemainingVariables = usages.ToList();

			// Find all the usages which are already cached
			foreach (var usage in usages)
			{
				// Try to find a register that contains the current variable
				var register = registers.Find(r => Equals(r.Handle, usage.Reference));

				if (register == null)
				{
					continue;
				}

				// Remove this variable from the remaining variables list since it's in a correct location
				RemainingVariables.Remove(usage);

				// The current variable occupies the register so it's not available
				Occupie(register, usage);
			}

			// Sort the variables based on their number of usages (the least used variables first)
			OccupiedStandardRegisters.Sort((a, b) => a.Information.Usages.CompareTo(b.Information.Usages));
			OccupiedMediaRegisters.Sort((a, b) => a.Information.Usages.CompareTo(b.Information.Usages));

			// Sort the variables based on their number of usages (the most used variables first)
			RemainingVariables.Sort((a, b) => b.Usages.CompareTo(a.Usages));
		}

		/// <summary>
		/// Moves the specified variable to memory
		/// </summary>
		public void Release(VariableUsageDescriptor usage)
		{
			if (!usage.Reference!.IsAnyRegister)
			{
				return;
			}

			Unit.Release(usage.Reference!.Value.To<RegisterHandle>().Register);
		}

		public Register? TryGetNextRegister(VariableUsageDescriptor usage)
		{
			var use_media_register = usage.Reference!.Format.IsDecimal();
			var registers = use_media_register ? AvailableMediaRegisters : AvailableStandardRegisters;

			// Try to find a register which holds a value but it's not important anymore
			var register = registers.Find(r => r.IsAvailable(Unit.Position));

			if (register != null)
			{
				return register;
			}

			// Try to get the next register
			register = registers.Pop();

			if (register != null)
			{
				// Clear the register safely if it holds something
				Unit.Release(register);
				return register;
			}

			var occupied = use_media_register ? OccupiedMediaRegisters : OccupiedStandardRegisters;

			if (!occupied.Any())
			{
				return null;
			}

			// The current variable is only allowed to take over the used register if it will be more used
			var index = occupied.FindLastIndex(i => i.Information.Usages < usage.Usages);

			if (index < 0)
			{
				return null;
			}

			var target = occupied[index];

			// Release the removed variable since its register will used with the current variable
			Release(target.Information);

			occupied.RemoveAt(index);

			return target.Register;
		}
	}

	public override void OnBuild()
	{
		// Removes all usages which should not be cached
		Filter();

		// Inspect the current state of the unit
		var state = new State(Unit, Usages, NonVolatileMode);
		var i = 0;

		for (; i < state.RemainingVariables.Count; i++)
		{
			// Try to find a justified register for the variable
			var current = state.RemainingVariables[i];
			var register = state.TryGetNextRegister(current);

			if (register == null)
			{
				// There is no register left for the current variable
				continue;
			}

			// Relocate the variable to the register
			var destination = new Result(new RegisterHandle(register), current.Reference!.Format);
			var source = current.Reference!;
			
			Unit.Append(new MoveInstruction(Unit, destination, source)
			{
				Type = MoveType.RELOCATE,
				Description = $"Moves variable '{current.Variable.Name}' to a register which suits the future instructions"
			});
		}

		// Release the remaining variables
		state.RemainingVariables.Skip(i).ForEach(u => state.Release(u));
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Cache-Variables-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.CACHE_VARIABLES;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}