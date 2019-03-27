﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static partial class Debugger
    {
        public interface IDebugger
        {
            Task StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken);
        }

        public static IDebugger GetDebugger(this ITurnContext context) =>
            context.TurnState.Get<IDebugger>() ?? NullDebugger.Instance;

        public static IDebugger GetDebugger(this DialogContext context) =>
            context.Context.GetDebugger();

        public static async Task DebuggerStepAsync(this DialogContext context, IDialog dialog, CancellationToken cancellationToken, [CallerMemberName]string memberName = null)
        {
            await context.GetDebugger().StepAsync(context, dialog, memberName, cancellationToken).ConfigureAwait(false);
        }

        public static async Task DebuggerStepAsync(this DialogContext context, IRecognizer recognizer, CancellationToken cancellationToken, [CallerMemberName]string memberName = null)
        {
            await context.GetDebugger().StepAsync(context, recognizer, memberName, cancellationToken).ConfigureAwait(false);
        }

        private sealed class NullDebugger : IDebugger
        {
            public static readonly IDebugger Instance = new NullDebugger();

            private NullDebugger()
            {
            }

            Task IDebugger.StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
