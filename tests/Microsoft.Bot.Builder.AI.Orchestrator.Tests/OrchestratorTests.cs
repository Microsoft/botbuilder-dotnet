﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.BotFramework.Orchestrator;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    public class OrchestratorTests
    {
        [Fact]
        public async Task TestIntentRecognize()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath")
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.Equal(1, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
        }
        
        [Fact]
        public async Task TestIntentRecognize_Telemetry_LogsPii_WhenTrue()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath"),
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = true
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);

            Assert.Equal(1, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
            ValidateTelemetry(recognizer, telemetryClient, dc, activity, callCount: 1);
        }
        
        [Fact]
        public async Task TestIntentRecognize_Telemetry_DoesNotLogPii_WhenFalse()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath"),
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);

            Assert.Equal(1, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
            ValidateTelemetry(recognizer, telemetryClient, dc, activity, callCount: 1);
        }
        
        [Fact]
        public async Task TestIntentRecognize_LogPii_IsFalseByDefault()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath"),
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            var (logPersonalInfo, _) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
            
            // Should be false by default, when not specified by user.
            Assert.False(logPersonalInfo);
            Assert.Equal(1, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
            ValidateTelemetry(recognizer, telemetryClient, dc, activity, callCount: 1);
        }

        [Fact]
        public async Task TestEntityRecognize()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath"),
                ExternalEntityRecognizer = new NumberEntityRecognizer()
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.NotNull(result.Entities);
            Assert.Equal(new JValue("12"), result.Entities["number"][0]);
            var resolution = result.Entities["$instance"]["number"][0]["resolution"];
            Assert.Equal(new JValue("integer"), resolution["subtype"]);
            Assert.Equal(new JValue("12"), resolution["value"]);
        }

        [Fact]
        public async Task TestAmbiguousResults()
        {
            var mockResult1 = new Result
            {
                Score = 0.61,
                Label = new Label { Name = "mockLabel1" }
            };

            var mockResult2 = new Result
            {
                Score = 0.62,
                Label = new Label { Name = "mockLabel2" }
            };

            var mockScore = new List<Result>
            {
                mockResult1,
                mockResult2
            };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath"),
                DetectAmbiguousIntents = new BoolExpression(true),
                DisambiguationScoreThreshold = new NumberExpression(0.5)
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.True(result.Intents.ContainsKey("ChooseIntent"));
        }

        private static Dictionary<string, string> GetExpectedTelemetryProps(Microsoft.Bot.Schema.IActivity activity, bool logPersonalInformation)
        {
            var props = new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "mockLabel" },
                { "TopIntentScore", "0.9" },
                { "Intents", "{\"mockLabel\":{\"score\":0.9}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"result\":[{\"Label\":{\"Type\":0,\"Name\":\"mockLabel\",\"Span\":{\"Offset\":0,\"Length\":0}},\"Score\":0.9,\"ClosestText\":null}]}" }
            };

            if (logPersonalInformation == true)
            {
                props.Add("Text", activity.AsMessageActivity().Text);
            }

            return props;
        }

        private static void ValidateTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, int callCount)
        {
            var eventName = GetEventName(recognizer.GetType().Name);
            var (logPersonalInfo, error) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
            var expectedTelemetryProps = GetExpectedTelemetryProps(activity, logPersonalInfo);
            var actualTelemetryProps = (Dictionary<string, string>)telemetryClient.Invocations[callCount - 1].Arguments[1];

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasValidTelemetryProps(expectedTelemetryProps, actualTelemetryProps)),
                    null),
                Times.Exactly(callCount));
        }

        private static string GetEventName(string recognizerName)
        {
            return $"{recognizerName}Result";
        }

        private static bool HasValidTelemetryProps(IDictionary<string, string> expected, IDictionary<string, string> actual)
        {
            if (expected.Count == actual.Count)
            {
                foreach (var property in actual)
                {
                    if (expected.ContainsKey(property.Key))
                    {
                        if (property.Value != expected[property.Key])
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
