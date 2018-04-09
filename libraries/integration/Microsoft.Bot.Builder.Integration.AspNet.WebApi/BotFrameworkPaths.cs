﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkPaths
    {
        public BotFrameworkPaths()
        {
            this.BasePath = "api/";
            this.MessagesPath = "messages";
            this.ProactiveMessagesPath = "messages/proactive";
        }

        /// <summary>
        /// Gets or sets the base path at which the bot's endpoints should be exposed.
        /// </summary>
        /// <value>
        /// A path that represents the base URL at which the bot should be exposed.
        /// </value>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which the bot framework messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A path that represents the URL at which the bot framework messages are expected to be delivered.
        /// </value>
        public string MessagesPath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which proactive messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A path that represents the base URL at which proactive messages.
        /// </value>
        /// <remarks>
        /// This path is only utilized if <see cref="BotFrameworkOptions.EnableProactiveMessages">the proactive messaging feature has been enabled</see>.
        /// </remarks>
        /// <seealso cref="BotFrameworkOptions.EnableProactiveMessages"/>
        /// <seealso cref="BotFrameworkConfigurationBuilder.EnableProactiveMessages(string)" />
        public string ProactiveMessagesPath { get; set; }
    }
}