// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class TestState : Dictionary<string, object>
    {
    }

    [TestClass]
    public class DialogTests
    {
        [TestMethod]
        public async Task NumberPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new NumberPrompt<int>(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var numberResult = (NumberResult<int>)dialogResult.Result;
                        await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Enter a number.");
                    }
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
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new NumberPrompt<int>(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var numberResult = (NumberResult<int>)dialogResult.Result;
                        await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Enter a number.", new PromptOptions { RetryPromptString = "You must enter a number." });
                    }
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
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new NumberPrompt<int>(Culture.English, validator));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var numberResult = (NumberResult<int>)dialogResult.Result;
                        await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Enter a number.", 
                            new PromptOptions
                            {
                                RetryPromptString = "You must enter a positive number less than 100."
                            });
                    }
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
        public async Task TextPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new TextPrompt());

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var textResult = (TextResult)dialogResult.Result;
                        await turnContext.SendActivity($"Bot received the text '{textResult.Value}'.");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Enter some text.");
                    }
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
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new TextPrompt(validator));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var textResult = (TextResult)dialogResult.Result;
                        await turnContext.SendActivity($"Bot received the text '{textResult.Value}'.");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Enter some text.", new PromptOptions
                        {
                            RetryPromptString = "Make sure the text is greater than three characters."
                        });
                    }
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

        [TestMethod]
        public async Task ConfirmPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new ConfirmPrompt(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        if (((ChoiceResult<bool>)dialogResult.Result).Value)
                        {
                            await turnContext.SendActivity("Confirmed.");
                        }
                        else
                        {
                            await turnContext.SendActivity("Not confirmed.");
                        }
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Please confirm.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm.")
            .Send("yes")
            .AssertReply("Confirmed.")
            .StartTest();
        }

        [TestMethod]
        public async Task ConfirmPromptRetry()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new ConfirmPrompt(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        if (((ChoiceResult<bool>)dialogResult.Result).Value)
                        {
                            await turnContext.SendActivity("Confirmed.");
                        }
                        else
                        {
                            await turnContext.SendActivity("Not confirmed.");
                        }
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Please confirm.",
                            new PromptOptions
                            {
                                RetryPromptString = "Please confirm, say 'yes' or 'no' or something like that."
                            });
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm.")
            .Send("lala")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that.")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTest();
        }
        
        [TestMethod]
        public async Task ChoicePrompt()
        {
            var options = new string[] { "blue", "yellow", "red" };
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new ChoicePrompt(Culture.English, options));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var selected = ((ChoiceResult<string>)dialogResult.Result).Value;
                        await turnContext.SendActivity($"Selected: {selected}");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Please select a color.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please select a color. 1. blue, 2. yellow, or 3. red")
            .Send("blue")
            .AssertReply("Selected: blue")
            .StartTest();
        }

        [TestMethod]
        public async Task ChoicePromptRetry()
        {
            var options = new string[] { "blue", "yellow", "red" };
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new ChoicePrompt(Culture.English, options));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var selected = ((ChoiceResult<string>)dialogResult.Result).Value;
                        await turnContext.SendActivity($"Selected: {selected}");
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "Select a color.",
                            new PromptOptions
                            {
                                RetryPromptString = "Please select a color."
                            });
                    }
                }
            })
            .Send("hello")
            .AssertReply("Select a color. 1. blue, 2. yellow, or 3. red")
            .Send("lala")
            .AssertReply("Please select a color. 1. blue, 2. yellow, or 3. red")
            .Send("seventh")
            .AssertReply("Please select a color. 1. blue, 2. yellow, or 3. red")
            .Send("2")
            .AssertReply("Selected: yellow")
            .StartTest();
        }

        [TestMethod]
        public async Task Waterfall()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall", Create_Waterfall1());

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        await turnContext.SendActivity((string)dialogResult.Result);
                    }
                    else
                    {
                        await dc.Begin("test-waterfall");
                    }
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .Send("hello")
            .AssertReply("step2")
            .Send("hello")
            .AssertReply("step3")
            .AssertReply("All Done!")
            .StartTest();
        }

        private static WaterfallStep[] Create_Waterfall1()
        {
            return new WaterfallStep[] {
                Waterfall1_Step1,
                Waterfall1_Step2,
                Waterfall1_Step3
            };
        }

        private static async Task Waterfall1_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1");
        }
        private static async Task Waterfall1_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2");
        }
        private static async Task Waterfall1_Step3(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step3");
            await dc.End("All Done!");
        }

        [TestMethod]
        public async Task WaterfallPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall", Create_Waterfall2());
                dialogs.Add("number", new NumberPrompt<int>(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        await turnContext.SendActivity((string)dialogResult.Result);
                    }
                    else
                    {
                        await dc.Begin("test-waterfall");
                    }
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .AssertReply("Enter a number.")
            .Send("hello again")
            .AssertReply("It must be a number")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("step2")
            .AssertReply("Enter a number.")
            .Send("apple")
            .AssertReply("It must be a number")
            .Send("orange")
            .AssertReply("It must be a number")
            .Send("64")
            .AssertReply("Thanks for '64'")
            .AssertReply("step3")
            .AssertReply("All Done!")
            .StartTest();
        }

        private static WaterfallStep[] Create_Waterfall2()
        {
            return new WaterfallStep[] {
                Waterfall2_Step1,
                Waterfall2_Step2,
                Waterfall2_Step3
            };
        }

        private static async Task Waterfall2_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1");
            await dc.Prompt("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private static async Task Waterfall2_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivity($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivity("step2");
            await dc.Prompt("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private static async Task Waterfall2_Step3(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivity($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivity("step3");
            await dc.End("All Done!");
        }

        [TestMethod]
        public async Task WaterfallNested()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall-a", Create_Waterfall3());
                dialogs.Add("test-waterfall-b", Create_Waterfall4());
                dialogs.Add("test-waterfall-c", Create_Waterfall5());

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        await turnContext.SendActivity((string)dialogResult.Result);
                    }
                    else
                    {
                        await dc.Begin("test-waterfall-a");
                    }
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .AssertReply("step1.1")
            .Send("hello")
            .AssertReply("step1.2")
            .Send("hello")
            .AssertReply("step2")
            .AssertReply("step2.1")
            .Send("hello")
            .AssertReply("step2.2")
            .AssertReply("All Done!")
            .StartTest();
        }

        private static WaterfallStep[] Create_Waterfall3()
        {
            return new WaterfallStep[] {
                Waterfall3_Step1,
                Waterfall3_Step2
            };
        }
        private static WaterfallStep[] Create_Waterfall4()
        {
            return new WaterfallStep[] {
                Waterfall4_Step1,
                Waterfall4_Step2
            };
        }

        private static WaterfallStep[] Create_Waterfall5()
        {
            return new WaterfallStep[] {
                Waterfall5_Step1,
                Waterfall5_Step2
            };
        }

        private static async Task Waterfall3_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1");
            await dc.Begin("test-waterfall-b");
        }
        private static async Task Waterfall3_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2");
            await dc.Begin("test-waterfall-c");
        }

        private static async Task Waterfall4_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1.1");
        }
        private static async Task Waterfall4_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1.2");
        }

        private static async Task Waterfall5_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2.1");
        }
        private static async Task Waterfall5_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2.2");
            await dc.End("All Done!");
        }
    }
}
