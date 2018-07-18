﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Provides access to the Microsoft Translator Text API.
    /// Uses api key and detect input language translate single sentence or array of sentences then apply translation post processing fix.
    /// </summary>
    public class Translator
    {
        private static readonly HttpClient DefaultHttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };
        private readonly AzureAuthToken _authToken;
        private HttpClient _httpClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Translator"/> class.
        /// </summary>
        /// <param name="apiKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="customHttpClient">An alternate HTTP client to use.</param>
        public Translator(string apiKey, HttpClient customHttpClient = null)
        {
            _httpClient = customHttpClient ?? DefaultHttpClient;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            _authToken = new AzureAuthToken(apiKey);
        }

        /// <summary>
        /// Detects the language of the input text.
        /// </summary>
        /// <param name="textToDetect">The text to translate.</param>
        /// <returns>The language identifier.</returns>
        public async Task<string> DetectAsync(string textToDetect)
        {
            textToDetect = PreprocessMessage(textToDetect);
            var url = "http://api.microsofttranslator.com/v2/Http.svc/Detect";
            var query = $"?text={System.Net.WebUtility.UrlEncode(textToDetect)}";

            using (var request = new HttpRequestMessage())
            {
                var accessToken = await _authToken.GetAccessTokenAsync().ConfigureAwait(false);
                request.Headers.Add("Authorization", accessToken);
                request.RequestUri = new Uri(url + query);
                using (var response = await _httpClient.SendAsync(request))
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return "ERROR: " + result;
                    }

                    var detectedLang = XElement.Parse(result).Value;
                    return detectedLang;
                }
            }
        }

        /// <summary>
        /// Translates a single message from a source language to a target language.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <param name="from">The language code of the translation text. For example, "en" for English.</param>
        /// <param name="to">The language code to translate the text into.</param>
        /// <returns>The translated document.</returns>
        public async Task<TranslatedDocument> TranslateAsync(string textToTranslate, string from, string to)
        {
            var currentTranslatedDocument = new TranslatedDocument(textToTranslate);
            PreprocessMessage(currentTranslatedDocument.SourceMessage, out var processedText, out var literanlNoTranslateList);
            currentTranslatedDocument.SourceMessage = processedText;
            currentTranslatedDocument.LiteranlNoTranslatePhrases = literanlNoTranslateList;

            var url = "http://api.microsofttranslator.com/v2/Http.svc/Translate";
            var query = $"?text={System.Net.WebUtility.UrlEncode(textToTranslate)}" +
                                 $"&from={from}" +
                                 $"&to={to}";

            using (var request = new HttpRequestMessage())
            {
                var accessToken = await _authToken.GetAccessTokenAsync().ConfigureAwait(false);
                request.Headers.Add("Authorization", accessToken);
                request.RequestUri = new Uri(url + query);
                using (var response = await _httpClient.SendAsync(request))
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ArgumentException(result);
                    }

                    var translatedText = XElement.Parse(result).Value.Trim();

                    currentTranslatedDocument.TargetMessage = translatedText;
                    return currentTranslatedDocument;
                }
            }
        }

        /// <summary>
        /// Translates an array of strings from a source language to a target language.
        /// </summary>
        /// <param name="translateArraySourceTexts">The strings to translate.</param>
        /// <param name="from">The language code of the translation text. For example, "en" for English.</param>
        /// <param name="to">The language code to translate the text into.</param>
        /// <returns>An array of the translated documents.</returns>
        public async Task<List<TranslatedDocument>> TranslateArrayAsync(string[] translateArraySourceTexts, string from, string to)
        {
            var translatedDocuments = new List<TranslatedDocument>();
            var uri = "https://api.microsofttranslator.com/v2/Http.svc/TranslateArray2";
            for (var srcTxtIndx = 0; srcTxtIndx < translateArraySourceTexts.Length; srcTxtIndx++)
            {
                // Check for literal tag in input user message
                var currentTranslatedDocument = new TranslatedDocument(translateArraySourceTexts[srcTxtIndx]);
                translatedDocuments.Add(currentTranslatedDocument);
                PreprocessMessage(currentTranslatedDocument.SourceMessage, out var processedText, out var literanlNoTranslateList);
                currentTranslatedDocument.SourceMessage = processedText;
                translateArraySourceTexts[srcTxtIndx] = processedText;
                currentTranslatedDocument.LiteranlNoTranslatePhrases = literanlNoTranslateList;
            }

            // body of http request
            var body = $"<TranslateArrayRequest>" +
                           "<AppId />" +
                           $"<From>{from}</From>" +
                           "<Options>" +
                           " <Category xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" >generalnn</Category>" +
                               "<ContentType xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">text/plain</ContentType>" +
                               "<ReservedFlags xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<State xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<Uri xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<User xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                           "</Options>" +
                           "<Texts>" +
                                   string.Join(string.Empty, translateArraySourceTexts.Select(s => $"<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">{SecurityElement.Escape(s)}</string>\n"))
                           + "</Texts>" +
                           $"<To>{to}</To>" +
                       "</TranslateArrayRequest>";

            var accessToken = await _authToken.GetAccessTokenAsync().ConfigureAwait(false);

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                request.Headers.Add("Authorization", accessToken);

                using (var response = await _httpClient.SendAsync(request))
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            Console.WriteLine("Request status is OK. Result of translate array method is:");
                            var doc = XDocument.Parse(responseBody);
                            var ns = XNamespace.Get("http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2");
                            var results = new List<string>();
                            var sentIndex = 0;
                            foreach (var xe in doc.Descendants(ns + "TranslateArray2Response"))
                            {
                                var currentTranslatedDocument = translatedDocuments[sentIndex];
                                currentTranslatedDocument.RawAlignment = xe.Element(ns + "Alignment").Value;
                                if (!string.IsNullOrEmpty(currentTranslatedDocument.RawAlignment))
                                {
                                    var alignments = currentTranslatedDocument.RawAlignment.Trim().Split(' ');
                                    currentTranslatedDocument.SourceTokens = PostProcessingUtilities.SplitSentence(currentTranslatedDocument.SourceMessage, alignments);
                                    currentTranslatedDocument.TranslatedTokens = PostProcessingUtilities.SplitSentence(xe.Element(ns + "TranslatedText").Value, alignments, false);
                                    currentTranslatedDocument.IndexedAlignment = PostProcessingUtilities.WordAlignmentParse(alignments, currentTranslatedDocument.SourceTokens, currentTranslatedDocument.TranslatedTokens);
                                }

                                sentIndex += 1;
                            }

                            return translatedDocuments;
                        default:
                            throw new Exception(response.ReasonPhrase);
                    }
                }
            }
        }

        /// <summary>
        /// Performs pre-processing to remove "literal" tags and flag sections of the text that will not be translated.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <param name="processedTextToTranslate">The processed text after removing the literal tags and other unwanted characters.</param>
        /// <param name="noTranslatePhrases">The extracted no translate phrases.</param>
        private void PreprocessMessage(string textToTranslate, out string processedTextToTranslate, out HashSet<string> noTranslatePhrases)
        {
            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " "); // used to remove multiple spaces in input user message
            var literalPattern = "<literal>(.*)</literal>";
            noTranslatePhrases = new HashSet<string>();
            var literalMatches = Regex.Matches(textToTranslate, literalPattern);
            if (literalMatches.Count > 0)
            {
                foreach (Match literalMatch in literalMatches)
                {
                    if (literalMatch.Groups.Count > 1)
                    {
                        noTranslatePhrases.Add("(" + literalMatch.Groups[1].Value + ")");
                    }
                }

                textToTranslate = Regex.Replace(textToTranslate, "</?literal>", " ");
            }

            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " ");
            processedTextToTranslate = textToTranslate;
        }

        /// <summary>
        /// Performs pre-processing to remove "literal" tags .
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        private string PreprocessMessage(string textToTranslate)
        {
            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " "); // used to remove multiple spaces in input user message
            var literalPattern = "<literal>(.*)</literal>";
            var literalMatches = Regex.Matches(textToTranslate, literalPattern);
            if (literalMatches.Count > 0)
            {
                textToTranslate = Regex.Replace(textToTranslate, "</?literal>", " ");
            }

            return Regex.Replace(textToTranslate, @"\s+", " ");
        }
    }
}
