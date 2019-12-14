using System;

public class Memory
{
	/**
     * Relocates a value to different register or to stack
     * @param value Value to relocate
     * @return Instructions for relocating the value
     */
	public static Instructions Relocate(Unit unit, Value value)
	{
		if (!value.IsCritical)
		{
			Console.Error.WriteLine("BUG: Uncritical values shouldn't be relocated!");
			return new Instructions();
		}

		if (unit.IsAnyRegisterUncritical)
		{
			var register = unit.GetNextRegister();

			var destination = new RegisterReference(register, value.GetSize());

			var instructions = new Instructions();
			instructions.Append(Instruction.Unsafe("mov", destination, value, value.GetSize()));
			instructions.SetReference(destination);

			register.Attach(value);

			return instructions;
		}

		Console.Error.WriteLine("ERROR: Too complex relocation");
		return null;
	}

	/**
     * Moves value from source to destination. If destination contains a value, it's relocated only if it's critical
     * @return Instructions for moving data from source to destination
     */
	public static Reference Move(Unit unit, Instructions instructions, Reference source, Reference destination)
	{
		// Sometimes unnecessary moves happen
		if (source.Equals(destination))
		{
			return destination;
		}

		// Relocate potential critical value from the destination register
		if (destination.IsRegister())
		{
			var register = destination.GetRegister();

			if (register.IsCritical)
			{
				var value = register.Value;
				instructions.Append(Memory.Relocate(unit, value));

				register.Reset();
			}
		}

		// FPU elements need special move instructions
		if (source.GetType() == LocationType.FPU)
		{
			unit.Fpu.Export(unit, instructions, source as FpuStackReference, destination);
		}
		else if (source is StackReference reference && reference.Element.Alignment == 0)
		{
			var stack = unit.Stack;
			stack.Pop(instructions, destination);
		}
		else
		{
			var size = source.GetSize();

			if (destination.IsRegister() && size != Size.DWORD)
			{
				instructions.Append($"movzx {destination.Peek()}, {source.Peek(size)}");
			}
			else
			{
				instructions.Append(Instruction.Unsafe("mov", destination, source, source.GetSize()));
			}
		}

		// Relocate potential source register value
		if (source.IsRegister() && destination.IsRegister())
		{
			var previous = source.GetRegister();
			var current = destination.GetRegister();

			current.Attach(previous.Value);

			previous.Reset();
		}

		return destination;
	}

	/**
     * Moves data from reference to some register. If no register is available, some register will be cleared and used
     * @return Instructions for moving data from reference to some register
     */
	public static Instructions ToRegister(Unit unit, Reference source)
	{
		var instructions = new Instructions();
		var register = unit.GetNextRegister();

		if (register.IsCritical)
		{
			var value = register.Value;
			instructions.Append(Memory.Relocate(unit, value));
		}

		var destination = new RegisterReference(register, Size.DWORD);
		var size = source.GetSize();

		if (size != Size.DWORD)
		{
			instructions.Append($"movzx {destination.Peek()}, {source.Peek(size)}");
		}
		else if (source is StackReference reference && reference.Element.Alignment == 0)
		{
			var stack = unit.Stack;
			stack.Pop(instructions, destination);
		}
		else
		{
			instructions.Append(Instruction.Unsafe("mov", destination, source, destination.GetSize()));
		}
		
		instructions.SetReference(destination);

		return instructions;
	}

	/**
     * Exchanges the values of two registers
     * @return Instructions for exchanging value between registers
     */
	public static Instruction Exchange(Unit unit, Register a, Register b)
	{
		a.Exchange(b);
		return new Instruction($"xchg {a}, {b}");
	}

	/**
     * Creates a register evacuation based on the given unit
     * @param unit Unit to evacuate
     * @return Instructions for evacuating the registers of the given unit
     */
	public static Evacuation Evacuate(Unit unit)
	{
		var evacuation = new Evacuation();

		foreach (var register in unit.Registers)
		{
			if (register.IsCritical)
			{
				evacuation.Values.Add(register.Value);
			}
		}

		return evacuation;
	}

	/**
     * Clears the register using xor instruction. If the register contains a value, it's relocated only if it's critical
     * @param register Register to clear
     * @param zero Should the register be zeroed out using xor instruction
     * @return Instructions for clearing the register properly
     */
	public static Instructions Clear(Unit unit, Register register, bool zero)
	{
		Instructions instructions = new Instructions();

		if (register.IsCritical)
		{
			instructions.Append(Memory.Relocate(unit, register.Value));
		}

		Reference reference = new RegisterReference(register);

		if (zero)
		{
			instructions.Append(new Instruction("xor", reference, reference, reference.GetSize()));
		}

		register.Reset();

		return instructions;
	}

	/**
     * Copies a copy of object pointer to some register
     * @param type Usage type of the object pointer
     * @return Instructions for copying object pointer to some register
     */
	public static Instructions GetObjectPointer(Unit unit, ReferenceType type)
	{
		Instructions instructions = new Instructions();
		Register? register = null;

		// Try to get the appropriate register for the object pointer
		if (type == ReferenceType.DIRECT)
		{
			if (!unit.EDI.IsCritical)
			{
				register = unit.EDI;
			}
		}
		else if (!unit.ESI.IsCritical)
		{
			register = unit.ESI;
		}

		// When the approriate register cannot be used, any uncritical register will be chosen
		if (register == null)
		{
			register = unit.GetNextRegister();
		}

		// Relocate possible value from the chosen register since the register is now reserved for the object pointer
		if (register.IsCritical)
		{
			instructions.Append(Memory.Relocate(unit, register.Value));
		}

		// Move a copy of the object pointer to the chosen register
		instructions.Append(new Instruction("mov", new RegisterReference(register), MemoryReference.Parameter(unit, 0, 4), Size.DWORD));
		instructions.SetReference(Value.GetObjectPointer(register));

		return instructions;
	}
}