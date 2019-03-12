﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers
{
    public class JPointerRefResolver : IRefResolver
    {
        private const string RefPropertyName = "$ref";

        private readonly JToken rootDocument;

        public JPointerRefResolver(JToken rootDocument = null)
        {
            this.rootDocument = rootDocument;
        }

        public bool IsRef(JToken token)
        {
            return GetRefJProperty(token) != null;
        }

        public JToken Resolve(JToken refToken)
        {
            var jProperty = GetRefJProperty(refToken);

            if (jProperty == null)
            {
                throw new InvalidOperationException("Failed to resolve reference, $ref property not present");
            }

            var refTarget = jProperty.Value.Value<string>();

            var targetFragments = refTarget.Split('#');

            string jsonFile;
            string jsonPointer = null;

            JToken refDoc = this.rootDocument;

            jsonPointer = targetFragments.Length == 2 ? targetFragments[1] : string.Empty;

            if (targetFragments[0] != string.Empty)
            {
                jsonFile = targetFragments[0];
                var jDoc = File.ReadAllText(jsonFile);
                refDoc = JToken.Parse(jDoc);
            }
            else
            {
                // throw
            }

            var jPointer = new JsonPointer(jsonPointer);

            var json = jPointer.Evaluate(refDoc);

            foreach (JProperty prop in refToken.Children<JProperty>())
                {
                    if (prop.Name != "$ref")
                    {
                        // JToken is an object, so we merge objects
                        if (json[prop.Name] != null && json[prop.Name].Type == JTokenType.Object)
                        {
                            JObject targetProperty = json[prop.Name] as JObject;
                            targetProperty.Merge(prop.Value);
                        }
                        // JToken is an object, so we merge objects
                        else if (json[prop.Name] != null && json[prop.Name].Type == JTokenType.Array)
                        {
                            JArray targetArray = json[prop.Name] as JArray;
                            targetArray.Merge(prop.Value);
                        }
                        // JToken is a value, simply assign
                        else
                        {
                            json[prop.Name] = prop.Value;
                        }
                    }
                }

            return json;
        }

        private JProperty GetRefJProperty(JToken token)
        {
            return token?
                .Children<JProperty>()
                .FirstOrDefault(jProperty => jProperty.Name == RefPropertyName);
        }
    }

    public interface IRefResolver
    {
        bool IsRef(JToken token);

        JToken Resolve(JToken refToken);
    }
}
