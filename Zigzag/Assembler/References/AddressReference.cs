using System.Collections.Generic;

public class AddressReference : Reference
{
	private object Value { get; set; }

	public AddressReference(object value) : base(Size.DWORD)
	{
		Value = value;
	}

	public override string Use(Size size)
	{
		return $"{size} [{(long)Value}]";
	}

	public override string Use()
	{
		return $"[{(long)Value}]";
	}

	public override bool IsComplex()
	{
		return true;
	}

	public override LocationType GetType()
	{
		return LocationType.ADDRESS;
	}

	public override bool Equals(object? obj)
	{
		return obj is AddressReference reference &&
			   EqualityComparer<object>.Default.Equals(Value, reference.Value);
	}
}