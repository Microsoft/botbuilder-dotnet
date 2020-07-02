﻿using System;
using System.Collections;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IndexOf : ExpressionEvaluator
    {
        public IndexOf()
            : base(ExpressionType.IndexOf, EvalIndexOf, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) EvalIndexOf(Expression expression, IMemory state, Options options)
        {
            object result = -1;
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0] is string || args[0] == null)
                {
                    if (args[1] is string || args[1] == null)
                    {
                        result = FunctionUtils.ParseStringOrNull(args[0]).IndexOf(FunctionUtils.ParseStringOrNull(args[1]));
                    }
                    else
                    {
                        error = $"Can only look for indexof string in {expression}";
                    }
                }
                else if (FunctionUtils.TryParseList(args[0], out IList list))
                {
                    result = FunctionUtils.ResolveListValue(list).IndexOf(args[1]);
                }
                else
                {
                    error = $"{expression} works only on string or list.";
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array | ReturnType.String, ReturnType.Object);
        }
    }
}
