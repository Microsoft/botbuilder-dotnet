﻿using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class SortByDescending : ExpressionEvaluator
    {
        public SortByDescending()
            : base(ExpressionType.SortByDescending, FunctionUtils.SortBy(true), ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array);
        }
    }
}
