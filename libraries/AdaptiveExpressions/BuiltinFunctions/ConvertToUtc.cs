﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class ConvertToUtc : ExpressionEvaluator
    {
        public ConvertToUtc()
            : base(ExpressionType.ConvertFromUtc, EvalConvertToUtc, ReturnType.String, Validator)
        {
        }

        private static (object value, string error) EvalConvertToUtc(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                var format = (args.Count() == 3) ? (string)args[2] : FunctionUtils.DefaultDateTimeFormat;
                if (args[1] is string sourceTimeZone)
                {
                    (value, error) = ConvertToUTC(args[0], sourceTimeZone, format);
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a origin time zone string and an optional output format string.";
                }
            }

            return (value, error);
        }

        private static (string, string) ConvertToUTC(object sourceTimestamp, string sourceTimezone, string format)
        {
            string error = null;
            string result = null;
            var srcDt = DateTime.UtcNow;
            try
            {
                if (sourceTimestamp is string st)
                {
                    srcDt = DateTime.Parse(st);
                }
                else
                {
                    srcDt = (DateTime)sourceTimestamp;
                }
            }
            catch
            {
                error = $"illegal time-stamp representation {sourceTimestamp}";
            }

            if (error == null)
            {
                object convertedTimeZone;
                (convertedTimeZone, error) = FunctionUtils.ConvertTimeZoneFormat(sourceTimezone);
                if (error == null)
                {
                    var convertedDateTime = TimeZoneInfo.ConvertTimeToUtc(srcDt, (TimeZoneInfo)convertedTimeZone);
                    (result, error) = FunctionUtils.ReturnFormatTimeStampStr(convertedDateTime, format);
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 3, ReturnType.String);
        }
    }
}
