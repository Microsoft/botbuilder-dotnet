﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Connector;

namespace AlarmBot.TopicViews
{
    public class ShowAlarmsTopicView : TemplateRendererMiddleware
    {
        public ShowAlarmsTopicView() : base(new DictionaryRenderer(ReplyTemplates))
        {

        }

        // Template Ids
        public const string SHOWALARMS = "ShowAlarmsTopic.ShowAlarms";

        public static IMessageActivity AlarmsCard(IBotContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            IMessageActivity activity = ((Activity)context.Request).CreateReply(message);
            var card = new AdaptiveCard();
            card.Body.Add(new TextBlock() { Text = title, Size = TextSize.Large, Wrap = true, Weight = TextWeight.Bolder });
            if (message != null)
                card.Body.Add(new TextBlock() { Text = message, Wrap = true });
            if (alarms.Any())
            {
                FactSet factSet = new FactSet();
                foreach (var alarm in alarms)
                    factSet.Facts.Add(new AdaptiveCards.Fact(alarm.Title, alarm.Time.Value.ToString("f")));
                card.Body.Add(factSet);
            }
            else
                card.Body.Add(new TextBlock() { Text = "There are no alarms defined", Weight = TextWeight.Lighter });
            activity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            return activity;
        }


        /// <summary>
        /// Language dictionary of template functions
        /// </summary>
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            ["default"] = new TemplateIdMap
                {
                    { SHOWALARMS, (context, data) => AlarmsCard(context, data, "Alarms", null) }
                }
        };


    }
}
