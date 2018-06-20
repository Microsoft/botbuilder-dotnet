// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class WaterfallTests
    {
        [TestMethod]
        public async Task Waterfall()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var waterfall = new Waterfall(new WaterfallStep[]
                {
                    async (dc, args, next) => { await dc.Context.SendActivity("step1"); },
                    async (dc, args, next) => { await dc.Context.SendActivity("step2"); },
                    async (dc, args, next) => { await dc.Context.SendActivity("step3"); },
                });

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);

                var dialogCompletion = await waterfall.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await waterfall.Begin(turnContext, state);
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .Send("hello")
            .AssertReply("step2")
            .Send("hello")
            .AssertReply("step3")
            .StartTest();
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

                if (!turnContext.Responded)
                {
                    await dc.Begin("test-waterfall");
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
            await dc.End(new Dictionary<string, object> { { "Value", "All Done!" } });
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

                if (!turnContext.Responded)
                {
                    await dc.Begin("test-waterfall-a");
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
            await dc.End();
        }
    }
}
