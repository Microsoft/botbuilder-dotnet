﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Simplified.Asp
{
    public class BotController : Controller
    {
        BotFrameworkAdapter _adapter;

        public BotController(Builder.Bot bot)
        {
            _adapter = (BotFrameworkAdapter)bot.Adapter;
            bot.OnReceive(BotReceiveHandler);
        }

        private async Task BotReceiveHandler(IBotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    await ReceiveMessage(context, context.Request.AsMessageActivity());
                    break;
                case ActivityTypes.ConversationUpdate:
                    await ReceiveConversationUpdate(context, context.Request.AsConversationUpdateActivity());
                    break;
                case ActivityTypes.ContactRelationUpdate:
                    await ReceiveContactRelationUpdate(context, context.Request.AsContactRelationUpdateActivity());
                    break;
                case ActivityTypes.EndOfConversation:
                    await ReceiveEndOfConversation(context, context.Request.AsEndOfConversationActivity());
                    break;
                case ActivityTypes.Invoke:
                    await ReceiveInvoke(context, context.Request.AsInvokeActivity());
                    break;
                // the following Activity types are defined in the protocol but are not sent by any channels currently
                case ActivityTypes.InstallationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.Event:
                case ActivityTypes.MessageUpdate:
                case ActivityTypes.MessageDelete:
                case ActivityTypes.MessageReaction:
                case ActivityTypes.Suggestion:
                default:
                    throw new InvalidOperationException(context.Request.Type);
            }
        }

        protected virtual Task ReceiveInvoke(IBotContext context, IInvokeActivity invokeActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveEndOfConversation(IBotContext context, IEndOfConversationActivity endOfConversationActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveContactRelationUpdate(IBotContext context, IContactRelationUpdateActivity contactRelationUpdateActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveMessage(IBotContext context, IMessageActivity activity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveConversationUpdate(IBotContext context, IConversationUpdateActivity activity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveUnknown(IBotContext context)
        {
            return Task.CompletedTask;
        }

        [HttpPost]
        public Task Post() => _adapter.Receive(this.Request);
    }
}
