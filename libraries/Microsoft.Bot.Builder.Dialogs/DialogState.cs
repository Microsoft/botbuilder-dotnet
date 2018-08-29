﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogState
    {
        public DialogState()
        {
            DialogStack = new List<DialogInstance>();
        }

        public List<DialogInstance> DialogStack { get; }
    }
}
