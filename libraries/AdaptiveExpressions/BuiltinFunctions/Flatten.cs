﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Flatten : ExpressionEvaluator
    {
        public Flatten()
            : base(ExpressionType.Flatten, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var depth = args.Count > 1 ? Convert.ToInt32(args[1]) : 100;
                            return EvalFlatten((IEnumerable<object>)args[0], depth);
                        });
        }

        private static IEnumerable<object> EvalFlatten(IEnumerable<object> list, int dept)
        {
            var result = list.ToList();
            if (dept < 1)
            {
                dept = 1;
            }

            for (var i = 0; i < dept; i++)
            {
                var hasArray = result.Any(u => FunctionUtils.TryParseList(u, out var _));
                if (hasArray)
                {
                    var tempList = new List<object>();
                    foreach (var item in result)
                    {
                        if (FunctionUtils.TryParseList(item, out var itemList))
                        {
                            foreach (var childItem in itemList)
                            {
                                tempList.Add(childItem);
                            }
                        }
                        else
                        {
                            tempList.Add(item);
                        }
                    }

                    result = tempList.ToList();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Array);
        }
    }
}
