// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides support for creating and scheduling
    /// <see cref="System.Threading.Tasks.Task{TResult}">Task{TResult}</see> objects.
    /// </summary>
    /// <typeparam name="TResult">The type of the results that are available though
    /// the <see cref="System.Threading.Tasks.Task{TResult}">Task{TResult}</see> objects that are associated with
    /// the methods in this class.</typeparam>
    /// <remarks>
    /// <para>
    /// There are many common patterns for which tasks are relevant. The <see cref="TaskFactory{TResult}"/>
    /// class encodes some of these patterns into methods that pick up default settings, which are
    /// configurable through its constructors.
    /// </para>
    /// <para>
    /// A default instance of <see cref="TaskFactory{TResult}"/> is available through the
    /// <see cref="System.Threading.Tasks.Task{TResult}.Factory">Task{TResult}.Factory</see> property.
    /// </para>
    /// </remarks>
    public class TaskFactory<TResult>
    {
        // Member variables, DefaultScheduler, other properties and ctors
        // copied right out of TaskFactory...  Lots of duplication here...
        // Should we be thinking about a TaskFactoryBase class?

        // member variables
        private readonly CancellationToken m_defaultCancellationToken;
        private readonly TaskScheduler? m_defaultScheduler;
        private readonly TaskCreationOptions m_defaultCreationOptions;
        private readonly TaskContinuationOptions m_defaultContinuationOptions;

        private TaskScheduler DefaultScheduler => m_defaultScheduler ?? TaskScheduler.Current;

        // sister method to above property -- avoids a TLS lookup
        private TaskScheduler GetDefaultScheduler(Task? currTask)
        {
            if (m_defaultScheduler != null) return m_defaultScheduler;
            else if ((currTask != null)
                && ((currTask.CreationOptions & TaskCreationOptions.HideScheduler) == 0)
                )
                return currTask.ExecutingTaskScheduler!; // a "current" task must be executing, which means it must have a scheduler
            else return TaskScheduler.Default;
        }

        /* Constructors */

        /// <summary>
        /// Initializes a <see cref="TaskFactory{TResult}"/> instance with the default configuration.
        /// </summary>
        /// <remarks>
        /// This constructor creates a <see cref="TaskFactory{TResult}"/> instance with a default configuration. The
        /// <see cref="TaskCreationOptions"/> property is initialized to
        /// <see cref="System.Threading.Tasks.TaskCreationOptions.None">TaskCreationOptions.None</see>, the
        /// <see cref="TaskContinuationOptions"/> property is initialized to <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.None">TaskContinuationOptions.None</see>,
        /// and the <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> property is
        /// initialized to the current scheduler (see <see
        /// cref="System.Threading.Tasks.TaskScheduler.Current">TaskScheduler.Current</see>).
        /// </remarks>
        public TaskFactory()
        {
        }

        /// <summary>
        /// Initializes a <see cref="TaskFactory{TResult}"/> instance with the default configuration.
        /// </summary>
        /// <param name="cancellationToken">The default <see cref="CancellationToken"/> that will be assigned
        /// to tasks created by this <see cref="TaskFactory"/> unless another CancellationToken is explicitly specified
        /// while calling the factory methods.</param>
        /// <remarks>
        /// This constructor creates a <see cref="TaskFactory{TResult}"/> instance with a default configuration. The
        /// <see cref="TaskCreationOptions"/> property is initialized to
        /// <see cref="System.Threading.Tasks.TaskCreationOptions.None">TaskCreationOptions.None</see>, the
        /// <see cref="TaskContinuationOptions"/> property is initialized to <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.None">TaskContinuationOptions.None</see>,
        /// and the <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> property is
        /// initialized to the current scheduler (see <see
        /// cref="System.Threading.Tasks.TaskScheduler.Current">TaskScheduler.Current</see>).
        /// </remarks>
        public TaskFactory(CancellationToken cancellationToken)
        {
            m_defaultCancellationToken = cancellationToken;
        }

        /// <summary>
        /// Initializes a <see cref="TaskFactory{TResult}"/> instance with the specified configuration.
        /// </summary>
        /// <param name="scheduler">
        /// The <see cref="System.Threading.Tasks.TaskScheduler">
        /// TaskScheduler</see> to use to schedule any tasks created with this TaskFactory{TResult}. A null value
        /// indicates that the current TaskScheduler should be used.
        /// </param>
        /// <remarks>
        /// With this constructor, the
        /// <see cref="TaskCreationOptions"/> property is initialized to
        /// <see cref="System.Threading.Tasks.TaskCreationOptions.None">TaskCreationOptions.None</see>, the
        /// <see cref="TaskContinuationOptions"/> property is initialized to <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.None">TaskContinuationOptions.None</see>,
        /// and the <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> property is
        /// initialized to <paramref name="scheduler"/>, unless it's null, in which case the property is
        /// initialized to the current scheduler (see <see
        /// cref="System.Threading.Tasks.TaskScheduler.Current">TaskScheduler.Current</see>).
        /// </remarks>
        public TaskFactory(TaskScheduler? scheduler) // null means to use TaskScheduler.Current
        {
            m_defaultScheduler = scheduler;
        }

        /// <summary>
        /// Initializes a <see cref="TaskFactory{TResult}"/> instance with the specified configuration.
        /// </summary>
        /// <param name="creationOptions">
        /// The default <see cref="System.Threading.Tasks.TaskCreationOptions">
        /// TaskCreationOptions</see> to use when creating tasks with this TaskFactory{TResult}.
        /// </param>
        /// <param name="continuationOptions">
        /// The default <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> to use when creating continuation tasks with this TaskFactory{TResult}.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument or the <paramref name="continuationOptions"/>
        /// argument specifies an invalid value.
        /// </exception>
        /// <remarks>
        /// With this constructor, the
        /// <see cref="TaskCreationOptions"/> property is initialized to <paramref name="creationOptions"/>,
        /// the
        /// <see cref="TaskContinuationOptions"/> property is initialized to <paramref
        /// name="continuationOptions"/>, and the <see
        /// cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> property is initialized to the
        /// current scheduler (see <see
        /// cref="System.Threading.Tasks.TaskScheduler.Current">TaskScheduler.Current</see>).
        /// </remarks>
        public TaskFactory(TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions)
        {
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            TaskFactory.CheckCreationOptions(creationOptions);

            m_defaultCreationOptions = creationOptions;
            m_defaultContinuationOptions = continuationOptions;
        }

        /// <summary>
        /// Initializes a <see cref="TaskFactory{TResult}"/> instance with the specified configuration.
        /// </summary>
        /// <param name="cancellationToken">The default <see cref="CancellationToken"/> that will be assigned
        /// to tasks created by this <see cref="TaskFactory"/> unless another CancellationToken is explicitly specified
        /// while calling the factory methods.</param>
        /// <param name="creationOptions">
        /// The default <see cref="System.Threading.Tasks.TaskCreationOptions">
        /// TaskCreationOptions</see> to use when creating tasks with this TaskFactory{TResult}.
        /// </param>
        /// <param name="continuationOptions">
        /// The default <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> to use when creating continuation tasks with this TaskFactory{TResult}.
        /// </param>
        /// <param name="scheduler">
        /// The default <see cref="System.Threading.Tasks.TaskScheduler">
        /// TaskScheduler</see> to use to schedule any Tasks created with this TaskFactory{TResult}. A null value
        /// indicates that TaskScheduler.Current should be used.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument or the <paramref name="continuationOptions"/>
        /// argumentspecifies an invalid value.
        /// </exception>
        /// <remarks>
        /// With this constructor, the
        /// <see cref="TaskCreationOptions"/> property is initialized to <paramref name="creationOptions"/>,
        /// the
        /// <see cref="TaskContinuationOptions"/> property is initialized to <paramref
        /// name="continuationOptions"/>, and the <see
        /// cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> property is initialized to
        /// <paramref name="scheduler"/>, unless it's null, in which case the property is initialized to the
        /// current scheduler (see <see
        /// cref="System.Threading.Tasks.TaskScheduler.Current">TaskScheduler.Current</see>).
        /// </remarks>
        public TaskFactory(CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions, TaskScheduler? scheduler)
            : this(creationOptions, continuationOptions)
        {
            m_defaultCancellationToken = cancellationToken;
            m_defaultScheduler = scheduler;
        }

        /* Properties */

        /// <summary>
        /// Gets the default <see cref="System.Threading.CancellationToken">CancellationToken</see> of this
        /// TaskFactory.
        /// </summary>
        /// <remarks>
        /// This property returns the default <see cref="CancellationToken"/> that will be assigned to all
        /// tasks created by this factory unless another CancellationToken value is explicitly specified
        /// during the call to the factory methods.
        /// </remarks>
        public CancellationToken CancellationToken => m_defaultCancellationToken;

        /// <summary>
        /// Gets the <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> of this
        /// TaskFactory{TResult}.
        /// </summary>
        /// <remarks>
        /// This property returns the default scheduler for this factory.  It will be used to schedule all
        /// tasks unless another scheduler is explicitly specified during calls to this factory's methods.
        /// If null, <see cref="System.Threading.Tasks.TaskScheduler.Current">TaskScheduler.Current</see>
        /// will be used.
        /// </remarks>
        public TaskScheduler? Scheduler => m_defaultScheduler;

        /// <summary>
        /// Gets the <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions
        /// </see> value of this TaskFactory{TResult}.
        /// </summary>
        /// <remarks>
        /// This property returns the default creation options for this factory.  They will be used to create all
        /// tasks unless other options are explicitly specified during calls to this factory's methods.
        /// </remarks>
        public TaskCreationOptions CreationOptions => m_defaultCreationOptions;

        /// <summary>
        /// Gets the <see cref="System.Threading.Tasks.TaskCreationOptions">TaskContinuationOptions
        /// </see> value of this TaskFactory{TResult}.
        /// </summary>
        /// <remarks>
        /// This property returns the default continuation options for this factory.  They will be used to create
        /// all continuation tasks unless other options are explicitly specified during calls to this factory's methods.
        /// </remarks>
        public TaskContinuationOptions ContinuationOptions => m_defaultContinuationOptions;


        /* StartNew */

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<TResult> function)
        {
            Task? currTask = Task.InternalCurrent;
            return Task<TResult>.StartNew(currTask, function, m_defaultCancellationToken,
                m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(currTask));
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<TResult> function, CancellationToken cancellationToken)
        {
            Task? currTask = Task.InternalCurrent;
            return Task<TResult>.StartNew(currTask, function, cancellationToken,
                m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(currTask));
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<TResult> function, TaskCreationOptions creationOptions)
        {
            Task? currTask = Task.InternalCurrent;
            return Task<TResult>.StartNew(currTask, function, m_defaultCancellationToken,
                creationOptions, InternalTaskOptions.None, GetDefaultScheduler(currTask));
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="scheduler">The <see
        /// cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created <see cref="System.Threading.Tasks.Task{TResult}">
        /// Task{TResult}</see>.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="scheduler"/>
        /// argument is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return Task<TResult>.StartNew(
                Task.InternalCurrentIfAttached(creationOptions), function, cancellationToken,
                creationOptions, InternalTaskOptions.None, scheduler);
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<object?, TResult> function, object? state)
        {
            Task? currTask = Task.InternalCurrent;
            return Task<TResult>.StartNew(currTask, function, state, m_defaultCancellationToken,
                m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(currTask));
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<object?, TResult> function, object? state, CancellationToken cancellationToken)
        {
            Task? currTask = Task.InternalCurrent;
            return Task<TResult>.StartNew(currTask, function, state, cancellationToken,
                m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(currTask));
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<object?, TResult> function, object? state, TaskCreationOptions creationOptions)
        {
            Task? currTask = Task.InternalCurrent;
            return Task<TResult>.StartNew(currTask, function, state, m_defaultCancellationToken,
                creationOptions, InternalTaskOptions.None, GetDefaultScheduler(currTask));
        }

        /// <summary>
        /// Creates and starts a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="scheduler">The <see
        /// cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created <see cref="System.Threading.Tasks.Task{TResult}">
        /// Task{TResult}</see>.</param>
        /// <returns>The started <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="scheduler"/>
        /// argument is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// Calling StartNew is functionally equivalent to creating a <see cref="Task{TResult}"/> using one
        /// of its constructors and then calling
        /// <see cref="System.Threading.Tasks.Task.Start()">Start</see> to schedule it for execution.
        /// However, unless creation and scheduling must be separated, StartNew is the recommended approach
        /// for both simplicity and performance.
        /// </remarks>
        public Task<TResult> StartNew(Func<object?, TResult> function, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return Task<TResult>.StartNew(Task.InternalCurrentIfAttached(creationOptions), function, state, cancellationToken,
                creationOptions, InternalTaskOptions.None, scheduler);
        }
        
        // Utility method to create a canceled future-style task.
        // Used by ContinueWhenAll/Any to bail out early on a pre-canceled token.
        private static Task<TResult> CreateCanceledTask(TaskContinuationOptions continuationOptions, CancellationToken ct)
        {
            Task.CreationOptionsFromContinuationOptions(continuationOptions, out TaskCreationOptions tco, out _);
            return new Task<TResult>(true, default, tco, ct);
        }

        //
        // ContinueWhenAll() methods
        //

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in
        /// the <paramref name="tasks"/> array have completed.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in
        /// the <paramref name="tasks"/> array have completed.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, CancellationToken cancellationToken)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in the <paramref
        /// name="tasks"/> array have completed.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAll.
        /// </remarks>
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in the <paramref
        /// name="tasks"/> array have completed.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <param name="scheduler">The <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created continuation <see
        /// cref="System.Threading.Tasks.Task">Task</see>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="scheduler"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAll.
        /// </remarks>
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction,
            CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in the
        /// <paramref name="tasks"/> array have completed.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl<TAntecedentResult>(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in the
        /// <paramref name="tasks"/> array have completed.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction,
            CancellationToken cancellationToken)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl<TAntecedentResult>(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in the
        /// <paramref name="tasks"/> array have completed.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAll.
        /// </remarks>
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction,
            TaskContinuationOptions continuationOptions)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl<TAntecedentResult>(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of a set of provided Tasks.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <param name="continuationFunction">The function delegate to execute when all tasks in the
        /// <paramref name="tasks"/> array have completed.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <param name="scheduler">The <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created continuation <see
        /// cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="scheduler"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAll.
        /// </remarks>
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction,
            CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAllImpl<TAntecedentResult>(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler);
        }


        // Core implementation of ContinueWhenAll -- the generic version
        // Note: if you make any changes to this method, please do the same to the non-generic version too.
        internal static Task<TResult> ContinueWhenAllImpl<TAntecedentResult>(Task<TAntecedentResult>[] tasks,
            Func<Task<TAntecedentResult>[], TResult>? continuationFunction, Action<Task<TAntecedentResult>[]>? continuationAction,
            TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            // check arguments
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
            // ArgumentNullException of continuationFunction or continuationAction is checked by the caller
            Debug.Assert((continuationFunction != null) != (continuationAction != null), "Expected exactly one of endFunction/endAction to be non-null");
            if (scheduler == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);

            // Check tasks array and make defensive copy
            Task<TAntecedentResult>[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy<TAntecedentResult>(tasks);

            // Bail early if cancellation has been requested.
            if (cancellationToken.IsCancellationRequested
                && ((continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
                )
            {
                return CreateCanceledTask(continuationOptions, cancellationToken);
            }

            // Call common ContinueWhenAll() setup logic, extract starter task.
            Task<Task<TAntecedentResult>[]> starter = TaskFactory.CommonCWAllLogic(tasksCopy);

            // returned continuation task, off of starter
            if (continuationFunction != null)
            {
                return starter.ContinueWith<TResult>(
                   GenericDelegateCache<TAntecedentResult, TResult>.CWAllFuncDelegate,
                   continuationFunction, scheduler, cancellationToken, continuationOptions);
            }
            else
            {
                Debug.Assert(continuationAction != null);

                return starter.ContinueWith<TResult>(
                   GenericDelegateCache<TAntecedentResult, TResult>.CWAllActionDelegate,
                   continuationAction, scheduler, cancellationToken, continuationOptions);
            }
        }

        // Core implementation of ContinueWhenAll -- the non-generic version
        // Note: if you make any changes to this method, please do the same to the generic version too.
        internal static Task<TResult> ContinueWhenAllImpl(Task[] tasks,
            Func<Task[], TResult>? continuationFunction, Action<Task[]>? continuationAction,
            TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            // check arguments
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
            // ArgumentNullException of continuationFunction or continuationAction is checked by the caller
            Debug.Assert((continuationFunction != null) != (continuationAction != null), "Expected exactly one of endFunction/endAction to be non-null");
            if (scheduler == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);

            // Check tasks array and make defensive copy
            Task[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy(tasks);

            // Bail early if cancellation has been requested.
            if (cancellationToken.IsCancellationRequested
                && ((continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
                )
            {
                return CreateCanceledTask(continuationOptions, cancellationToken);
            }

            // Perform common ContinueWhenAll() setup logic, extract starter task
            Task<Task[]> starter = TaskFactory.CommonCWAllLogic(tasksCopy);

            // returned continuation task, off of starter
            if (continuationFunction != null)
            {
                return starter.ContinueWith(
                    static (completedTasks, state) =>
                    {
                        completedTasks.NotifyDebuggerOfWaitCompletionIfNecessary();
                        Debug.Assert(state is Func<Task[], TResult>);
                        return ((Func<Task[], TResult>)state)(completedTasks.Result);
                    },
                    continuationFunction, scheduler, cancellationToken, continuationOptions);
            }
            else
            {
                Debug.Assert(continuationAction != null);
                return starter.ContinueWith<TResult>(
                   static (completedTasks, state) =>
                   {
                       completedTasks.NotifyDebuggerOfWaitCompletionIfNecessary();
                       Debug.Assert(state is Action<Task[]>);
                       ((Action<Task[]>)state)(completedTasks.Result); return default!;
                   },
                   continuationAction, scheduler, cancellationToken, continuationOptions);
            }
        }

        //
        // ContinueWhenAny() methods
        //

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the <paramref
        /// name="tasks"/> array completes.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the <paramref
        /// name="tasks"/> array completes.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the <paramref
        /// name="tasks"/> array completes.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAny.
        /// </remarks>
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the <paramref
        /// name="tasks"/> array completes.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <param name="scheduler">The <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created continuation <see
        /// cref="System.Threading.Tasks.Task">Task</see>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="scheduler"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAny.
        /// </remarks>
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction,
            CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the
        /// <paramref name="tasks"/> array completes.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl<TAntecedentResult>(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the
        /// <paramref name="tasks"/> array completes.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction,
            CancellationToken cancellationToken)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl<TAntecedentResult>(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the
        /// <paramref name="tasks"/> array completes.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAny.
        /// </remarks>
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction,
            TaskContinuationOptions continuationOptions)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl<TAntecedentResult>(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler);
        }

        /// <summary>
        /// Creates a continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>
        /// that will be started upon the completion of any Task in the provided set.
        /// </summary>
        /// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks"/>.</typeparam>
        /// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
        /// <param name="continuationFunction">The function delegate to execute when one task in the
        /// <paramref name="tasks"/> array completes.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new continuation task.</param>
        /// <param name="continuationOptions">The <see cref="System.Threading.Tasks.TaskContinuationOptions">
        /// TaskContinuationOptions</see> value that controls the behavior of
        /// the created continuation <see cref="System.Threading.Tasks.Task{TResult}">Task</see>.</param>
        /// <param name="scheduler">The <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created continuation <see
        /// cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The new continuation <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="continuationFunction"/> argument is null.</exception>
        /// <exception cref="System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="scheduler"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array contains a null value.</exception>
        /// <exception cref="System.ArgumentException">The exception that is thrown when the
        /// <paramref name="tasks"/> array is empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="continuationOptions"/> argument specifies an invalid TaskContinuationOptions
        /// value.</exception>
        /// <exception cref="System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        /// <remarks>
        /// The NotOn* and OnlyOn* <see cref="System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>,
        /// which constrain for which <see cref="System.Threading.Tasks.TaskStatus">TaskStatus</see> states a continuation
        /// will be executed, are illegal with ContinueWhenAny.
        /// </remarks>
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction,
            CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            if (continuationFunction == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);

            return ContinueWhenAnyImpl<TAntecedentResult>(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler);
        }

        // Core implementation of ContinueWhenAny, non-generic version
        // Note: if you make any changes to this method, be sure to do the same to the generic version
        internal static Task<TResult> ContinueWhenAnyImpl(Task[] tasks,
            Func<Task, TResult>? continuationFunction, Action<Task>? continuationAction,
            TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            // check arguments
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
            if (tasks.Length == 0) ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList, ExceptionArgument.tasks);

            // ArgumentNullException of continuationFunction or continuationAction is checked by the caller
            Debug.Assert((continuationFunction != null) != (continuationAction != null), "Expected exactly one of endFunction/endAction to be non-null");
            if (scheduler == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);

            // Call common ContinueWhenAny() setup logic, extract starter
            Task<Task> starter = TaskFactory.CommonCWAnyLogic(tasks);

            // Bail early if cancellation has been requested.
            if (cancellationToken.IsCancellationRequested
                && ((continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
                )
            {
                return CreateCanceledTask(continuationOptions, cancellationToken);
            }

            // returned continuation task, off of starter
            if (continuationFunction != null)
            {
                return starter.ContinueWith(
                     static (completedTask, state) =>
                     {
                         Debug.Assert(state is Func<Task, TResult>);
                         return ((Func<Task, TResult>)state)(completedTask.Result);
                     },
                     continuationFunction, scheduler, cancellationToken, continuationOptions);
            }
            else
            {
                Debug.Assert(continuationAction != null);
                return starter.ContinueWith<TResult>(
                    static (completedTask, state) =>
                    {
                        Debug.Assert(state is Action<Task>);
                        ((Action<Task>)state)(completedTask.Result);
                        return default!;
                    },
                    continuationAction, scheduler, cancellationToken, continuationOptions);
            }
        }


        // Core implementation of ContinueWhenAny, generic version
        // Note: if you make any changes to this method, be sure to do the same to the non-generic version
        internal static Task<TResult> ContinueWhenAnyImpl<TAntecedentResult>(Task<TAntecedentResult>[] tasks,
            Func<Task<TAntecedentResult>, TResult>? continuationFunction, Action<Task<TAntecedentResult>>? continuationAction,
            TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            // check arguments
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
            if (tasks.Length == 0) ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList, ExceptionArgument.tasks);
            // ArgumentNullException of continuationFunction or continuationAction is checked by the caller
            Debug.Assert((continuationFunction != null) != (continuationAction != null), "Expected exactly one of endFunction/endAction to be non-null");
            if (scheduler == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);

            // Call common ContinueWhenAny setup logic, extract starter
            Task<Task> starter = TaskFactory.CommonCWAnyLogic(tasks);

            // Bail early if cancellation has been requested.
            if (cancellationToken.IsCancellationRequested
                && ((continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
                )
            {
                return CreateCanceledTask(continuationOptions, cancellationToken);
            }

            // returned continuation task, off of starter
            if (continuationFunction != null)
            {
                return starter.ContinueWith<TResult>(
                    GenericDelegateCache<TAntecedentResult, TResult>.CWAnyFuncDelegate,
                    continuationFunction, scheduler, cancellationToken, continuationOptions);
            }
            else
            {
                Debug.Assert(continuationAction != null);
                return starter.ContinueWith<TResult>(
                    GenericDelegateCache<TAntecedentResult, TResult>.CWAnyActionDelegate,
                    continuationAction, scheduler, cancellationToken, continuationOptions);
            }
        }
    }

    // For the ContinueWhenAnyImpl/ContinueWhenAllImpl methods that are generic on TAntecedentResult,
    // the compiler won't cache the internal ContinueWith delegate because it is generic on both
    // TAntecedentResult and TResult.  The GenericDelegateCache serves as a cache for those delegates.
    internal static class GenericDelegateCache<TAntecedentResult, TResult>
    {
        // ContinueWith delegate for TaskFactory<TResult>.ContinueWhenAnyImpl<TAntecedentResult>(non-null continuationFunction)
        internal static Func<Task<Task>, object?, TResult> CWAnyFuncDelegate =
            static (Task<Task> wrappedWinner, object? state) =>
            {
                Debug.Assert(state is Func<Task<TAntecedentResult>, TResult>);
                var func = (Func<Task<TAntecedentResult>, TResult>)state;
                var arg = (Task<TAntecedentResult>)wrappedWinner.Result;
                return func(arg);
            };

        // ContinueWith delegate for TaskFactory<TResult>.ContinueWhenAnyImpl<TAntecedentResult>(non-null continuationAction)
        internal static Func<Task<Task>, object?, TResult> CWAnyActionDelegate =
            static (Task<Task> wrappedWinner, object? state) =>
            {
                Debug.Assert(state is Action<Task<TAntecedentResult>>);
                var action = (Action<Task<TAntecedentResult>>)state;
                var arg = (Task<TAntecedentResult>)wrappedWinner.Result;
                action(arg);
                return default!;
            };

        // ContinueWith delegate for TaskFactory<TResult>.ContinueWhenAllImpl<TAntecedentResult>(non-null continuationFunction)
        internal static Func<Task<Task<TAntecedentResult>[]>, object?, TResult> CWAllFuncDelegate =
            static (Task<Task<TAntecedentResult>[]> wrappedAntecedents, object? state) =>
            {
                wrappedAntecedents.NotifyDebuggerOfWaitCompletionIfNecessary();
                Debug.Assert(state is Func<Task<TAntecedentResult>[], TResult>);
                var func = (Func<Task<TAntecedentResult>[], TResult>)state;
                return func(wrappedAntecedents.Result);
            };

        // ContinueWith delegate for TaskFactory<TResult>.ContinueWhenAllImpl<TAntecedentResult>(non-null continuationAction)
        internal static Func<Task<Task<TAntecedentResult>[]>, object?, TResult> CWAllActionDelegate =
            static (Task<Task<TAntecedentResult>[]> wrappedAntecedents, object? state) =>
            {
                wrappedAntecedents.NotifyDebuggerOfWaitCompletionIfNecessary();
                Debug.Assert(state is Action<Task<TAntecedentResult>[]>);
                var action = (Action<Task<TAntecedentResult>[]>)state;
                action(wrappedAntecedents.Result);
                return default!;
            };
    }
}
