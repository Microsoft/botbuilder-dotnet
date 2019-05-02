﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    /// <summary>
    /// Defines a template for an IMessageActivity using property semantics.
    /// </summary>
    /// <remarks>
    /// This defines an activity template which is driven by the types array and property name.
    /// </remarks>
    public class ActivityPropertyTemplate : ITemplate<Activity>
    {
        public ActivityPropertyTemplate()
        {
        }

        // Fixed text constructor
        public ActivityPropertyTemplate(IEnumerable<string> types, string property)
        {
            this.Types = types.ToList();
            this.Property = property ?? throw new ArgumentNullException(nameof(Property));
        }

        public List<string> Types { get; set; }

        public string Property { get; set; }

        public async Task<Activity> BindToData(ITurnContext context, object data)
        {
            IMessageActivityGenerator messageGenerator = context.TurnState.Get<IMessageActivityGenerator>();
            if (messageGenerator != null)
            {
                var result = await messageGenerator.Generate(
                    context.Activity.Locale,
                    inlineTemplate: null,
                    id: this.Property,
                    data: data,
                    tags: null,
                    types: this.Types.ToArray()).ConfigureAwait(false);

                return result as Activity;
            }

            // fallback to just text based LG if there is a language generator
            var message = Activity.CreateMessageActivity();

            ILanguageGenerator languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            if (languageGenerator != null)
            {
                var result = await languageGenerator.Generate(
                    context.Activity.Locale,
                    inlineTemplate: null,
                    id: this.Property,
                    data: data,
                    tags: null,
                    types: this.Types.ToArray()).ConfigureAwait(false);

                if (result != null)
                {
                    message.Text = result;
                    message.Speak = result;
                    return message as Activity;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return $"{nameof(ActivityPropertyTemplate)}({this.Property})";
        }
    }
}
