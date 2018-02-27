﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Helper Bot Controller for ASP.NET 
    /// </summary>
    public abstract class BotController : ApiController
    {
        protected readonly BotFrameworkAdapter _adapter;

        public BotController(BotFrameworkAdapter adapter)
        {
            this._adapter = adapter;
        }

        protected abstract Task OnReceiveActivity(IBotContext context);

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.ProcessActivity(this.Request.Headers.Authorization?.Parameter, activity, OnReceiveActivity);
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
