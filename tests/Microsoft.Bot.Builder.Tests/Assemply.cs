﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 100, Scope = ExecutionScope.ClassLevel)]
