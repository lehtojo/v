﻿using System;

public static class Inlines
{
	private static void Transfer(FunctionImplementation implementation)
	{
		var reference = (FunctionNode)implementation.References[0];
		var node = (ImplementationNode?)reference.FindParent(n => n.GetNodeType() == NodeType.IMPLEMENTATION_NODE);

		if (node == null)
		{
			throw new ApplicationException("Couldn't find the implementation node of a function");
		}

		var context = node.Implementation;

		// Change inline function parameter into local variables
		implementation.Parameters.ForEach(p => p.Category = VariableCategory.LOCAL);

		// Transfer all inline function properties
		context.Merge(implementation);

		foreach (var parameter in implementation.Parameters)
		{
			var name = $"inline.{implementation.Metadata!.GetFullname().Replace('_', '.')}.{parameter.Name}";
			parameter.Name = name;

			context.Declare(parameter);
		}
		
		var inline = new InlineNode(implementation, reference, implementation.Node!);
		reference.Replace(inline);
	}

	public static void Build(Context context)
	{
		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				if (Flag.Has(overload.Modifiers, AccessModifier.EXTERNAL))
				{
					continue;
				}

				foreach (var implementation in overload.Implementations)
				{
					if (implementation.Node != null && 
						implementation.IsInline && 
					   !implementation.IsMember)
					{
						Transfer(implementation);
					}
				}
			}
		}

		foreach (var type in context.Types.Values)
		{
			Build(type);
		}
	}
}