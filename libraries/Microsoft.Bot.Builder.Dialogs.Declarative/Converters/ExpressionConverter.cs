﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class ExpressionConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IExpressionEval);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new CommonExpression((string)reader.Value);
            }
            throw new JsonSerializationException("Expected string expression.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

}
