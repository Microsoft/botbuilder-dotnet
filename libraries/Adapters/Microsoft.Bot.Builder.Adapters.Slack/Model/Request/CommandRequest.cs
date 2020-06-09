﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Represents a Slack Command request https://api.slack.com/interactivity/slash-commands
    /// </summary>
    public class CommandRequest
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "team_id")]
        public string TeamId { get; set; }

        [JsonProperty(PropertyName = "trigger_id")]
        public string TriggerId { get; set; }

        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }

        [JsonProperty(PropertyName = "response_url")]
        public string ResponseUrl { get; set; }
    }
}
