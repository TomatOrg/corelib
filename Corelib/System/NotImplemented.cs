// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System
{
    //
    // Support for tooling-friendly NotImplementedExceptions.
    //
    public static class NotImplemented
    {
        /// <summary>
        /// Permanent NotImplementedException with no message shown to user.
        /// </summary>
        public static Exception ByDesign => new NotImplementedException();

        /// <summary>
        /// Permanent NotImplementedException with localized message shown to user.
        /// </summary>
        public static Exception ByDesignWithMessage(string message)
        {
            return new NotImplementedException(message);
        }
    }
}
