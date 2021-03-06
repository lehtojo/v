using System.Collections.Generic;

public class ModifierSectionPattern : Pattern
{
	public const int MODIFIER = 0;
	public const int COLON = 1;

	public const int PRIORITY = 20;

	// Pattern: $modifiers :
	public ModifierSectionPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.OPERATOR
	) { }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[MODIFIER].To<KeywordToken>().Keyword.Type == KeywordType.MODIFIER && tokens[COLON].Is(Operators.COLON);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		return new SectionNode(tokens[MODIFIER].To<KeywordToken>().Keyword.To<ModifierKeyword>().Modifier, tokens[MODIFIER].Position);
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}