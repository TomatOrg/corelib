// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System
{
    /// <summary>
    /// The exception that is thrown when accessing an object that was disposed.
    /// </summary>
    public class ObjectDisposedException : InvalidOperationException
    {
        
        internal const string Generic = "";
        internal const string ReaderClosed = "Cannot read from a closed TextReader.";
        
        private readonly string? _objectName;

        // This constructor should only be called by the EE (COMPlusThrow)
        private ObjectDisposedException() :
            this(null, "Cannot access a disposed object.")
        {
        }

        public ObjectDisposedException(string? objectName) :
            this(objectName, "Cannot access a disposed object.")
        {
        }

        public ObjectDisposedException(string? objectName, string? message) : base(message)
        {
            _objectName = objectName;
        }

        public ObjectDisposedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the text for the message for this exception.
        /// </summary>
        public override string Message
        {
            get
            {
                string name = ObjectName;
                if (string.IsNullOrEmpty(name))
                {
                    return base.Message;
                }

                string objectDisposed = $"Object name: '{name}'.";
                return base.Message + Environment.NewLineConst + objectDisposed;
            }
        }

        public string ObjectName => _objectName ?? string.Empty;
    }
}
