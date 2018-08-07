﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class AppInsight_MiddlewareTests
    {
        private List<ITelemetry> _sendItems;
        private TelemetryConfiguration _configuration;
        [TestInitialize]
        public void TestInitialize()
        {
            _configuration = new TelemetryConfiguration();
            _sendItems = new List<ITelemetry>();
            // Mock TelemetryChannel
            var mock = new Mock<ITelemetryChannel>();
            mock.Setup(channel => channel.Send(It.IsAny<ITelemetry>()))
                .Callback<ITelemetry>(item => _sendItems.Add(item));
            _configuration.TelemetryChannel = mock.Object;
            _configuration.InstrumentationKey = Guid.NewGuid().ToString();
            _configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            
        }

        [TestMethod]
        [TestCategory("Middleware")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppInsight_BadInstrumentation()
        {
            new AppInsightsLoggerMiddleware(null);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void AppInsight_CheckProperties1()
        {


            var mw = new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, false, _configuration);


            Assert.IsFalse(mw.LogOriginalMessage);
            Assert.IsFalse(mw.LogUserName);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void AppInsight_CheckProperties2()
        {


            var mw = new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), true, false, _configuration);


            Assert.IsFalse(mw.LogOriginalMessage);
            Assert.IsTrue(mw.LogUserName);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void AppInsight_CheckProperties3()
        {


            var mw = new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, true, _configuration);


            Assert.IsTrue(mw.LogOriginalMessage);
            Assert.IsFalse(mw.LogUserName);
        }


        [TestMethod]
        [TestCategory("Middleware")]
        public async Task AppInsight_LogActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, false, _configuration));
            //string instrumentationKey, bool logUserName = false, bool logOriginalMessage = false)
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(2, _sendItems.Count);
            Assert.IsFalse(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.IsFalse(_sendItems[1].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.IsTrue(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromIdProperty));
            Assert.IsTrue(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.ChannelProperty));
            Assert.IsTrue(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.ConversationIdProperty));
            Assert.IsFalse(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromNameProperty));
            Assert.IsFalse(_sendItems[1].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromNameProperty));
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task AppInsight_LogUsername()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), true, false, _configuration));
            //string instrumentationKey, bool logUserName = false, bool logOriginalMessage = false)
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(2, _sendItems.Count);
            Assert.IsFalse(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.IsFalse(_sendItems[1].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.IsTrue(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromNameProperty));
            Assert.IsTrue(_sendItems[1].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromNameProperty));
        }
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task AppInsight_LogText()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, true, _configuration));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(2, _sendItems.Count);
            Assert.IsTrue(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.IsTrue(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.AreEqual<string>(_sendItems[0].Context.Properties[AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty], "foo");
            Assert.IsTrue(_sendItems[1].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty));
            Assert.AreEqual<string>(_sendItems[1].Context.Properties[AppInsightsLoggerMiddleware.AppInsightsConstants.TextProperty], "bar");
            Assert.IsFalse(_sendItems[0].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromNameProperty));
            Assert.IsFalse(_sendItems[1].Context.Properties.ContainsKey(AppInsightsLoggerMiddleware.AppInsightsConstants.FromNameProperty));
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task AppInsight_UseContext()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, false, _configuration));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                var telemetry = context.Services.Get<TelemetryClient>(AppInsightsLoggerMiddleware.AppInsightsServiceKey);
                var ex = new Exception("Test123");
                telemetry.TrackException(ex);
                telemetry.TrackTrace("Testing123");

                await context.SendActivityAsync(typingActivity);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(6, _sendItems.Count);
            Assert.IsTrue(_sendItems[1].GetType() == typeof(ExceptionTelemetry));
            Assert.IsTrue(((ExceptionTelemetry)_sendItems[1]).Exception.GetType() == typeof(Exception));
            Assert.AreEqual<string>(((ExceptionTelemetry)_sendItems[1]).Exception.Message, "Test123");

            Assert.IsTrue(_sendItems[2].GetType() == typeof(TraceTelemetry));
            Assert.AreEqual<string>(((TraceTelemetry)_sendItems[2]).Message, "Testing123");

            Assert.IsTrue(_sendItems[3].GetType() == typeof(EventTelemetry));
            Assert.AreEqual<string>(((EventTelemetry)_sendItems[3]).Name, AppInsightsLoggerMiddleware.BotMsgEvent);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task AppInsight_SimulateLuis()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, false, _configuration));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                var telemetry = context.Services.Get<TelemetryClient>(AppInsightsLoggerMiddleware.AppInsightsServiceKey);
                Dictionary<string, string> properties = new Dictionary<string, string>()
                {
                    {"Question", "My Question"},
                    {"ConversationId", "ConversationId" },
                    {"ActivityId", "MyActivityId" },
                    {"CorrelationId", "MyCorrelationId" },
                };
                Dictionary<string, double> metrics = new Dictionary<string, double>()
                {
                    { "Score", 34.67 },
                };
                telemetry.TrackEvent("Intent.MyIntent", properties, metrics);

                await context.SendActivityAsync(typingActivity);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(4, _sendItems.Count);
            Assert.IsTrue(_sendItems[1].GetType() == typeof(EventTelemetry));
            Assert.AreEqual<string>(((EventTelemetry)_sendItems[1]).Name, "Intent.MyIntent");
            Assert.AreEqual(((EventTelemetry)_sendItems[1]).Metrics.Count, 1);
            Assert.AreEqual(((EventTelemetry)_sendItems[1]).Properties.Count, 4);
            Assert.AreEqual<string>(((EventTelemetry)_sendItems[1]).Properties["Question"], "My Question");
            Assert.AreEqual<string>(((EventTelemetry)_sendItems[1]).Properties["ActivityId"], "MyActivityId");
            Assert.AreEqual<string>(((EventTelemetry)_sendItems[1]).Properties["ConversationId"], "ConversationId");
        }
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task AppInsight_Dependency()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new AppInsightsLoggerMiddleware(Guid.NewGuid().ToString(), false, false, _configuration));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                var telemetry = context.Services.Get<TelemetryClient>(AppInsightsLoggerMiddleware.AppInsightsServiceKey);
                telemetry.TrackDependency(
                                        "Luis",                           // Dependency Type 
                                        "Recognize",                            // Operation
                                        "Luis",                                 // Name
                                        "Book me a reservation",                // Data
                                        DateTimeOffset.Now,                     // Timestamp
                                        TimeSpan.FromMilliseconds(1000),        // Duration
                                        "200",                                  // Result
                                        true);                                  // Success?

                await context.SendActivityAsync(typingActivity);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(4, _sendItems.Count);
            Assert.IsTrue(_sendItems[1].GetType() == typeof(DependencyTelemetry));
            Assert.AreEqual("Book me a reservation", ((DependencyTelemetry)_sendItems[1]).Data);
            Assert.AreEqual("Recognize", ((DependencyTelemetry)_sendItems[1]).Target);
        }




    }
}
