﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the current timestamp.
    /// </summary>
    public class UtcNow : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtcNow"/> class.
        /// </summary>
        public UtcNow()
            : base(ExpressionType.UtcNow, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => DateTime.UtcNow.ToString(args.Count == 1 ? args[0].ToString() : FunctionUtils.DefaultDateTimeFormat, CultureInfo.InvariantCulture));
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String });
        }
    }
}
