﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackSelectOption
    {
        [JsonProperty(PropertyName = "text")]
        public object Text { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
