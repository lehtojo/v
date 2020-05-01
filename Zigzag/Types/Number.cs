public abstract class Number : Type
{
	public Format Type { get; private set; }
	public bool IsUnsigned { get; private set; }
	public int Bits { get; private set; }

	public int Bytes => Bits / 8;

	public Number(Format type, int bits, bool unsigned, string name) : base(name, AccessModifier.PUBLIC)
	{
		Type = type;
		Bits = bits;
		IsUnsigned = unsigned;
	}

	public override Format GetFormat()
	{
		return Type;
	}
}
