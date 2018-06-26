﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{

    public class Transcript
    {
        /// <summary>
        /// ChannelId that the transcript was taken from
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Conversation Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date conversation was started
        /// </summary>
        public DateTimeOffset Created { get; set; }
    }

    /// <summary>
    /// Page of results from an enumeration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Page of items
        /// </summary>
        public T[] Items { get; set; } = new T[0];

        /// <summary>
        /// Token used to page through multiple pages
        /// </summary>
        public string ContinuationToken { get; set; }
    }

    /// <summary>
    /// Transcript logger stores activities for conversations for recall
    /// </summary>
    public interface ITranscriptStore : ITranscriptLogger
    {
        /// <summary>
        /// Get activities for a conversation (Aka the transcript)
        /// </summary>
        /// <param name="channelId">Channel id</param>
        /// <param name="conversationId">Conversation id</param>
        /// <param name="continuationToken">continuatuation token to page through results</param>
        /// <param name="startDate">Earliest time to include.</param>
        /// <returns>Enumeration over the recorded activities.</returns>
        Task<PagedResult<IActivity>> GetTranscriptActivities(string channelId, string conversationId, string continuationToken=null, DateTime startDate = default(DateTime));

        /// <summary>
        /// List conversations in the channelId
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken">continuation token to get next page of results</param>
        /// <returns></returns>
        Task<PagedResult<Transcript>> ListTranscripts(string channelId, string continuationToken=null);

        /// <summary>
        /// Delete a specific conversation and all of it's activities
        /// </summary>
        /// <param name="channelId">Channel where conversation took place.</param>
        /// <param name="conversationId">Id of conversation to delete.</param>
        /// <returns>Task.</returns>
        Task DeleteTranscript(string channelId, string conversationId);
    }
}
