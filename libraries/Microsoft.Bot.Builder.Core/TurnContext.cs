﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public class TurnContext : ITurnContext
    {
        private readonly BotAdapter _adapter;
        private readonly Activity _activity;
        private bool _responded = false;

        private readonly IList<SendActivitiesHandler> _onSendActivities = new List<SendActivitiesHandler>();
        private readonly IList<UpdateActivityHandler> _onUpdateActivity = new List<UpdateActivityHandler>();
        private readonly IList<DeleteActivityHandler> _onDeleteActivity = new List<DeleteActivityHandler>();

        private readonly TurnContextServiceCollection _services = new TurnContextServiceCollection();

        public TurnContext(BotAdapter adapter, Activity activity)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onSendActivities.Add(handler);
            return this;
        }

        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onUpdateActivity.Add(handler);
            return this;
        }

        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onDeleteActivity.Add(handler);
            return this;
        }

        public BotAdapter Adapter => _adapter;

        public ITurnContextServiceCollection Services => _services;

        public Activity Activity => _activity;


        /// <summary>
        /// If true at least one response has been sent for the current turn of conversation.
        /// </summary>
        public bool Responded
        {
            get { return _responded; }
            set
            {
                if (value == false)
                {
                    throw new ArgumentException("TurnContext: cannot set 'responded' to a value of 'false'.");
                }
                _responded = true;
            }
        }

        public async Task<ResourceResponse> SendActivity(string textReplyToSend)
        {
            if (string.IsNullOrWhiteSpace(textReplyToSend))
                throw new ArgumentNullException(nameof(textReplyToSend)); 
            
            var activityToSend = new Activity(ActivityTypes.Message) { Text = textReplyToSend };

            return await SendActivity(activityToSend);
        }

        public async Task<ResourceResponse> SendActivity(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            ResourceResponse[] responses = await SendActivities(new IActivity[] { activity });
            if (responses == null || responses.Length == 0)
            {
                // It's possible an interceptor prevented the activity from having been sent. 
                // Just return an empty response in that case. 
                return new ResourceResponse();
            }
            else
            {
                return responses[0];
            }            
        }

        public async Task<ResourceResponse[]> SendActivities(IActivity[] activities)
        {
            // Bind the relevant Conversation Reference properties, such as URLs and 
            // ChannelId's, to the activities we're about to send. 
            ConversationReference cr = GetConversationReference(this._activity);
            foreach (Activity a in activities)
            {                
                ApplyConversationReference(a, cr);
            }

            // Convert the IActivities to Activies. 
            Activity[] activityArray = Array.ConvertAll(activities, (input) => (Activity)input);            

            // Create the list used by the recursive methods. 
            List<Activity> activityList = new List<Activity>(activityArray);

            // provide a variable to capture the set of responses returned from the adapter.
            ResourceResponse[] responses = null; 

            async Task ActuallySendStuff()
            {
                bool anythingToSend = false;
                if (activities.Count() > 0)
                    anythingToSend = true;

                // Send from the list, which may have been manipulated via the event handlers. 
                // Note that 'responses' was captured from the root of the call, and will be
                // returned to the original caller
                responses = await this.Adapter.SendActivities(this, activityList.ToArray());

                // If we actually sent something, set the flag. 
                if (anythingToSend)
                    this.Responded = true;                
            }

            await SendActivitiesInternal(activityList, _onSendActivities, ActuallySendStuff);

            return responses;
        }

        /// <summary>
        /// Replaces an existing activity. 
        /// </summary>
        /// <param name="activity">New replacement activity. The activity should already have it's ID information populated</param>        
        public async Task<ResourceResponse> UpdateActivity(IActivity activity)
        {
            Activity a = (Activity)activity;
            ResourceResponse response = null;

            async Task ActuallyUpdateStuff()
            {
                response = await this.Adapter.UpdateActivity(this, a);
            }

            await UpdateActivityInternal(a, _onUpdateActivity, ActuallyUpdateStuff);

            return response; 
        }

        public async Task DeleteActivity(string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));

            ConversationReference cr = GetConversationReference(this.Activity);
            cr.ActivityId = activityId;

            async Task ActuallyDeleteStuff()
            {
                await this.Adapter.DeleteActivity(this, cr);
            }

            await DeleteActivityInternal(cr, _onDeleteActivity, ActuallyDeleteStuff);
        }

        private async Task SendActivitiesInternal(
            List<Activity> activities,
            IEnumerable<SendActivitiesHandler> sendHandlers,
            Func<Task> callAtBottom)
        {
            if (activities == null)
                throw new ArgumentException(nameof(activities));
            if (sendHandlers == null)
                throw new ArgumentException(nameof(sendHandlers));

            if (sendHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                    await callAtBottom();

                return;
            }

            // Default to "No more Middleware after this"
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<SendActivitiesHandler> remaining = sendHandlers.Skip(1);
                await SendActivitiesInternal(activities, remaining, callAtBottom).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            SendActivitiesHandler caller = sendHandlers.First();
            await caller(this, activities, next);
        }

        private async Task UpdateActivityInternal(Activity activity,
            IEnumerable<UpdateActivityHandler> updateHandlers,
            Func<Task> callAtBottom)
        {
            BotAssert.ActivityNotNull(activity);
            if (updateHandlers == null)
                throw new ArgumentException(nameof(updateHandlers));

            if (updateHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                {
                    await callAtBottom();
                }

                return;
            }

            // Default to "No more Middleware after this"
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<UpdateActivityHandler> remaining = updateHandlers.Skip(1);
                await UpdateActivityInternal(activity, remaining, callAtBottom).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            UpdateActivityHandler toCall = updateHandlers.First();
            await toCall(this, activity, next);
        }

        private async Task DeleteActivityInternal(ConversationReference cr,
           IEnumerable<DeleteActivityHandler> updateHandlers,
           Func<Task> callAtBottom)
        {
            BotAssert.ConversationReferenceNotNull(cr);
            if (updateHandlers == null)
                throw new ArgumentException(nameof(updateHandlers));

            if (updateHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                {
                    await callAtBottom();
                }

                return;
            }

            // Default to "No more Middleware after this"
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<DeleteActivityHandler> remaining = updateHandlers.Skip(1);
                await DeleteActivityInternal(cr, remaining, callAtBottom).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            DeleteActivityHandler toCall = updateHandlers.First();
            await toCall(this, cr, next);
        }

        /// <summary>
        /// Creates a Conversation Reference from an Activity
        /// </summary>
        /// <param name="activity">The activity to update. Existing values in the Activity will be overwritten.</param>        
        public static ConversationReference GetConversationReference(Activity activity)
        {
            BotAssert.ActivityNotNull(activity);

            ConversationReference r = new ConversationReference
            {
                ActivityId = activity.Id,
                User = activity.From,
                Bot = activity.Recipient,
                Conversation = activity.Conversation,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl
            };

            return r;
        }

        /// <summary>
        /// Updates an activity with the delivery information from a conversation reference. Calling
        /// this after[getConversationReference()] (#getconversationreference) on an incoming activity 
        /// will properly address the reply to a received activity.
        /// </summary>
        /// <param name="a">Activity to copy delivery information to</param>
        /// <param name="r">Conversation reference containing delivery information</param>
        /// <param name="isIncoming">(Optional) flag indicating whether the activity is an incoming or outgoing activity. Defaults to `false` indicating the activity is outgoing.</param>
        public static Activity ApplyConversationReference(Activity a, ConversationReference r, bool isIncoming = false)
        {
            a.ChannelId = r.ChannelId;
            a.ServiceUrl = r.ServiceUrl;
            a.Conversation = r.Conversation;

            if (isIncoming)
            {
                a.From = r.User;
                a.Recipient = r.Bot;
                if (r.ActivityId != null)
                    a.Id = r.ActivityId;
            }
            else  // Outoing
            {
                a.From = r.Bot;
                a.Recipient = r.User;
                if (r.ActivityId != null)
                    a.ReplyToId = r.ActivityId;
            }
            return a;
        }
    }
}
