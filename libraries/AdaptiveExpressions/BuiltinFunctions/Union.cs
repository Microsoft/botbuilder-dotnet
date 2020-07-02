﻿using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Union : ExpressionEvaluator
    {
        public Union()
            : base(ExpressionType.Union, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var result = (IEnumerable<object>)args[0];
                            for (var i = 1; i < args.Count; i++)
                            {
                                var nextItem = (IEnumerable<object>)args[i];
                                result = result.Union(nextItem);
                            }

                            return result.ToList();
                        }, FunctionUtils.VerifyList);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Array);
        }
    }
}
