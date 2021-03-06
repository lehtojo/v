using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateFunction : Function
{
	private const int HEAD = 0;

	public List<string> TemplateArgumentNames { get; private set; }
	public List<Type> TemplateArgumentTypes { get; }
	private Dictionary<string, Function> Variants { get; set; } = new Dictionary<string, Function>();

	public TemplateFunction(Context context, int modifiers, string name, List<string> template_argument_names, Position? start, Position? end) : base(context, modifiers | Modifier.TEMPLATE_FUNCTION, name, start, end)
	{
		TemplateArgumentNames = template_argument_names;
		TemplateArgumentTypes = new List<Type>();

		for (var i = 0; i < TemplateArgumentNames.Count; i++)
		{
			TemplateArgumentTypes.Add(new Type(this, TemplateArgumentNames[i], Modifier.DEFAULT, Start));
		}
	}

	public TemplateFunction(Context context, int modifiers, string name, int arguments) : base(context, modifiers | Modifier.TEMPLATE_FUNCTION, name, (Position?)null, (Position?)null)
	{
		TemplateArgumentNames = new List<string>();
		TemplateArgumentTypes = new List<Type>();

		for (var i = 0; i < arguments; i++)
		{
			TemplateArgumentNames.Add($"T{i}");
			TemplateArgumentTypes.Add(new Type(this, TemplateArgumentNames[i], Modifier.DEFAULT, Start));
		}
	}

	private Function? TryGetVariant(Type[] template_parameters)
	{
		var identifier = string.Join(", ", template_parameters.Take(TemplateArgumentNames.Count).Select(i => i.ToString()));

		if (Variants.TryGetValue(identifier, out Function? variant))
		{
			return variant;
		}

		return null;
	}

	private void InsertArguments(List<Token> tokens, Type[] arguments)
	{
		for (var i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].Type == TokenType.IDENTIFIER)
			{
				var j = TemplateArgumentNames.IndexOf(tokens[i].To<IdentifierToken>().Value);

				if (j == -1)
				{
					continue;
				}

				var position = tokens[i].To<IdentifierToken>().Position;

				tokens.RemoveAt(i);
				tokens.InsertRange(i, Common.GetTokens(arguments[j], position));
			}
			else if (tokens[i].Type == TokenType.FUNCTION)
			{
				InsertArguments(tokens[i].To<FunctionToken>().Parameters.Tokens, arguments);
			}
			else if (tokens[i].Type == TokenType.CONTENT)
			{
				InsertArguments(tokens[i].To<ContentToken>().Tokens, arguments);
			}
		}
	}

	private Function? CreateVariant(Type[] template_arguments)
	{
		var identifier = string.Join(", ", template_arguments.Select(a => a.ToString()));

		// Copy the blueprint and insert the specified arguments to their places
		var blueprint = Blueprint.Select(t => (Token)t.Clone()).ToList();
		blueprint[HEAD].To<FunctionToken>().Identifier.Value = Name + $"<{identifier}>";

		InsertArguments(blueprint, template_arguments);

		// Parse the new variant
		var result = Parser.Parse(Parent ?? throw new ApplicationException("Template function did not have parent context"), blueprint).First;

		if (result == null || !result.Is(NodeType.FUNCTION_DEFINITION))
		{
			throw new ApplicationException("Tried to parse a new variant from template function but the result was not a new function");
		}

		// Register the new variant
		var variant = result.To<FunctionDefinitionNode>().Function;
		Variants.Add(identifier, variant);

		return variant;
	}

	public override bool Passes(List<Type> arguments)
	{
		throw new InvalidOperationException("Tried to execute pass function without template parameters");
	}

	public new bool Passes(List<Type> parameters, Type[] template_arguments)
	{
		if (parameters.Count != Parameters.Count || template_arguments.Length != TemplateArgumentNames.Count)
		{
			return false;
		}

		for (var i = 0; i < Parameters.Count; i++)
		{
			if (Parameters[i].Type == null)
			{
				continue;
			}

			var j = TemplateArgumentTypes.FindIndex(l => l == Parameters[i].Type);

			if (j != -1)
			{
				if (parameters[i] != template_arguments[j])
				{
					return false;
				}
			}
			else if (Resolver.GetSharedType(Parameters[i].Type, parameters[i]) == null)
			{
				return false;
			}
		}

		return true;
	}

	public override FunctionImplementation? Get(IEnumerable<Type> arguments)
	{
		throw new InvalidOperationException("Tried to get overload of template function without template parameters");
	}

	public FunctionImplementation? Get(List<Type> parameters, Type[] template_arguments)
	{
		if (template_arguments.Length != TemplateArgumentNames.Count)
		{
			throw new ApplicationException("Missing template arguments");
		}

		var variant = TryGetVariant(template_arguments) ?? CreateVariant(template_arguments);

		if (variant == null)
		{
			return null;
		}

		var implementation = variant.Get(parameters)!;
		implementation.Identifier = Name;
		implementation.Metadata.Modifiers = Modifiers;
		implementation.TemplateArguments = template_arguments;

		return implementation;
	}

	public override string ToString()
	{
		return Name + '<' + string.Join(", ", TemplateArgumentNames) + $">({string.Join(", ", Parameters)})";
	}
}