// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.IO
{
    public class EndOfStreamException : IOException
    {
        public EndOfStreamException()
            : base("Attempted to read past the end of the stream.")
        {
        }

        public EndOfStreamException(string? message)
            : base(message)
        {
        }

        public EndOfStreamException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
