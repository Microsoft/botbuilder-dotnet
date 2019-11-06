﻿using System.Data;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogPath
    {
        /// <summary>
        /// Counter of emitted events.
        /// </summary>
        public const string EventCounter = "dialog.eventCounter";

        /// <summary>
        /// Information for tracking memory changes.
        /// </summary>
        public const string Tracker = "dialog.tracker";

        /// <summary>
        /// Information for tracking when trigger handlers last ran.
        /// </summary>
        public const string ConditionTracker = Tracker + ".conditions";

        /// <summary>
        /// Currently expected properties.
        /// </summary>
        public const string ExpectedProperties = "dialog.expectedProperties";

        /// <summary>
        /// Last surfaced event from a form dialog.
        /// </summary>
        public const string LastEvent = "dialog.LastEvent";

        /// <summary>
        /// Currently required properties.
        /// </summary>
        public const string RequiredProperties = "dialog.requiredProperties";
    }
}
