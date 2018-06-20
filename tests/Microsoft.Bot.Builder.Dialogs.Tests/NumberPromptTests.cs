﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class NumberPromptTests
    {
        [TestMethod]
        public async Task NumberPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<int>(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Bot received the number '42'.")
            .StartTest();
        }

        [TestMethod]
        public async Task NumberPromptRetry()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<int>(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter a number.",
                            RetryPromptString = "You must enter a number."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("hello")
            .AssertReply("You must enter a number.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTest();
        }

        [TestMethod]
        public async Task NumberPromptValidator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            PromptValidator<NumberResult<int>> validator = async (ctx, result) =>
            {
                if (result.Value < 0)
                    result.Status = PromptStatus.TooSmall;
                if (result.Value > 100)
                    result.Status = PromptStatus.TooBig;
                await Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<int>(Culture.English, validator);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter a number.",
                            RetryPromptString = "You must enter a positive number less than 100."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("150")
            .AssertReply("You must enter a positive number less than 100.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTest();
        }

        [TestMethod]
        public async Task FloatNumberPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<float>(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<float>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTest();
        }

        [TestMethod]
        public async Task LongNumberPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<long>(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<long>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Bot received the number '42'.")
            .StartTest();
        }

        [TestMethod]
        public async Task DoubleNumberPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<double>(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<double>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTest();
        }

        [TestMethod]
        public async Task DecimalNumberPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<decimal>(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<decimal>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTest();
        }
    }
}
