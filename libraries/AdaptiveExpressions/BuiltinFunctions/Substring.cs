﻿using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Substring : ExpressionEvaluator
    {
        public Substring()
            : base(ExpressionType.Substring, EvalSubstring, ReturnType.String, Validator)
        {
        }

        private static (object value, string error) EvalSubstring(Expression expression, IMemory state, Options options)
        {
            string result = null;
            string error;
            string str;
            (str, error) = expression.Children[0].TryEvaluate<string>(state, options);
            if (error == null)
            {
                if (str == null)
                {
                    result = string.Empty;
                }
                else
                {
                    int start;
                    var startExpr = expression.Children[1];
                    (start, error) = startExpr.TryEvaluate<int>(state, options);
                    if (error == null && (start < 0 || start >= str.Length))
                    {
                        error = $"{startExpr}={start} which is out of range for {str}.";
                    }

                    if (error == null)
                    {
                        int length;
                        if (expression.Children.Length == 2)
                        {
                            // Without length, compute to end
                            length = str.Length - start;
                        }
                        else
                        {
                            var lengthExpr = expression.Children[2];
                            (length, error) = lengthExpr.TryEvaluate<int>(state, options);
                            if (error == null && (length < 0 || start + length > str.Length))
                            {
                                error = $"{lengthExpr}={length} which is out of range for {str}.";
                            }
                        }

                        if (error == null)
                        {
                            result = str.Substring(start, length);
                        }
                    }
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.String, ReturnType.Number);
        }
    }
}
