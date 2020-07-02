﻿using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Date : ExpressionEvaluator
    {
        public Date()
            : base(ExpressionType.Date, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Date.ToString("M/dd/yyyy", CultureInfo.InvariantCulture)));
        }
    }
}
