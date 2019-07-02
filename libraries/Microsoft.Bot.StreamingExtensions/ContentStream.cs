﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.StreamingExtensions.Payloads;

namespace Microsoft.Bot.StreamingExtensions
{
    internal class ContentStream : IContentStream
    {
        private readonly ContentStreamAssembler _assembler;
        private PayloadStream _stream;

        internal ContentStream(Guid id, ContentStreamAssembler assembler)
        {
            Id = id;
            _assembler = assembler ?? throw new ArgumentNullException();
        }

        public Guid Id { get; private set; }

        public string Type { get; set; }

        public int? Length { get; set; }

        public Stream GetStream()
        {
            if (_stream == null)
            {
                _stream = (PayloadStream)_assembler.GetPayloadStream();
            }

            return _stream;
        }

        public void Cancel()
        {
            _assembler.Close();
        }
    }
}
