﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Current dialog state.</param>
        /// <param name="onCompleted">An action to perform when the dialog completes, that is,
        /// when <see cref="EndAsync(IDictionary{string, object})"/> is called on the current context.</param>
        internal DialogContext(DialogSet dialogs, ITurnContext turnContext, DialogState state)
        {
            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            Context = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            Stack = state.DialogStack;
        }

        public DialogSet Dialogs { get; private set; }

        public ITurnContext Context { get; private set; }

        public List<DialogInstance> Stack { get; private set; }

        /// <summary>
        /// Gets the cached instance of the active dialog on the top of the stack or <c>null</c> if the stack is empty.
        /// </summary>
        /// <value>
        /// The cached instance of the active dialog on the top of the stack or <c>null</c> if the stack is empty.
        /// </value>
        public DialogInstance ActiveDialog
        {
            get
            {
                if (Stack.Any())
                {
                    return Stack.First();
                }

                return null;
            }
        }

        /// <summary>
        /// Pushes a new dialog onto the dialog stack.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to start.</param>
        /// <param name="options">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> BeginAsync(string dialogId, DialogOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            // Lookup dialog
            var dialog = Dialogs.Find(dialogId);
            if (dialog == null)
            {
                throw new Exception($"DialogContext.BeginAsync(): A dialog with an id of '{dialogId}' wasn't found.");
            }

            // Push new instance onto stack.
            var instance = new DialogInstance
            {
                Id = dialogId,
                State = new Dictionary<string, object>(),
            };

            Stack.Insert(0, instance);

            // Call dialogs BeginAsync() method.
            var turnResult = await dialog.DialogBeginAsync(this, options, cancellationToken).ConfigureAwait(false);
            return VerifyTurnResult(turnResult);
        }

        /// <summary>
        /// Helper function to simplify formatting the options for calling a prompt dialog. This helper will
        /// take a `PromptOptions` argument and then call[begin(context, dialogId, options)](#begin).
        /// </summary>
        /// <param name="dialogId">ID of the prompt to start.</param>
        /// <param name="options">Contains a Prompt, potentially a RetryPrompt and if using ChoicePrompt, Choices.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> PromptAsync(string dialogId, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await BeginAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Continues execution of the active dialog, if there is one, by passing the context object to
        /// its `Dialog.ContinueAsync()` method. You can check `context.responded` after the call completes
        /// to determine if a dialog was run and a reply was sent to the user.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> ContinueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check for a dialog on the stack
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogContext.ContinueAsync(): Can't continue dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Continue execution of dialog
                var turnResult = await dialog.DialogContinueAsync(this, cancellationToken).ConfigureAwait(false);
                return VerifyTurnResult(turnResult);
            }
            else
            {
                return new DialogTurnResult
                {
                    HasActive = false,
                    HasResult = false,
                };
            }
        }

        /// <summary>
        /// Ends a dialog by popping it off the stack and returns an optional result to the dialogs
        /// parent.The parent dialog is the dialog the started the on being ended via a call to
        /// either[begin()](#begin) or [prompt()](#prompt).
        /// The parent dialog will have its `Dialog.resume()` method invoked with any returned
        /// result. If the parent dialog hasn't implemented a `resume()` method then it will be
        /// automatically ended as well and the result passed to its parent. If there are no more
        /// parent dialogs on the stack then processing of the turn will end.
        /// </summary>
        /// <param name="result"> (Optional) result to pass to the parent dialogs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> EndAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Pop active dialog off the stack
            if (Stack.Any())
            {
                Stack.RemoveAt(0);
            }

            // Resume previous dialog
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogContext.EndAsync(): Can't resume previous dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Return result to previous dialog
                var turnResult = await dialog.DialogResumeAsync(this, DialogReason.EndCalled, result, cancellationToken).ConfigureAwait(false);
                return VerifyTurnResult(turnResult);
            }
            else
            {
                return new DialogTurnResult
                {
                    HasActive = false,
                    HasResult = result != null,
                    Result = result,
                };
            }
        }

        /// <summary>
        /// Deletes any existing dialog stack thus cancelling all dialogs on the stack.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The dialog context.</returns>
        public async Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (Stack.Any())
            {
                await EndActiveDialogAsync(DialogReason.CancelCalled, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Ends the active dialog and starts a new dialog in its place. This is particularly useful
        /// for creating loops or redirecting to another dialog.
        /// </summary>
        /// <param name="dialogId">ID of the new dialog to start.</param>
        /// <param name="options">(Optional) additional argument(s) to pass to the new dialog.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> ReplaceAsync(string dialogId, DialogOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Pop stack
            if (Stack.Any())
            {
                Stack.RemoveAt(0);
            }

            // Start replacement dialog
            return await BeginAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
        }

        public async Task RepromptAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check for a dialog on the stack
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogSet.RepromptAsync(): Can't find A dialog with an id of '{ActiveDialog.Id}'.");
                }

                // Ask dialog to re-prompt if supported
                await dialog.DialogRepromptAsync(Context, ActiveDialog, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task EndActiveDialogAsync(DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            var instance = ActiveDialog;
            if (instance != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(instance.Id);
                if (dialog != null)
                {
                    // Notify dialog of end
                    await dialog.DialogEndAsync(Context, instance, reason, cancellationToken).ConfigureAwait(false);
                }

                // Pop dialog off stack
                Stack.RemoveAt(0);
            }
        }

        private DialogTurnResult VerifyTurnResult(DialogTurnResult result)
        {
            result.HasActive = Stack.Count() > 0;
            if (result.HasActive)
            {
                result.HasResult = false;
                result.Result = null;
            }

            return result;
        }
    }
}
