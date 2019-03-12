﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Parsers
{
    public static class TemplateExpressionParser
    {
        private static readonly Regex funcRegex = new Regex(@"{\b[^()]+\((.*)\)}$", RegexOptions.Compiled);
        private static readonly Regex argsRegex = new Regex(@"([^,]+\(.+?\))|([^,]+)", RegexOptions.Compiled);

        // Receives expression of the form: {<func>(<arg1>, <arg2>, <argn>)}
        // and returns an object containig the values <func> and a collection of
        // arg values
        public static List<string> Parse(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                throw new ArgumentException(nameof(template));
            }

            var func = funcRegex.Match(template);

            string innerArgs = func?.Groups?[1]?.Value;

            if (innerArgs == null)
            {
                throw new ArgumentException(nameof(template), "Expected function format {<func>(<arg1>, <arg2>, <argn>)}");
            }

            var paramTags = argsRegex.Matches(innerArgs);

            var paramsList = new List<string>();

            foreach (var param in paramTags)
            {
                paramsList.Add(param.ToString());
            }

            return paramsList;
        }
    }
}
