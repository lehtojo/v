public class U64 : Number
{
	private const int BYTES = 8;

	public U64() : base(Format.UINT64, 64, true, "u64") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
