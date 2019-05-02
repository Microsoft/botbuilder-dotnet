﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Tests
{
    public class TestUtilities
    {
        public static TurnContext CreateEmptyContext()
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                ChannelId = "EmptyContext",
                Conversation = new ConversationAccount
                {
                    Id = "test",
                },
                From = new ChannelAccount
                {
                    Id = "empty@empty.context.org",
                },
            };
            var bc = new TurnContext(b, a);

            return bc;
        }

        /*
        public static T CreateEmptyContext<T>() where T:ITurnContext
        {
            TestAdapter b = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            Activity a = new Activity();
            if (typeof(T).IsAssignableFrom(typeof(ITurnContext)))
            {
                ITurnContext bc = new TurnContext(b, a);
                return (T)bc;
            }
            else
                throw new ArgumentException($"Unknown Type {typeof(T).Name}");            
        }
        */

        private static Lazy<Dictionary<string, string>> environmentKeys = new Lazy<Dictionary<string, string>>(() =>
        {
            try
            {
                return File.ReadAllLines(@"\\fusebox\private\sdk\UnitTestKeys-new.cmd")
                    .Where(l => l.StartsWith("@set", StringComparison.OrdinalIgnoreCase))
                    .Select(l => l.Replace("@set ", string.Empty, StringComparison.OrdinalIgnoreCase).Split('='))
                    .ToDictionary(pairs => pairs[0], pairs => pairs[1]);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
                return new Dictionary<string, string>();
            }
        });

        public static string GetKey(string key)
        {
            if (!environmentKeys.Value.TryGetValue(key, out var value))
            {
                // fallback to environment variables
                value = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
            }
            return value;
        }
    }
}
