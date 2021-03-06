﻿using System.Collections.Generic;

public class VirtualFunction : Function
{
	public Type Type { get; private set; }
	public Type? ReturnType { get; set; }

	public VirtualFunction(Type type, string name, Type? return_type, Position? start, Position? end) : base(type, Modifier.DEFAULT, name, new List<Token>(), start, end)
	{
		Type = type;
		ReturnType = return_type;
	}
}