﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Expressions;

namespace Microsoft.Bot.Expressions.TriggerTrees.Tests
{
    public class Comparison
    {
        public Comparison(string type, object value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }

        public object Value { get; set; }
    }
}
