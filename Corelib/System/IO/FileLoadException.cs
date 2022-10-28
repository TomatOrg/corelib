// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.IO
{
    public class FileLoadException : IOException
    {
        public string? FileName { get; }

        public FileLoadException()
            : base("Could not load the specified file.")
        {
        }

        public FileLoadException(string? message)
            : base(message)
        {
        }

        public FileLoadException(string? message, Exception? inner)
            : base(message, inner)
        {
        }

        public FileLoadException(string? message, string? fileName) : base(message)
        {
            FileName = fileName;
        }

        public FileLoadException(string? message, string? fileName, Exception? inner)
            : base(message, inner)
        {
            FileName = fileName;
        }


    }
}
