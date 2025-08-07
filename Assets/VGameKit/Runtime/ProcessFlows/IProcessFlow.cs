using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace VGameKit.Runtime.ProcessFlows
{
    /// <summary>
    /// Represents a process flow that can be executed, cancelled, and chained with other process flows.
    /// </summary>
    public interface IProcessFlow : IDisposable
    {
        /// <summary>
        /// Event triggered when the process flow is completed.
        /// </summary>
        event Action<IProcessFlow> onComplete;
        
        /// <summary>
        /// Gets the unique identifier of the process flow.
        /// </summary>
        string Id { get; }
    
        /// <summary>
        /// Gets a value indicating whether the process flow has completed.
        /// </summary>
        bool IsCompleted { get; }
    
        /// <summary>
        /// Initializes the process flow with the specified arguments.
        /// </summary>
        /// <param name="flowArgs">Arguments for the process flow.</param>
        void Initialize(IProcessFlowArgs flowArgs);
    
        /// <summary>
        /// Gets the cancellation token source for the process flow.
        /// </summary>
        CancellationTokenSource cancellationTokenSource { get; }
    
        /// <summary>
        /// Gets the cancellation token for the process flow.
        /// </summary>
        CancellationToken cancellationToken { get; }
    
        /// <summary>
        /// Appends another process flow to be executed after this one.
        /// </summary>
        /// <param name="process">The process flow to append.</param>
        /// <returns>The current process flow instance.</returns>
        IProcessFlow AppendProcess(IProcessFlow process);
    
        /// <summary>
        /// Registers a callback to be invoked when the process flow completes.
        /// </summary>
        /// <param name="callBack">The callback to register.</param>
        /// <returns>The current process flow instance.</returns>
        IProcessFlow OnComplete(Action<IProcessFlow> callBack);
    
        /// <summary>
        /// Executes the process flow synchronously.
        /// </summary>
        /// <param name="ctx">Cancellation token for execution.</param>
        void Execute(CancellationToken ctx);
    
        /// <summary>
        /// Executes the process flow asynchronously.
        /// </summary>
        /// <param name="ctx">Cancellation token for execution.</param>
        /// <returns>A task representing the asynchronous operation, with the process flow as the result.</returns>
        UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx);
    
        /// <summary>
        /// Cancels the process flow.
        /// </summary>
        void Cancel();
    }
    
    /// <summary>
    /// Represents a process flow with strongly-typed arguments.
    /// </summary>
    /// <typeparam name="TArgs">The type of arguments for the process flow.</typeparam>
    public interface IProcessFlow<TArgs> : IProcessFlow where TArgs : IProcessFlowArgs
    {
        /// <summary>
        /// Gets the arguments for the process flow.
        /// </summary>
        TArgs Args { get; }
    }
}