﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class AdaptiveDialogBotTests
    {
        [Fact]
        public async Task AdaptiveDialogBotTurnState()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceProvider.Add("Main.dialog", new MockResource("{ \"$kind\": \"Microsoft.AdaptiveDialog\" }"));
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            // Act
            var bot = new AdaptiveDialogBot(resourceExplorer, "Main.dialog", "defaultLocale", logger, storage, botFrameworkAuthenticationMock.Object);
            await ((IBot)bot).OnTurnAsync(turnContext, CancellationToken.None);

            // Assert
            Assert.NotNull(turnContext.TurnState.Get<BotFrameworkClient>());
            Assert.NotNull(turnContext.TurnState.Get<SkillConversationIdFactoryBase>());
            Assert.NotNull(turnContext.TurnState.Get<ConversationState>());
            Assert.NotNull(turnContext.TurnState.Get<UserState>());
            Assert.NotNull(turnContext.TurnState.Get<ResourceExplorer>());
            Assert.NotNull(turnContext.TurnState.Get<LanguageGenerator>());
            Assert.NotNull(turnContext.TurnState.Get<LanguageGeneratorManager>());
            Assert.NotNull(turnContext.TurnState.Get<LanguagePolicy>());
        }

        private class MockResourceProvider : ResourceProvider
        {
            private IDictionary<string, Resource> _resources = new Dictionary<string, Resource>();

            public MockResourceProvider(ResourceExplorer resourceExplorer)
                : base(resourceExplorer)
            {
            }

            public override IEnumerable<Resource> GetResources(string extension) => Enumerable.Empty<Resource>();

            public override void Refresh()
            {
            }

            public override bool TryGetResource(string id, out Resource resource) => _resources.TryGetValue(id, out resource);

            public void Add(string id, Resource resource) => _resources.Add(id, resource);
        }

        private class MockResource : Resource
        {
            private string _json;

            public MockResource(string json)
            {
                _json = json;
            }

            public override Task<Stream> OpenStreamAsync() => throw new NotImplementedException();

            public override Task<string> ReadTextAsync() => Task.FromResult(_json);
        }
    }
}
