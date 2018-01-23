﻿using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class LuisRecognizerTests
    {
        public string luisAppId = TestUtilities.GetKey("LUISAPPID");
        public string subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Luis")]
        public async Task Luis_TopIntentAndEntities()
        {
            if (luisAppId == null || subscriptionKey == null)
            {
                Debug.WriteLine($"Missing Luis Key- Skipping test");
                return;
            }
            LuisRecognizerMiddleware recognizer =
                new LuisRecognizerMiddleware(luisAppId, subscriptionKey);
            var context = TestUtilities.CreateEmptyContext();
            context.Request.AsMessageActivity().Text = "I want a ham and cheese sandwich";

            IList<Middleware.Intent> res = await recognizer.Recognize(context);
            Assert.IsTrue(res.Count == 1, "Incorrect number of intents");
            Assert.IsTrue(res[0].Name == "sandwichorder", "Incorrect Name");
            Assert.IsTrue(res[0].Entities.Count > 0, "No Entities Found");
            Assert.IsTrue(((LuisEntity)res[0].Entities[0]).Type == "meat");
            Assert.IsTrue(((LuisEntity)res[0].Entities[0]).Value == "ham");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Luis")]
        public async Task Luis_TopIntentPopulated()
        {
            if (luisAppId == null || subscriptionKey == null)
            {
                Debug.WriteLine($"Missing Luis Key- Skipping test");
                return;
            }

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)                                
                .Use(new LuisRecognizerMiddleware(luisAppId, subscriptionKey))
                .OnReceive(async (context, next) =>
                {
                    context.Reply(context.TopIntent.Name);
                    await next();
                });
            await adapter
                .Send("I want ham and cheese sandwich!")
                    .AssertReply("sandwichorder", "should have sandwichorder as top intent!")
                .StartTest();
        }
    }
}
