﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Translation.Tests
{
    [TestClass]
    public class TranslatorPostProcessorsTests
    {
        public string translatorKey = TestUtilities.GetKey("TRANSLATORKEY");


        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_PatternsTest_InvalidArguments()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = null;

            Assert.ThrowsException<ArgumentNullException>(() => new PatternsPostProcessor(patterns));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_PatternsTest_EmptyPatternsDictionary()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();

            Assert.ThrowsException<ArgumentException>(() => new PatternsPostProcessor(patterns));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest_EmptyLanguagePatternsData()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            //using an empty language list won't throw an exception, but it won't affect the post processing for this language
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> spanishPatterns = new List<string>();
            patterns.Add("es", spanishPatterns);

            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);
            var sentence = "mi perro se llama Enzo";

            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "es", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "es").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My dog's name is Enzo", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest_FrenchPatterns()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> frenchPatterns = new List<string> { "mon nom est (.+)" };
            patterns.Add("fr", frenchPatterns);


            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);

            var sentence = "mon nom est l'etat";
            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My name is l'etat", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest_FrenchPatternsWithMultipleSpaces()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> frenchPatterns = new List<string> { "mon nom est (.+)" };
            patterns.Add("fr", frenchPatterns);


            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);

            var sentence = "mon     nom     est    l'etat   ";
            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My name is l'etat", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest_FrenchPatternsWithNumbers()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> frenchPatterns = new List<string> { "mon nom est (.+)" };
            patterns.Add("fr", frenchPatterns);


            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);

            var sentence = "J'ai 25 ans et mon nom est l'etat";
            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("I am 25 years old and my name is l'etat", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest_SpanishPatterns()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> spanishPatterns = new List<string> { "perr[oa]" };
            patterns.Add("es", spanishPatterns);

            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);
            var sentence = "mi perro se llama Enzo";

            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "es", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "es").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My perro's name is Enzo", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_DictionaryTest_InvalidArguments()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            CustomDictionary userCustomDictonaries = null;
            Assert.ThrowsException<ArgumentNullException>(() => new CustomDictionaryPostProcessor(userCustomDictonaries));

        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest_EmptyCustomLanguageDictionaryData()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            CustomDictionary userCustomDictonaries = new CustomDictionary();
            IPostProcessor customDictionaryPostProcessor = new CustomDictionaryPostProcessor(userCustomDictonaries);

            var frenchSentence = "Je veux voir éclair";

            var translatedDocuments = await translator.TranslateArray(new string[] { frenchSentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            Assert.ThrowsException<ArgumentException>(() => customDictionaryPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_DictionaryTest_FrenchDictionary()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            CustomDictionary userCustomDictonary = new CustomDictionary();
            Dictionary<string, string> frenctDictionary = new Dictionary<string, string>
            {
                { "éclair", "eclairs tart" }
            };
            userCustomDictonary.AddNewLanguageDictionary("fr", frenctDictionary);
            IPostProcessor customDictionaryPostProcessor = new CustomDictionaryPostProcessor(userCustomDictonary);

            var frenchSentence = "Je veux voir éclair";

            var translatedDocuments = await translator.TranslateArray(new string[] { frenchSentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = customDictionaryPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("I want to see eclairs tart", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_DictionaryTest_ItalianDictionary()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            CustomDictionary userCustomDictonary = new CustomDictionary();

            Dictionary<string, string> italianDictionary = new Dictionary<string, string>
            {
                { "camera", "bedroom" },
                { "foto", "personal photo" }
            };
            userCustomDictonary.AddNewLanguageDictionary("it", italianDictionary);

            IPostProcessor customDictionaryPostProcessor = new CustomDictionaryPostProcessor(userCustomDictonary);

            var italianSentence = "Voglio fare una foto nella camera";

            var translatedDocuments = await translator.TranslateArray(new string[] { italianSentence }, "it", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = customDictionaryPostProcessor.Process(translatedDocuments[0], "it").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("I want to take a personal photo in the bedroom", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsAndDictionaryTest()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            //creating the patterns post processor
            List<IPostProcessor> attachedPostProcessors = new List<IPostProcessor>();
            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> frenchPatterns = new List<string> { "mon nom est (.+)" };

            patterns.Add("fr", frenchPatterns);
            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);

            //attaching patterns post processor to the list of post processors
            attachedPostProcessors.Add(patternsPostProcessor);

            //creating user custom dictionary post processor
            CustomDictionary userCustomDictonaries = new CustomDictionary();
            Dictionary<string, string> frenctDictionary = new Dictionary<string, string>
            {
                { "etat", "Eldad" }
            };
            userCustomDictonaries.AddNewLanguageDictionary("fr", frenctDictionary);
            IPostProcessor customDictionaryPostProcessor = new CustomDictionaryPostProcessor(userCustomDictonaries);

            //attaching user custom dictionary post processor to the list of post processors
            attachedPostProcessors.Add(customDictionaryPostProcessor);

            var sentence = "mon nom est etat";

            //translating the document
            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = null;

            //test the actual use case of the compined post processors together
            foreach (IPostProcessor postProcessor in attachedPostProcessors)
            {
                postProcessedMessage = postProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            }

            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My name is Eldad", postProcessedMessage);
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
