﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class TextPromptTests
    {
        [TestMethod]
        public async Task TextPromptOptionsAsDictionary()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new TextPrompt();

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    var options = new Dictionary<string, object>
                    {
                        { nameof(PromptOptions.PromptActivity), MessageFactory.Text("Enter some text.") }
                    };

                    await prompt.Begin(turnContext, state, options);
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var textResult = (TextResult)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the text '{textResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("some text")
            .AssertReply("Bot received the text 'some text'.")
            .StartTest();
        }

        [TestMethod]
        public async Task TextPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new TextPrompt();

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Enter some text." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var textResult = (TextResult)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the text '{textResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("some text")
            .AssertReply("Bot received the text 'some text'.")
            .StartTest();
        }

        [TestMethod]
        public async Task TextPromptValidator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            PromptValidator<TextResult> validator = async (ctx, result) =>
            {
                if (result.Value.Length <= 3)
                    result.Status = PromptStatus.TooSmall;
                await Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new TextPrompt(validator);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter some text.",
                            RetryPromptString = "Make sure the text is greater than three characters."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var textResult = (TextResult)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the text '{textResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("Make sure the text is greater than three characters.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTest();
        }
    }
}
