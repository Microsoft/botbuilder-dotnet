﻿using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class StartsWith : ExpressionEvaluator
    {
        public StartsWith()
            : base(ExpressionType.StartsWith, Evaluator(), ReturnType.Boolean, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            string rawStr = FunctionUtils.ParseStringOrNull(args[0]);
                            string seekStr = FunctionUtils.ParseStringOrNull(args[1]);
                            return rawStr.StartsWith(seekStr);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);
        }
    }
}
