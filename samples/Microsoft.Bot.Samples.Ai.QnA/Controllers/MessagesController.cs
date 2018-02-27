﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Ai.QnA.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        static BotFrameworkAdapter adapter;

        public MessagesController(IConfiguration configuration)
        {
            if (adapter == null)
            {
                var qnaOptions = new QnAMakerMiddlewareOptions
                {
                    // add subscription key and knowledge base id
                    SubscriptionKey = "xxxxxx",
                    KnowledgeBaseId = "xxxxxx"
                };
                adapter = new BotFrameworkAdapter(configuration)
                    // add QnA middleware 
                    .Use(new QnAMakerMiddleware(qnaOptions, _httpClient));
            }
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
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivity(this.Request.Headers["Authorization"].FirstOrDefault(), activity, BotReceiveHandler);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
