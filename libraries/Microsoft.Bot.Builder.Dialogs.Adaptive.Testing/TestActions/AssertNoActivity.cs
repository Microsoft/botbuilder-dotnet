﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Basic assertion TestAction, which validates assertions against a reply activity.
    /// </summary>
    [DebuggerDisplay("AssertNoActivity:{GetConditionDescription()}")]
    public class AssertNoActivity : TestAction
    {
        /// <summary>
        /// Kind for json serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.AssertNoActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertNoActivity"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public AssertNoActivity([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the description of this assertion.
        /// </summary>
        /// <value>Description of what this assertion is.</value>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the milliseconds to wait for a reply.
        /// </summary>
        /// <value>the milliseceods to wait.</value>
        [DefaultValue(3000)]
        [JsonProperty("timeout")]
        public uint Timeout { get; set; } = 3000;

        /// <summary>
        /// Gets the assertions.
        /// </summary>
        /// <value>The expressions for assertions.</value>
        [JsonProperty("assertions")]
        public List<string> Assertions { get; } = new List<string>();

        /// <summary>
        /// Gets the text to assert for an activity.
        /// </summary>
        /// <returns>String.</returns>
        public virtual string GetConditionDescription()
        {
            return Description ?? "No activity";
        }

        /// <inheritdoc/>
        public override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            if (adapter.ActiveQueue.Count > 0)
            {
                throw new Exception($"{GetConditionDescription()}");
            }

            return Task.CompletedTask;
        }
    }
}
