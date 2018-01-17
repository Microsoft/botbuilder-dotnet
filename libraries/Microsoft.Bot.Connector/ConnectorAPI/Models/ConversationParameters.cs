// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Microsoft.Bot.Connector
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;

    /// <summary>
    /// Parameters for creating a new conversation
    /// </summary>
    public partial class ConversationParameters
    {
        /// <summary>
        /// Initializes a new instance of the ConversationParameters class.
        /// </summary>
        public ConversationParameters() { }

        /// <summary>
        /// Initializes a new instance of the ConversationParameters class.
        /// </summary>
        public ConversationParameters(bool? isGroup = default(bool?), ChannelAccount bot = default(ChannelAccount), IList<ChannelAccount> members = default(IList<ChannelAccount>), string topicName = default(string), Activity activity = default(Activity), object channelData = default(object))
        {
            IsGroup = isGroup;
            Bot = bot;
            Members = members;
            TopicName = topicName;
            Activity = activity;
            ChannelData = channelData;
        }

        /// <summary>
        /// IsGroup
        /// </summary>
        [JsonProperty(PropertyName = "isGroup")]
        public bool? IsGroup { get; set; }

        /// <summary>
        /// The bot address for this conversation
        /// </summary>
        [JsonProperty(PropertyName = "bot")]
        public ChannelAccount Bot { get; set; }

        /// <summary>
        /// Members to add to the conversation
        /// </summary>
        [JsonProperty(PropertyName = "members")]
        public IList<ChannelAccount> Members { get; set; }

        /// <summary>
        /// (Optional) Topic of the conversation (if supported by the channel)
        /// </summary>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>
        /// (Optional) When creating a new conversation, use this activity as
        /// the intial message to the conversation
        /// </summary>
        [JsonProperty(PropertyName = "activity")]
        public Activity Activity { get; set; }

        /// <summary>
        /// Channel specific payload for creating the conversation
        /// </summary>
        [JsonProperty(PropertyName = "channelData")]
        public object ChannelData { get; set; }

    }
}
