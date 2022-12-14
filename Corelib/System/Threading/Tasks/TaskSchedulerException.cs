// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
//
//
// An exception for task schedulers.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Runtime.Serialization;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an exception used to communicate an invalid operation by a
    /// <see cref="System.Threading.Tasks.TaskScheduler"/>.
    /// </summary>
    public class TaskSchedulerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskSchedulerException"/> class.
        /// </summary>
        public TaskSchedulerException() : base("An exception was thrown by a TaskScheduler.") //
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskSchedulerException"/>
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TaskSchedulerException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskSchedulerException"/>
        /// class using the default error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TaskSchedulerException(Exception? innerException)
            : base("An exception was thrown by a TaskScheduler.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskSchedulerException"/>
        /// class with a specified error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TaskSchedulerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
