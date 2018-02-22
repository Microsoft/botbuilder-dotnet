﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461
{
    public class MessagesController : ApiController
    {
        private readonly BotFrameworkAdapter _adapter;

        public MessagesController(BotFrameworkAdapter adapter)
        {
            _adapter = adapter;
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            var msgActivity = context.Request.AsMessageActivity();
            if (msgActivity != null)
            {
                long turnNumber = context.State.Conversation["turnNumber"] ?? 0;
                context.State.Conversation["turnNumber"] = ++turnNumber;

                // calculate something for us to return
                int length = (msgActivity.Text ?? string.Empty).Length;

                // return our reply to the user
                context.Reply($"[{turnNumber}] You sent {msgActivity.Text} which was {length} characters");
                return Task.CompletedTask;
            }

            var convUpdateActivity = context.Request.AsConversationUpdateActivity();
            if (convUpdateActivity != null)
            {
                foreach (var newMember in convUpdateActivity.MembersAdded)
                {
                    if (newMember.Id != convUpdateActivity.Recipient.Id)
                    {
                        context.Reply("Hello and welcome to the echo bot.");
                    }
                }
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.ProcessActivty(this.Request.Headers.Authorization?.Parameter, activity, BotReceiveHandler);
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (UnauthorizedAccessException e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, e.Message);
            }
            catch (InvalidOperationException e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message);
            }
        }
    }
}