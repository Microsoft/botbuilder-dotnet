﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DateTimePrompt : Prompt<DateTimeResult>
    {
        private DateTimePromptInternal _prompt;

        public DateTimePrompt(string culture, PromptValidator<DateTimeResult> validator = null)
        {
            _prompt = new DateTimePromptInternal(culture, validator);
        }

        protected override Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return dc.Context.SendActivity(PromptMessageFactory.CreateActivity(options, isRetry));
        }

        protected override async Task<DateTimeResult> OnRecognize(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return await _prompt.Recognize(dc.Context);
        }

    }
}
