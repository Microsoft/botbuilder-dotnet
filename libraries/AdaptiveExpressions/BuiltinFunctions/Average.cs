﻿using System;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Average : ExpressionEvaluator
    {
        public Average(string alias = null))
            : base(ExpressionType.Average, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var operands = FunctionUtils.ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.Average(u => Convert.ToSingle(u));
                        },
                        FunctionUtils.VerifyNumericList);
        }
    }
}
