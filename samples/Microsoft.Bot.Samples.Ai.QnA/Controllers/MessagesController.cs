﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Ai.QnA.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        BotFrameworkAdapter _adapter;

        public MessagesController(IConfiguration configuration)
        {
            var qnaMiddlewareOptions = new QnAMakerMiddlewareOptions
            {
                // add subscription key and knowledge base id
                SubscriptionKey = "xxxxxx",
                KnowledgeBaseId = "xxxxxx"
            };
            var bot = new Builder.Bot(new BotFrameworkAdapter(configuration))
                // add QnA middleware 
                .Use(new QnAMakerMiddleware(qnaMiddlewareOptions, _httpClient));

            bot.OnReceive(BotReceiveHandler);

            _adapter = (BotFrameworkAdapter)bot.Adapter;
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message && context.Responses.Count == 0)
            {
                // add app logic when QnA Maker doesn't find an answer
                context.Reply("No good match found in the KB.");
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task Post([FromBody]Activity activity) => this.Response.StatusCode = await _adapter.Receive(this.Request.Headers, activity);
    }
}
