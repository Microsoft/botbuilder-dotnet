﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// NumberExpression - represents a property which is either a float or a string expression which resolves to a float.
    /// </summary>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(NumberExpressionConverter))]
    public class NumberExpression : ExpressionProperty<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        public NumberExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="value">value to use.</param>
        public NumberExpression(float value) 
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expression">string to interpret as expression or number.</param>
        public NumberExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expression">expression.</param>
        public NumberExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="lambda">expression.</param>
        public NumberExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expressionOrValue">jtoken to interpret as expression or number.</param>
        public NumberExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Convert a float value to a NumberExpression instance.
        /// </summary>
        /// <param name="value">A float number to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator NumberExpression(float value) => new NumberExpression(value);

        /// <summary>
        /// Convert a string value to a NumberExpression instance.
        /// </summary>
        /// <param name="expression">A string value to convert.</param>
        public static implicit operator NumberExpression(string expression) => new NumberExpression(expression);

        /// <summary>
        /// Convert an Expression instance to a NumberExpression instance.
        /// </summary>
        /// <param name="expression">An Expression instance to convert.</param>
        public static implicit operator NumberExpression(Expression expression) => new NumberExpression(expression);

        /// <summary>
        /// Convert a JSON Token to an NumberExpression instance.
        /// </summary>
        /// <param name="expressionOrValue">A JSON Token to convert.</param>
        public static implicit operator NumberExpression(JToken expressionOrValue) => new NumberExpression(expressionOrValue);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }
}
