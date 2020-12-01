public class LabelInstruction : Instruction
{
	public Label Label { get; private set; }

	public LabelInstruction(Unit unit, Label label) : base(unit)
	{
		Label = label;
	}

	public override void OnBuild()
	{
		Build($"{Label.GetName()}:");
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.LABEL;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}
}