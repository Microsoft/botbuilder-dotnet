﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    public static partial class DialogContextExtensions
    {
        const string TurnStateKey = "ActivityConsumed";

        public static DialogContext FromV3(IDialogContext context) => ((Context)context).V4;

        public static async Task<DialogTurnResult> BeginAsync<T>(this IDialogContext context, string dialogId, DialogOptions options, ResumeAfter<T> resume)
        {
            context.Wait<T>(resume);
            var dc = FromV3(context);
            return await dc.BeginAsync(dialogId, options);
        }

        public static bool GetActivityConsumed(this DialogContext dc)
        {
            var turn = dc.Context.TurnState;
            return turn.TryGetValue(TurnStateKey, out var value) && (bool)value == true;
        }

        public static void SetActivityConsumed(this DialogContext dc, bool consumed)
        {
            var turn = dc.Context.TurnState;
            turn[TurnStateKey] = consumed;
        }
    }
}
