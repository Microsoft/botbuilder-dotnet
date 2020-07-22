﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a timestamp in the specified format from ticks.
    /// </summary>
    public class FormatTicks : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatTicks"/> class.
        /// </summary>
        public FormatTicks()
            : base(ExpressionType.FormatTicks, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            if (timestamp.IsInteger())
                            {
                                var ticks = Convert.ToInt64(timestamp, CultureInfo.InvariantCulture);
                                var dateTime = new DateTime(ticks);
                                result = dateTime.ToString(args.Count == 2 ? args[1].ToString() : FunctionUtils.DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatTicks first argument {timestamp} must be an integer";
                            }

                            return (result, error);
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Number);
        }
    }
}
