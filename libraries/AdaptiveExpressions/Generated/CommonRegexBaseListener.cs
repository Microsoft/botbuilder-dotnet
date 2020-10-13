//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from CommonRegex.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419


#pragma warning disable 3021 // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
#pragma warning disable 0108 // Disable StyleCop warning CS0108, hides inherited member in generated files.


using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ICommonRegexListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public partial class CommonRegexBaseListener : ICommonRegexListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.parse"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParse([NotNull] CommonRegexParser.ParseContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.parse"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParse([NotNull] CommonRegexParser.ParseContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.alternation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAlternation([NotNull] CommonRegexParser.AlternationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.alternation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAlternation([NotNull] CommonRegexParser.AlternationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr([NotNull] CommonRegexParser.ExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr([NotNull] CommonRegexParser.ExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.element"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterElement([NotNull] CommonRegexParser.ElementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.element"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitElement([NotNull] CommonRegexParser.ElementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.quantifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterQuantifier([NotNull] CommonRegexParser.QuantifierContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.quantifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitQuantifier([NotNull] CommonRegexParser.QuantifierContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.quantifier_type"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterQuantifier_type([NotNull] CommonRegexParser.Quantifier_typeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.quantifier_type"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitQuantifier_type([NotNull] CommonRegexParser.Quantifier_typeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.character_class"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCharacter_class([NotNull] CommonRegexParser.Character_classContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.character_class"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCharacter_class([NotNull] CommonRegexParser.Character_classContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.capture"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCapture([NotNull] CommonRegexParser.CaptureContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.capture"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCapture([NotNull] CommonRegexParser.CaptureContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.non_capture"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNon_capture([NotNull] CommonRegexParser.Non_captureContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.non_capture"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNon_capture([NotNull] CommonRegexParser.Non_captureContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.option"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOption([NotNull] CommonRegexParser.OptionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.option"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOption([NotNull] CommonRegexParser.OptionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.option_flag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOption_flag([NotNull] CommonRegexParser.Option_flagContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.option_flag"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOption_flag([NotNull] CommonRegexParser.Option_flagContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom([NotNull] CommonRegexParser.AtomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom([NotNull] CommonRegexParser.AtomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.cc_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCc_atom([NotNull] CommonRegexParser.Cc_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.cc_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCc_atom([NotNull] CommonRegexParser.Cc_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.shared_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterShared_atom([NotNull] CommonRegexParser.Shared_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.shared_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitShared_atom([NotNull] CommonRegexParser.Shared_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLiteral([NotNull] CommonRegexParser.LiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLiteral([NotNull] CommonRegexParser.LiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.cc_literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCc_literal([NotNull] CommonRegexParser.Cc_literalContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.cc_literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCc_literal([NotNull] CommonRegexParser.Cc_literalContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.shared_literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterShared_literal([NotNull] CommonRegexParser.Shared_literalContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.shared_literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitShared_literal([NotNull] CommonRegexParser.Shared_literalContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.number"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNumber([NotNull] CommonRegexParser.NumberContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.number"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNumber([NotNull] CommonRegexParser.NumberContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.octal_char"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOctal_char([NotNull] CommonRegexParser.Octal_charContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.octal_char"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOctal_char([NotNull] CommonRegexParser.Octal_charContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.octal_digit"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOctal_digit([NotNull] CommonRegexParser.Octal_digitContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.octal_digit"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOctal_digit([NotNull] CommonRegexParser.Octal_digitContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.digits"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDigits([NotNull] CommonRegexParser.DigitsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.digits"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDigits([NotNull] CommonRegexParser.DigitsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.digit"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDigit([NotNull] CommonRegexParser.DigitContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.digit"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDigit([NotNull] CommonRegexParser.DigitContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.name"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterName([NotNull] CommonRegexParser.NameContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.name"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitName([NotNull] CommonRegexParser.NameContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.alpha_nums"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAlpha_nums([NotNull] CommonRegexParser.Alpha_numsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.alpha_nums"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAlpha_nums([NotNull] CommonRegexParser.Alpha_numsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.non_close_parens"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNon_close_parens([NotNull] CommonRegexParser.Non_close_parensContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.non_close_parens"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNon_close_parens([NotNull] CommonRegexParser.Non_close_parensContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.non_close_paren"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNon_close_paren([NotNull] CommonRegexParser.Non_close_parenContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.non_close_paren"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNon_close_paren([NotNull] CommonRegexParser.Non_close_parenContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommonRegexParser.letter"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLetter([NotNull] CommonRegexParser.LetterContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommonRegexParser.letter"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLetter([NotNull] CommonRegexParser.LetterContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
