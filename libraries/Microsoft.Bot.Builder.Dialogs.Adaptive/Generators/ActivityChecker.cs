﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    public static class ActivityChecker
    {
        /// <summary>
        /// check the LG string result before generate an Activity.
        /// </summary>
        /// <param name="lgStringResult">string result from languageGenerator.</param>
        /// <returns>Diagnostic list.</returns>
        public static IList<Diagnostic> Check(string lgStringResult)
        {
            if (string.IsNullOrWhiteSpace(lgStringResult))
            {
                return new List<Diagnostic> { BuildDiagnostic("LG output is empty", false) };
            }

            if (!lgStringResult.StartsWith("{") || !lgStringResult.EndsWith("}"))
            {
                return new List<Diagnostic> { BuildDiagnostic("LG output is not a json object, and will fallback to string format.", false) };
            }

            JObject lgStructuredResult;
            try
            {
                lgStructuredResult = JObject.Parse(lgStringResult);
            }
            catch
            {
                return new List<Diagnostic> { BuildDiagnostic("LG output is not a json object, and will fallback to string format.", false) };
            }

            return CheckStructuredResult(lgStructuredResult);
        }

        private static IList<Diagnostic> CheckStructuredResult(JObject lgJObj)
        {
            var result = new List<Diagnostic>();
            var type = GetStructureType(lgJObj);
            
            if (ActivityFactory.GenericCardTypeMapping.ContainsKey(type))
            {
                result.AddRange(CheckAttachment(lgJObj));
            }
            else if (type == nameof(Activity).ToLowerInvariant())
            {
                result.AddRange(CheckActivity(lgJObj));
            }
            else
            {
                var diagnosticMessage = string.IsNullOrWhiteSpace(type) ? 
                    "'type' or '$type' is not exist in lg output json object."
                    : $"Type '{type}' is not support currently.";
                result.Add(BuildDiagnostic(diagnosticMessage));
            }

            return result;
        }

        private static IList<Diagnostic> CheckActivity(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            var activityType = lgJObj["type"]?.ToString()?.Trim();

            result.AddRange(CheckActivityType(activityType));
            result.AddRange(CheckPropertyName(lgJObj, typeof(Activity)));
            result.AddRange(CheckActivityProperities(lgJObj));

            return result;
        }

        private static IList<Diagnostic> CheckActivityType(string activityType)
        {
            var result = new List<Diagnostic>();

            if (!string.IsNullOrEmpty(activityType))
            {
                var types = GetAllPublicConstantValues<string>(typeof(ActivityTypes));

                if (types.All(u => u.ToLowerInvariant() != activityType.ToLowerInvariant()))
                {
                    result.Add(BuildDiagnostic($"'{activityType}' is not a valid activity type."));
                }
            }

            return result;
        }

        private static IList<Diagnostic> CheckActivityProperities(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "attachments":
                        result.AddRange(CheckAttachments(value));
                        break;
                    case "suggestedactions":
                        result.AddRange(CheckSuggestions(value));
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private static IList<Diagnostic> CheckSuggestions(JToken value)
        {
            var actions = NormalizedToList(value);
            return CheckCardActions(actions);
        }

        private static IList<Diagnostic> CheckButtons(JToken value)
        {
            var actions = NormalizedToList(value);
            return CheckCardActions(actions);
        }

        private static IList<Diagnostic> CheckCardActions(IList<JToken> actions)
        {
            var result = new List<Diagnostic>();

            foreach (var action in actions)
            {
                if (!IsStringValue(action))
                {
                    if (action is JObject actionJObj)
                    {
                        result.AddRange(CheckCardAction(actionJObj));
                    }
                    else
                    {
                        result.Add(BuildDiagnostic($"'{action}' is not a valid card action format.", false));
                    }
                }
            }

            return result;
        }

        private static IList<Diagnostic> CheckCardAction(JObject cardActionJObj)
        {
            var result = new List<Diagnostic>();
            var type = GetStructureType(cardActionJObj);
            if (type != nameof(CardAction).ToLowerInvariant())
            {
                result.Add(BuildDiagnostic($"'{type}' is not card action type.", false));
            }
            else
            {
                result.AddRange(CheckPropertyName(cardActionJObj, typeof(CardAction)));
                var cardActionType = cardActionJObj["type"]?.ToString()?.Trim();

                result.AddRange(CheckCardActionType(cardActionType));
            }

            return result;
        }

        private static IList<Diagnostic> CheckCardActionType(string cardActionType)
        {
            var result = new List<Diagnostic>();

            if (!string.IsNullOrEmpty(cardActionType))
            {
                var types = GetAllPublicConstantValues<string>(typeof(ActionTypes));

                if (types.All(u => u.ToLowerInvariant() != cardActionType.ToLowerInvariant()))
                {
                    result.Add(BuildDiagnostic($"'{cardActionType}' is not a valid card action type."));
                }
            }

            return result;
        }

        private static string GetStructureType(JObject jObj)
        {
            if (jObj == null)
            {
                return string.Empty;
            }

            var type = jObj["$type"]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(type))
            {
                // Adaptive card type
                type = jObj["type"]?.ToString()?.Trim();
            }

            return type?.ToLowerInvariant() ?? string.Empty;
        }

        private static IList<Diagnostic> CheckAttachments(JToken value)
        {
            var result = new List<Diagnostic>();

            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson is JObject attachmentsJsonJObj)
                {
                    result.AddRange(CheckAttachment(attachmentsJsonJObj));
                }
            }

            return result;
        }

        private static IList<Diagnostic> CheckAttachment(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            var type = GetStructureType(lgJObj);

            if (ActivityFactory.GenericCardTypeMapping.ContainsKey(type))
            {
                result.AddRange(CheckCardAtttachment(lgJObj));
            }
            else if (type == "adaptivecard")
            {
                // TODO
                // check adaptivecard format
                // it is hard to check the adaptive card without AdaptiveCards package
            }
            else
            {
                result.Add(BuildDiagnostic($"'{type}' is not an attachment type.", false));
            }

            return result;
        }

        private static IList<Diagnostic> CheckCardAtttachment(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLowerInvariant();
                var value = item.Value;

                switch (property)
                {
                    case "buttons":
                        result.AddRange(CheckButtons(value));
                        break;

                    case "autostart":
                    case "shareable":
                    case "autoloop":
                        if (!IsValidBooleanValue(value.ToString()))
                        {
                            result.Add(BuildDiagnostic($"'{value.ToString()}' is not a boolean value."));
                        }

                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private static IList<Diagnostic> CheckPropertyName(JObject value, Type type)
        {
            var result = new List<Diagnostic>();
            if (value == null)
            {
                return result;
            }

            var properties = value.Properties().Select(u => u.Name.ToLowerInvariant()).Where(u => u != "$type");

            var activityProperties = type.GetProperties().Select(u => u.Name.ToLowerInvariant());

            var additionalProperties = properties.Where(u => !activityProperties.Contains(u));
            if (additionalProperties.Any())
            {
                result.Add(BuildDiagnostic($"'{string.Join(",", additionalProperties)}' not support in {type.Name}.", false));
            }

            return result;
        }

        private static bool IsStringValue(JToken value)
        {
            return value is JValue jValue && jValue.Type == JTokenType.String;
        }

        private static bool IsValidBooleanValue(string boolStr)
        {
            if (string.IsNullOrWhiteSpace(boolStr))
            {
                return false;
            }

            return boolStr.ToLowerInvariant() == "true" || boolStr.ToLowerInvariant() == "false";
        }

        private static IList<JToken> NormalizedToList(JToken item)
        {
            return item == null ?
                new List<JToken>() :
                item is JArray array ? array.ToList() : new List<JToken>() { item };
        }

        private static Diagnostic BuildDiagnostic(string message, bool isError = true)
        {
            message = message ?? string.Empty;
            var emptyRange = new Range(new Position(0, 0), new Position(0, 0));

            return isError ? new Diagnostic(emptyRange, message, DiagnosticSeverity.Error)
                : new Diagnostic(emptyRange, message, DiagnosticSeverity.Warning);
        }

        private static IList<T> GetAllPublicConstantValues<T>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }
    }
}
