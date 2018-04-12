// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    public class TranslationMiddleware : IMiddleware
    {
        private readonly string[] _nativeLanguages;
        private readonly Translator _translator;
        private readonly Dictionary<string, List<string>> _patterns;
        private readonly Func<ITurnContext, Task<string>> _getUserLanguage;
        private readonly Func<ITurnContext, Task<bool>> _isUserLanguageChanged;
        private readonly bool _toUserLanguage;

        /// <summary>
        /// Constructor for automatic detection of user messages
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey"></param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, bool toUserLanguage = false)
        {
            AssertValidNativeLanguages(nativeLanguages);
            this._nativeLanguages = nativeLanguages;
            if (string.IsNullOrEmpty(translatorKey))
                throw new ArgumentNullException(nameof(translatorKey));
            this._translator = new Translator(translatorKey);
            _patterns = new Dictionary<string, List<string>>();
            _toUserLanguage = toUserLanguage;
        }


        /// <summary>
        /// Constructor for automatic of user messages and using templates
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey"></param>
        /// <param name="patterns">Dictionary with language as a key and list of patterns as value</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, bool toUserLanguage = false) : this(nativeLanguages, translatorKey, toUserLanguage)
        {
            this._patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
        }

        /// <summary>
        /// Constructor for developer defined detection of user messages and using templates
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey"></param>
        /// <param name="patterns">Dictionary with language as a key and list of patterns as value</param>
        /// <param name="getUserLanguage">Delegate for getting the user language</param>
        /// <param name="isUserLanguageChanged">Delegate for checking if  the user language is changed, returns true if the language was changed (implements logic to change language by intercepting the message)</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, Func<ITurnContext, Task<string>> getUserLanguage, Func<ITurnContext, Task<bool>> isUserLanguageChanged, bool toUserLanguage = false) : this(nativeLanguages, translatorKey, patterns, toUserLanguage)
        {
            this._getUserLanguage = getUserLanguage ?? throw new ArgumentNullException(nameof(getUserLanguage));
            this._isUserLanguageChanged = isUserLanguageChanged ?? throw new ArgumentNullException(nameof(isUserLanguageChanged));
        }

        private static void AssertValidNativeLanguages(string[] nativeLanguages)
        {
            if (nativeLanguages == null)
                throw new ArgumentNullException(nameof(nativeLanguages));
        }

        /// <summary>
        /// Incoming activity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>       
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            IMessageActivity message = context.Activity.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {

                    var languageChanged = false;

                    if (_isUserLanguageChanged != null)
                    {
                        languageChanged = await _isUserLanguageChanged(context);
                    }

                    if (!languageChanged)
                    {
                        // determine the language we are using for this conversation
                        var sourceLanguage = "";
                        var targetLanguage = "";
                        if (_getUserLanguage == null)
                            sourceLanguage = await _translator.Detect(message.Text); //awaiting user language detection using Microsoft Translator API.
                        else
                        {
                            sourceLanguage = await _getUserLanguage(context);
                        }
                        //check if the developer has added pattern list for the input source language
                        if (_patterns.ContainsKey(sourceLanguage) && _patterns[sourceLanguage].Count > 0)
                        {
                            //if we have a list of patterns for the current user's language send it to the translator post processor.
                            _translator.SetPostProcessorTemplate(_patterns[sourceLanguage]);
                        }
                        targetLanguage = (_nativeLanguages.Contains(sourceLanguage)) ? sourceLanguage : this._nativeLanguages.FirstOrDefault() ?? "en";
                        await TranslateMessageAsync(context, message, sourceLanguage, targetLanguage, _nativeLanguages.Contains(sourceLanguage)).ConfigureAwait(false);
                        if (_toUserLanguage)
                        {
                            context.OnSendActivities(async (newContext, activities, newNext) =>
                            {
                                //Translate messages sent to the user to user language
                                if (activities.Count > 0)
                                {
                                    IMessageActivity currentMEssageActivity = activities[0].AsMessageActivity();
                                    await TranslateMessageAsync(newContext, currentMEssageActivity, targetLanguage, sourceLanguage, false).ConfigureAwait(false);
                                    activities[0].Text = currentMEssageActivity.Text;
                                    await newNext();
                                }
                            });
                        }
                    }
                    else
                    {
                        // skip routing in case of user changed the language
                        return;
                    }
                }
            }
            await next().ConfigureAwait(false);
        }





        /// <summary>
        /// Translate .Text field of a message
        /// </summary>
        /// <param name="context"/>
        /// <param name="message"></param>
        /// <param name="sourceLanguage"/>
        /// <param name="targetLanguage"></param>
        /// <returns></returns>
        private async Task TranslateMessageAsync(ITurnContext context, IMessageActivity message, string sourceLanguage, string targetLanguage, bool InNativeLanguages)
        {
            if (!InNativeLanguages && sourceLanguage != targetLanguage)
            {
                // if we have text and a target language
                if (!String.IsNullOrWhiteSpace(message.Text) && !String.IsNullOrEmpty(targetLanguage))
                {
                    if (targetLanguage == sourceLanguage)
                        return;

                    var text = message.Text;
                    string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var translateResult = await this._translator.TranslateArray(lines, sourceLanguage, targetLanguage).ConfigureAwait(false);
                    text = String.Join("\n", translateResult);
                    message.Text = text;
                }
            }
        }
    }
}
