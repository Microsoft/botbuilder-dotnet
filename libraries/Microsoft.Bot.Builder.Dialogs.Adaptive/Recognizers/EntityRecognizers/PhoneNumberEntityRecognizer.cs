﻿using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes phone number input.
    /// </summary>
    public class PhoneNumberEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.PhoneNumberEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberEntityRecognizer"/> class.
        /// </summary>
        public PhoneNumberEntityRecognizer()
        {
        }

        /// <summary>
        /// Phone number recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizePhoneNumber(text, culture);
        }
    }
}
