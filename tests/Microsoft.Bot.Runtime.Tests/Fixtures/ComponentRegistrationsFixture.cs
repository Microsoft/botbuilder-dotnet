﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace Microsoft.Bot.Runtime.Tests.Fixtures
{
    public class ComponentRegistrationsFixture : IDisposable
    {
        public ComponentRegistrationsFixture()
        {
            ComponentRegistrations.Add();
        }

        public void Dispose()
        {
        }
    }
}
