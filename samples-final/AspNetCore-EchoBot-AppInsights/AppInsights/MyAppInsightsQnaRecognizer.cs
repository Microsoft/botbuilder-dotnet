﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;

namespace AspNetCore_EchoBot_With_AppInsights.AppInsights
{
    /// <summary>
    /// MyAppInsightsQnaRecognizer invokes the Qna Maker and logs some results into Application Insights.
    /// Logs the score, and (optionally) question
    /// 
    /// Along with Conversation and ActivityID.
    /// 
    /// The Custom Event name this logs is "QnaMessage"
    /// See <seealso cref="QnaMaker"/> for additional information.
    /// </summary>

    public class MyAppInsightsQnaMaker : QnAMaker
    {
        public static readonly string QnaMsgEvent = "QnaMessage";
        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        public MyAppInsightsQnaMaker(QnAMakerEndpoint endpoint, QnAMakerOptions options = null, bool logUserName = false, bool logOriginalMessage = false, HttpClient httpClient = null) : base(endpoint, options, httpClient)
        {
            LogUserName = logUserName;
            LogOriginalMessage = logOriginalMessage;
        }
        public bool LogUserName { get; }
        public bool LogOriginalMessage { get; }

        public new async Task<QueryResult[]> GetAnswersAsync(ITurnContext context)
        {
            // Call Qna Maker
            var queryResults = await base.GetAnswersAsync(context);

            // Find the Application Insights Telemetry Client
            if (queryResults != null && context.TurnState.TryGetValue(MyAppInsightsLoggerMiddleware.AppInsightsServiceKey, out var telemetryClient))
            {
                var telemetryProperties = new Dictionary<string, string>();
                var telemetryMetrics = new Dictionary<string, double>();

                // Make it so we can correlate our reports with Activity or Conversation
                telemetryProperties.Add(MyQnaConstants.ActivityIdProperty, context.Activity.Id);
                var conversationId = context.Activity.Conversation.Id;
                if (!string.IsNullOrEmpty(conversationId))
                {
                    telemetryProperties.Add(MyQnaConstants.ConversationIdProperty, conversationId);
                }

                // For some customers, logging original text name within Application Insights might be an issue  
                var text = context.Activity.Text;
                if (LogOriginalMessage && !string.IsNullOrWhiteSpace(text))
                {
                    telemetryProperties.Add(MyQnaConstants.OriginalQuestionProperty, text);
                }
                // For some customers, logging user name within Application Insights might be an issue 
                var userName = context.Activity.From.Name;
                if (LogUserName && !string.IsNullOrWhiteSpace(userName))
                {
                    telemetryProperties.Add(MyQnaConstants.UsernameProperty, userName);
                }

                // Fill in Qna Results (found or not)
                if (queryResults.Length > 0)
                {
                    var queryResult = queryResults[0];
                    telemetryProperties.Add(MyQnaConstants.QuestionProperty, string.Join(",", queryResult.Questions));
                    telemetryProperties.Add(MyQnaConstants.AnswerProperty, queryResult.Answer);
                    telemetryMetrics.Add(MyQnaConstants.ScoreProperty, (double)queryResult.Score);
                }
                else
                { 
                    telemetryProperties.Add(MyQnaConstants.QuestionProperty, "No Qna Question matched");
                    telemetryProperties.Add(MyQnaConstants.AnswerProperty, "No Qna Question matched");
                }

                // Track the event
                ((TelemetryClient)telemetryClient).TrackEvent(QnaMsgEvent, telemetryProperties, telemetryMetrics);
            }

            return queryResults;
        }
    }

    /// <summary>
    /// The Application Insights property names that we're logging.
    /// </summary>
    public static class MyQnaConstants
    {
        public const string ActivityIdProperty = "ActivityId";
        public const string UsernameProperty = "Username";
        public const string ConversationIdProperty = "ConversationId";
        public const string OriginalQuestionProperty = "OriginalQuestion";
        public const string QuestionProperty = "Question";
        public const string AnswerProperty = "Answer";
        public const string ScoreProperty = "Score";
        
    }
}

