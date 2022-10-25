// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*============================================================
**
**
**
** Purpose: Exception for cancelled IO requests.
**
**
===========================================================*/

using System.Runtime.Serialization;
using System.Threading;

namespace System
{
    public class OperationCanceledException : SystemException
    {
        private CancellationToken _cancellationToken;

        public CancellationToken CancellationToken
        {
            get => _cancellationToken;
            private set => _cancellationToken = value;
        }

        public OperationCanceledException()
            : base("The operation was canceled.")
        {
        }

        public OperationCanceledException(string? message)
            : base(message)
        {
        }

        public OperationCanceledException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }


        public OperationCanceledException(CancellationToken token)
            : this()
        {
            CancellationToken = token;
        }

        public OperationCanceledException(string? message, CancellationToken token)
            : this(message)
        {
            CancellationToken = token;
        }

        public OperationCanceledException(string? message, Exception? innerException, CancellationToken token)
            : this(message, innerException)
        {
            CancellationToken = token;
        }
    }
}
