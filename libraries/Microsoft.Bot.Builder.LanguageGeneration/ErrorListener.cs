﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG parser error listener.
    /// </summary>
    public class ErrorListener : BaseErrorListener
    {
        private readonly string _source;
        private readonly int _lineOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorListener"/> class.
        /// </summary>
        /// <param name="errorSource">A string value represents the source of the error.</param>
        /// <param name="lineOffset">The offset of the line where the occurred.</param>
        public ErrorListener(string errorSource, int lineOffset = 0)
        {
            _source = errorSource;
            _lineOffset = lineOffset;
        }

        /// <summary>
        /// Upon syntax error, notify any interested parties.
        /// </summary>
        /// <param name="recognizer">What parser got the error. From this object, you can access the context as well
        ///  as the input stream.</param>
        /// <param name="offendingSymbol">The offending token in the input token stream, unless recognizer is a lexer (then
        /// it's null). If no viable alternative error, e has token at which we started production for the decision.</param>
        /// <param name="line">The line number in the input where the error occurred.</param>
        /// <param name="charPositionInLine">The character position within that line where the error occurred.</param>
        /// <param name="msg">The message to emit.</param>
        /// <param name="e">The exception generated by the parser that led to the reporting of an error.
        /// It is null in the case where the parser was able to recover in line without exiting the surrounding rule.</param>
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            var startPosition = new Position(_lineOffset + line, charPositionInLine);
            var stopPosition = new Position(_lineOffset + line, charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1);
            var range = new Range(startPosition, stopPosition);
            var diagnostic = new Diagnostic(range, TemplateErrors.SyntaxError(msg), DiagnosticSeverity.Error, _source);
            throw new TemplateException(diagnostic.ToString(), new List<Diagnostic>() { diagnostic });
        }
    }
}
