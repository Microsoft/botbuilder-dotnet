﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// Defines a implementation of SkillConversationIdFactoryBase which uses IStorage to persist SkillConversationReference.
    /// </summary>
    public class SkillConversationReferenceStorage : SkillConversationIdFactoryBase
    {
        private readonly IStorage _storage;

        public SkillConversationReferenceStorage(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Creates a conversation id for a skill conversation.
        /// </summary>
        /// <param name="options">A <see cref="SkillConversationIdFactoryOptions"/> instance containing parameters for creating the conversation ID.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A unique conversation ID used to communicate with the skill.</returns>
        /// <remarks>
        /// It should be possible to use the returned string on a request URL and it should not contain special characters. 
        /// </remarks>
        public override async Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
        {
            var skillState = new SkillConversationReference
            {
                Id = Guid.NewGuid().ToString("n"),
                ConversationReference = options.Activity.GetConversationReference(),
                OAuthScope = options.FromBotOAuthScope
            };

            var changes = new Dictionary<string, object>();
            changes[skillState.Id] = skillState;
            await _storage.WriteAsync(changes, cancellationToken).ConfigureAwait(false);

            return skillState.Id;
        }

        /// <summary>
        /// Gets the <see cref="ConversationReference"/> created using <see cref="CreateSkillConversationIdAsync(Microsoft.Bot.Schema.ConversationReference,System.Threading.CancellationToken)"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync(Microsoft.Bot.Schema.ConversationReference,System.Threading.CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The caller's <see cref="ConversationReference"/> for a skillConversationId. null if not found.</returns>
        [Obsolete("Method is deprecated, please use GetSkillConversationReferenceAsync() instead.", false)]
        public override async Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            var result = await GetSkillConversationReferenceAsync(skillConversationId, cancellationToken).ConfigureAwait(false);
            if (result != null)
            {
                return result.ConversationReference;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="SkillConversationReference"/> used during <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The caller's <see cref="ConversationReference"/> for a skillConversationId, with originatingAudience. Null if not found.</returns>
        public override async Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            var results = await _storage.ReadAsync(new[] { skillConversationId }, cancellationToken).ConfigureAwait(false);
            if (results.TryGetValue(skillConversationId, out var reference))
            {
                if (reference is SkillConversationReference scr)
                {
                    return scr;
                }

                return JObject.FromObject(reference).ToObject<SkillConversationReference>();
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="SkillConversationReference"/> used during <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationReference">A skill conversation reference.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>task.</returns>
        public override async Task SaveSkillConversationReferenceAsync(SkillConversationReference skillConversationReference, CancellationToken cancellationToken)
        {
            var changes = new Dictionary<string, object>();
            changes[skillConversationReference.Id] = skillConversationReference;
            await _storage.WriteAsync(changes, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a <see cref="SkillConversationReference"/>.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            await _storage.DeleteAsync(new[] { skillConversationId }, cancellationToken).ConfigureAwait(false);
        }
    }
}
