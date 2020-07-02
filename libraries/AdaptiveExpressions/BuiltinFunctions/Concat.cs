﻿using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Concat : ExpressionEvaluator
    {
        public Concat()
            : base(ExpressionType.Concat, Evaluator(), ReturnType.Array | ReturnType.String, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplySequence(
                        args =>
                        {
                            var firstItem = args[0];
                            var secondItem = args[1];
                            var isFirstList = FunctionUtils.TryParseList(firstItem, out var firstList);
                            var isSecondList = FunctionUtils.TryParseList(secondItem, out var secondList);

                            if (firstItem == null && secondItem == null)
                            {
                                return null;
                            }
                            else if (firstItem == null && isSecondList)
                            {
                                return secondList;
                            }
                            else if (secondItem == null && isFirstList)
                            {
                                return firstList;
                            }
                            else if (isFirstList && isSecondList)
                            {
                                return firstList.OfType<object>().Concat(secondList.OfType<object>()).ToList();
                            }
                            else
                            {
                                return $"{firstItem?.ToString()}{secondItem?.ToString()}";
                            }
                        });
        }
    }
}
