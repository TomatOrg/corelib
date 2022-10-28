// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.IO
{
    public class IOException : SystemException
    {

        internal const string StreamTooLong = "Stream was too long.";
        internal const string SeekBeforeBegin = "An attempt was made to move the position before the beginning of the stream.";
        
        public IOException()
            : base("I/O error occurred.")
        {
        }

        public IOException(string? message)
            : base(message)
        {
        }

        public IOException(string? message, int hresult)
            : base(message)
        {
        }

        public IOException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
