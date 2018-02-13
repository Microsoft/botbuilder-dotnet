﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter _adapter;

        /// <summary>
        /// In this Echo Bot, a new instance of the Bot is created by the controller 
        /// on every incoming HTTP reques. The bot is constructed using the credentials
        /// found in the config file. Note that no credentials are needed if testing
        /// the bot locally using the emulator. 
        ///         
        /// The bot itself simply echoes any messages sent, using the OnReceive 
        /// handler. This handler checks the activity type, and returns
        /// back the sent text. 
        /// </summary>        
        public MessagesController(IConfiguration configuration)
        {
            var bot = new Builder.Bot(new BotFrameworkAdapter(new ConfigurationCredentialProvider(configuration)));
            _adapter = (BotFrameworkAdapter)bot.Adapter;
            bot.OnReceive(BotReceiveHandler);
        }

        private async Task BotReceiveHandler(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                context.Reply($"echo: {context.Request.AsMessageActivity().Text}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.Receive(this.Request.Headers, activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}