﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class DialogCommand : Dialog, IDialogDependencies
    {
        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnRunCommandAsync(dc, options);
        }

        protected abstract Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken));

        protected async Task<DialogTurnResult> EndParentDialogAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            PopCommands(dc);

            if (dc.Stack.Count > 0 || dc.ParentContext == null)
            {
                return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.ParentContext.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        protected async Task<DialogTurnResult> ReplaceParentDialogAsync(DialogContext dc, string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            PopCommands(dc);

            if (dc.Stack.Count > 0 || dc.ParentContext == null)
            {
                return await dc.ReplaceDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.ParentContext.ReplaceDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        protected async Task<DialogTurnResult> RepeatParentDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            PopCommands(dc);

            if (dc.Stack.Count > 0 || dc.ParentContext == null)
            {
                return await dc.ReplaceDialogAsync(dc.ActiveDialog.Id, options, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.ParentContext.ReplaceDialogAsync(dc.ParentContext.ActiveDialog.Id, options, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        protected async Task<DialogTurnResult> CancelAllParentDialogsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            PopCommands(dc);

            if (dc.Stack.Count > 0 || dc.ParentContext == null)
            {
                return await dc.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.ParentContext.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        private static void PopCommands(DialogContext dc)
        {
            // Pop all commands off the stack
            var i = dc.Stack.Count - 1;

            while (i > 0)
            {
                // Commands store the index of the state they are inheriting so we can tell a command
                // by looking to see if its state is of type int
                if (dc.Stack[i].State.GetType() == typeof(int))
                {
                    dc.Stack.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }
            }
        }

        public virtual List<IDialog> ListDependencies()
        {
            return new List<IDialog>();
        }
    }
}
