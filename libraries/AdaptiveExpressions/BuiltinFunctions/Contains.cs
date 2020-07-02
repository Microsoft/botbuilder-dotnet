﻿using System;
using System.Collections;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Contains : ExpressionEvaluator
    {
        public Contains()
            : base(ExpressionType.Contains, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateBinary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            var found = false;
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0] is string string0 && args[1] is string string1)
                {
                    found = string0.Contains(string1);
                }
                else if (FunctionUtils.TryParseList(args[0], out IList ilist))
                {
                    // list to find a value
                    var operands = FunctionUtils.ResolveListValue(ilist);
                    found = operands.Contains(args[1]);
                }
                else if (args[1] is string string2)
                {
                    found = FunctionUtils.TryAccessProperty((object)args[0], string2, out var _);
                }
            }

            return (found, null);
        }
    }
}
