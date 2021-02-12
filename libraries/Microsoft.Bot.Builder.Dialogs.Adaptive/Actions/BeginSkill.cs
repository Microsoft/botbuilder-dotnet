﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Begin a Skill.
    /// </summary>
    public class BeginSkill : SkillDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BeginSkill";

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginSkill"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public BeginSkill([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(new SkillDialogOptions())
        {
            // NOTE: We are passing an unconfigured SkillDialogOptions so that we can bind 
            // intiialize the this.DialogOptions later when we are in a turncontext with access to the settings object.

            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have the new dialog should process the activity.
        /// </summary>
        /// <value>
        /// The default for this will be true, which means the new dialog will use the <see cref="Activity"/> to start the conversation with the skill.
        /// You can set this to false to dispatch the activity in the current turn context to the skill.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("activityProcessed")]
        public BoolExpression ActivityProcessed { get; set; } = true;

        /// <summary>
        /// Gets or sets the property path to store the dialog result in.
        /// </summary>
        /// <value>
        /// The property path to store the dialog result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft App ID that will be calling the skill.
        /// </summary>
        /// <value>
        /// The Microsoft App ID that will be calling the skill.
        /// </value>
        [JsonProperty("botId")]
        public StringExpression BotId { get; set; } = "=settings.MicrosoftAppId";

        /// <summary>
        /// Gets or sets the callback Url for the skill host.
        /// </summary>
        /// <value>
        /// The callback Url for the skill host.
        /// </value>
        [JsonProperty("skillHostEndpoint")]
        public StringExpression SkillHostEndpoint { get; set; } = "=settings.SkillHostEndpoint";

        /// <summary>
        /// Gets or sets the Microsoft App ID for the skill.
        /// </summary>
        /// <value>
        /// The Microsoft App ID for the skill.
        /// </value>
        [JsonProperty("skillAppId")]
        public StringExpression SkillAppId { get; set; }

        /// <summary>
        /// Gets or sets the /api/messages endpoint for the skill.
        /// </summary>
        /// <value>
        /// The /api/messages endpoint for the skill.
        /// </value>
        [JsonProperty("skillEndpoint")]
        public StringExpression SkillEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Connection Name, that would be used to perform Single SignOn with a skill.
        /// </summary>
        /// <value>
        /// The OAuth Connection Name for the Parent Bot.
        /// </value>
        [JsonProperty("connectionName")]
        public StringExpression ConnectionName { get; set; } = "=settings.connectionName";

        /// <summary>
        /// Gets or sets template for the activity.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

        /// <summary>
        /// Gets or sets the delivery mode to use for communication with the skill.
        /// </summary>
        /// <value>
        /// A deliveryMode constant [normal|expectReplies].
        /// </value>
        [JsonProperty("deliveryMode")]
        public StringExpression DeliveryMode { get; set; } = DeliveryModes.Normal;

        /// <summary>
        /// Gets or sets interruption policy. 
        /// </summary>
        /// <value>
        /// Bool or expression which evaluates to bool.
        /// </value>
        [JsonProperty("allowInterruptions")]
        public BoolExpression AllowInterruptions { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            InitializeDialogOptions(dc);

            // Get the activity to send to the skill.
            Activity activity = null;
            if (Activity != null && ActivityProcessed.GetValue(dc.State))
            {
                // The parent consumed the activity in context, use the Activity property to start the skill.
                activity = await Activity.BindAsync(dc, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Call the base to invoke the skill
            // (If the activity has not been processed, send the turn context activity to the skill (pass through)). 
            return await base.BeginDialogAsync(dc, new BeginSkillDialogOptions { Activity = activity ?? dc.Context.Activity }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // Capture the result of the dialog if the property is set
                if (ResultProperty != null)
                {
                    dc.State.SetValue(ResultProperty.GetValue(dc.State), dc.Context.Activity.Value);
                }
            }

            return base.ContinueDialogAsync(dc, cancellationToken);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            var appId = SkillAppId?.ToString() ?? string.Empty;
            string activity;

            if (Activity is ActivityTemplate at)
            {
                activity = StringUtils.Ellipsis(at.Template.Trim(), 30);
            }
            else
            {
                activity = StringUtils.Ellipsis(Activity?.ToString().Trim(), 30);
            }

            return $"{GetType().Name}['{appId}','{activity}']";
        }

        /// <summary>
        /// Called before an event is bubbled to its parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected override async Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            if (e.Name == DialogEvents.ActivityReceived && dc.Context.Activity.Type == ActivityTypes.Message)
            {
                // Ask parent to perform recognition
                await dc.Parent.EmitEventAsync(AdaptiveEvents.RecognizeUtterance, value: dc.Context.Activity, bubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Should we allow interruptions
                var canInterrupt = true;
                if (this.AllowInterruptions != null)
                {
                    var (allowInterruptions, error) = this.AllowInterruptions.TryGetValue(dc.State);
                    canInterrupt = error == null && allowInterruptions;
                }

                // Stop bubbling if interruptions ar NOT allowed
                return !canInterrupt;
            }

            return false;
        }

        /// <summary>
        /// Initializes the base class DialogOptions.
        /// </summary>
        /// <remarks>
        /// NOTE: We need to bootstrap the dialogoptions by evaluting expressions so that we can bind to settings, 
        /// but these options should be immutable after initialization. 
        /// </remarks>
        private void InitializeDialogOptions(DialogContext dc)
        {
            if (DialogOptions.Skill == null)
            {
                // initialize the dialog options with bound runtime settings.
                DialogOptions.Skill = new BotFrameworkSkill();
                DialogOptions.BotId = BotId.GetValue(dc.State);
                DialogOptions.SkillHostEndpoint = new Uri(SkillHostEndpoint.GetValue(dc.State));
                DialogOptions.ConversationIdFactory = dc.Context.TurnState.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
                DialogOptions.SkillClient = dc.Context.TurnState.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
                DialogOptions.ConversationState = dc.Context.TurnState.Get<ConversationState>() ?? throw new NullReferenceException($"Unable to get an instance of {nameof(ConversationState)} from TurnState.");
                DialogOptions.ConnectionName = ConnectionName.GetValue(dc.State);
                DialogOptions.DeliveryMode = DeliveryMode.GetValue(dc.State);
                DialogOptions.Skill.Id = DialogOptions.Skill.AppId = SkillAppId.GetValue(dc.State);
                DialogOptions.Skill.SkillEndpoint = new Uri(SkillEndpoint.GetValue(dc.State));
            }
        }
    }
}
