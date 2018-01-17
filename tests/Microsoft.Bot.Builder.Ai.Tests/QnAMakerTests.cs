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
    public class QnaMakerTests
    {
        public string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public string subscriptionKey = TestUtilities.GetKey("QNASUBSCRIPTIONKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer()
        {
            var qna = new QnAMaker(new QnAMakerOptions()
            {
                KnowledgeBaseId = knowlegeBaseId,
                SubscriptionKey = subscriptionKey,
                Top = 1
            });

            var results = await qna.GetAnswers("how do I clean the stove?");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            Assert.IsTrue(results[0].Answer.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThreshold()
        {

            var qna = new QnAMaker(new QnAMakerOptions()
            {
                KnowledgeBaseId = knowlegeBaseId,
                SubscriptionKey = subscriptionKey,
                Top = 1,
                ScoreThreshold = 0.99F
            });

            var results = await qna.GetAnswers("how do I clean the stove?");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get zero result because threshold");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestMiddleware()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .OnReceive(async (context, next) =>
                {
                    if (context.Request.AsMessageActivity().Text == "foo")
                    {
                        context.Reply(context.Request.AsMessageActivity().Text);                        
                    }
                    await next();
                })
                .Use(new QnAMaker(new QnAMakerOptions()
                {
                    KnowledgeBaseId = knowlegeBaseId,
                    SubscriptionKey = subscriptionKey,
                    Top = 1
                }));

            await adapter
                .Send("foo")
                    .AssertReply("foo", "passthrough")
                .Send("how do I clean the stove?")
                    .AssertReply("BaseCamp: You can use a damp rag to clean around the Power Pack. Do not attempt to detach it from the stove body. As with any electronic device, never pour water on it directly. CampStove 2 &amp; CookStove: Power module: Remove the plastic power module from the fuel chamber and wipe it down with a damp cloth with soap and water. DO NOT submerge the power module in water or get it excessively wet. Fuel chamber: Wipe out with a nylon brush as needed. The pot stand at the top of the fuel chamber can be wiped off with a damp cloth and dried well. The fuel chamber can also be washed in a dishwasher. Dry very thoroughly.")
                .StartTest();
        }

    }
}
