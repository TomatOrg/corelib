// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
//
//
// An exception for task cancellations.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an exception used to communicate task cancellation.
    /// </summary>
    public class TaskCanceledException : OperationCanceledException
    {
        private readonly Task? _canceledTask; // The task which has been canceled.

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskCanceledException"/> class.
        /// </summary>
        public TaskCanceledException() : base("A task was canceled.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskCanceledException"/>
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TaskCanceledException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskCanceledException"/>
        /// class with a specified error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TaskCanceledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskCanceledException"/>
        /// class with a specified error message, a reference to the inner exception that is the cause of
        /// this exception, and the <see cref="CancellationToken"/> that triggered the cancellation.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="token">The <see cref="CancellationToken"/> that triggered the cancellation.</param>
        public TaskCanceledException(string? message, Exception? innerException, CancellationToken token) : base(message, innerException, token)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Threading.Tasks.TaskCanceledException"/> class
        /// with a reference to the <see cref="System.Threading.Tasks.Task"/> that has been canceled.
        /// </summary>
        /// <param name="task">A task that has been canceled.</param>
        public TaskCanceledException(Task? task) :
            base("A task was canceled.", task != null ? task.CancellationToken : CancellationToken.None)
        {
            _canceledTask = task;
        }

        /// <summary>
        /// Gets the task associated with this exception.
        /// </summary>
        /// <remarks>
        /// It is permissible for no Task to be associated with a
        /// <see cref="System.Threading.Tasks.TaskCanceledException"/>, in which case
        /// this property will return null.
        /// </remarks>
        public Task? Task => _canceledTask;
    }
}
