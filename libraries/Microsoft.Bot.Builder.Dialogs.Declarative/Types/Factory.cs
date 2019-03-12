﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Plugins;
using Microsoft.Bot.Builder.Dialogs.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Steps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Types
{
    public static class Factory
    {
        private static Dictionary<Type, ICustomDeserializer> builders = new Dictionary<Type, ICustomDeserializer>();
        private static Dictionary<string, Type> types = new Dictionary<string, Type>();
        private static Dictionary<Type, string> names = new Dictionary<Type, string>();

        static Factory()
        {
            RegisterDefaults();   
        }

        public static void Register(string name, Type type, ICustomDeserializer loader = null)
        {
            // Default loader if none specified
            if (loader == null)
            {
                loader = new DefaultLoader();
            }

            types.Add(name, type);
            names.Add(type, name);
            builders.Add(type, loader);
        }

        public static async Task RegisterPlugin(IPlugin plugin)
        {
            await plugin.Load();
            Register(plugin.SchemaUri, plugin.Type, plugin.Loader);
        }

        public static T Build<T>(string name, JToken obj, JsonSerializer serializer) where T : class
        {
            ICustomDeserializer builder;
            var type = TypeFromName(name);

            if (type == null)
            {
                throw new ArgumentException($"Type {name} not registered in factory.");
            }
            var found = builders.TryGetValue(type, out builder);

            if (!found)
            {
                throw new ArgumentException($"Type {name} not registered in factory.");
            }

            var built = builder.Load(obj, serializer, type);

            var result = built as T;

            if (result == null)
            {
                throw new Exception($"Factory registration for name {name} resulted in type {built.GetType()}, but expected assignable to {typeof(T)}");
            }

            return result;
        }

        public static Type TypeFromName(string name)
        {
            Type type;
            return types.TryGetValue(name, out type) ? type : default(Type);
        }

        public static string NameFromType(Type type)
        {
            string name;
            return names.TryGetValue(type, out name) ? name: default(string);
        }

        public static void Reset()
        {
            types.Clear();
            names.Clear();
            builders.Clear();
            RegisterDefaults();
        }

        private static void RegisterDefaults()
        {
            //TODO: we don't want this static initialization, leaving it here for convenience now
            // while things are changing rapidly still

            // Rules
            Register("Microsoft.IntentRule", typeof(IntentRule));
            Register("Microsoft.EventRule", typeof(EventRule));
            Register("Microsoft.FallbackRule", typeof(FallbackRule));
            Register("Microsoft.IfPropertyRule", typeof(IfPropertyRule));
            Register("Microsoft.ReplacePlanRule", typeof(ReplacePlanRule));
            Register("Microsoft.UtteranceRecognizeRule", typeof(UtteranceRecognizeRule));
            Register("Microsoft.WelcomeRule", typeof(WelcomeRule));

            // Steps
            Register("Microsoft.CallDialog", typeof(CallDialog));
            Register("Microsoft.CancelDialog", typeof(CancelDialog));
            Register("Microsoft.EndDialog", typeof(EndDialog));
            Register("Microsoft.GotoDialog", typeof(GotoDialog));
            Register("Microsoft.IfProperty", typeof(IfProperty));
            Register("Microsoft.SendActivity", typeof(SendActivity));
            Register("Microsoft.WaitForInput", typeof(WaitForInput));

            // Dialogs
            Register("Microsoft.ComponentDialog", typeof(ComponentDialog), new ComponentDialogLoader());
            Register("Microsoft.RuleDialog", typeof(RuleDialog));
            Register("Microsoft.TextPrompt", typeof(TextPrompt));
            Register("Microsoft.IntegerPrompt", typeof(IntegerPrompt));
            Register("Microsoft.FloatPrompt", typeof(FloatPrompt));

            // Recognizers
            Register("Microsoft.LuisRecognizer", typeof(LuisRecognizer), new LuisRecognizerLoader());
            Register("Microsoft.RegexRecognizer", typeof(RegexRecognizer));

            // Storage
            Register("Microsoft.MemoryStorage", typeof(MemoryStorage));
        }
    }
}
