using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.Log;

namespace VGameKit.Runtime.ProcessFlows
{
    /// <summary>
    /// Abstract base class for process flows with arguments of type <typeparamref name="TArgs"/>.
    /// Handles process execution, chaining, cancellation, and completion events.
    /// </summary>
    /// <typeparam name="TArgs">Type of arguments implementing <see cref="IProcessFlowArgs"/>.</typeparam>
    public abstract class BaseProcessFlow<TArgs> : IProcessFlow<TArgs> where TArgs : IProcessFlowArgs
    {
        /// <summary>
        /// Dependency-injected object resolver.
        /// </summary>
        [Inject] protected readonly IObjectResolver _resolver;
    
        /// <summary>
        /// Reference to the next process in the flow (not used in this base class).
        /// </summary>
        private IProcessFlow<TArgs> _nextProcess;
    
        /// <summary>
        /// List of appended process flows to execute sequentially.
        /// </summary>
        protected readonly List<IProcessFlow> _processFlows;
    
        /// <summary>
        /// Source for cancellation tokens.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;
    
        /// <summary>
        /// Event triggered when the process flow is completed.
        /// </summary>
        public event Action<IProcessFlow> onComplete;
    
        /// <summary>
        /// Indicates whether the process flow has completed.
        /// </summary>
        public bool IsCompleted { get; private set; }
    
        /// <summary>
        /// Identifier for the process flow (class name).
        /// </summary>
        public string Id => GetType().Name;
    
        /// <summary>
        /// Arguments for the process flow.
        /// </summary>
        public TArgs Args { get; set; }
    
        /// <summary>
        /// Gets or creates the cancellation token source.
        /// </summary>
        public CancellationTokenSource cancellationTokenSource =>
            _cancellationTokenSource ??= new CancellationTokenSource();
    
        /// <summary>
        /// Gets the cancellation token for this process flow.
        /// </summary>
        public CancellationToken cancellationToken => cancellationTokenSource.Token;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProcessFlow{TArgs}"/> class.
        /// </summary>
        protected BaseProcessFlow()
        {
            _processFlows = new List<IProcessFlow>();
    
    #if UNITY_EDITOR
            onComplete += flow => { GKLog.Log(LogState.ProcessFlow, $"PROCESS COMPLETED : {Id}"); };
    #endif
        }
    
        /// <summary>
        /// Initializes the process flow with the provided arguments.
        /// </summary>
        /// <param name="flowArgs">Arguments for the process flow.</param>
        public void Initialize(IProcessFlowArgs flowArgs)
        {
            Args = flowArgs is TArgs ? (TArgs)flowArgs : default;
            IsCompleted = false;
            Initialize();
        }
    
        /// <summary>
        /// Optional override for additional initialization logic.
        /// </summary>
        protected virtual void Initialize()
        {
        }
    
        /// <summary>
        /// Registers a callback to be invoked when the process flow completes.
        /// </summary>
        /// <param name="callBack">Callback action.</param>
        /// <returns>The current process flow instance.</returns>
        public IProcessFlow OnComplete(Action<IProcessFlow> callBack)
        {
            onComplete += callBack;
            return this;
        }
    
        /// <summary>
        /// Appends a process flow to be executed after this one.
        /// </summary>
        /// <param name="process">Process flow to append.</param>
        /// <returns>The current process flow instance.</returns>
        public IProcessFlow AppendProcess(IProcessFlow process)
        {
            if (process is { } flow)
            {
                _processFlows.Add(flow);
            }
    
            return this;
        }
    
        /// <summary>
        /// Starts execution of the process flow.
        /// </summary>
        /// <param name="ctx">Cancellation token.</param>
        public void Execute(CancellationToken ctx)
        {
            IsCompleted = false;
            ExecuteAsync(ctx).AttachExternalCancellation(cancellationToken);
        }
    
        /// <summary>
        /// Asynchronously executes the process flow and its continuation.
        /// </summary>
        /// <param name="ctx">Cancellation token.</param>
        private async UniTask ExecuteAsync(CancellationToken ctx)
        {
    #if UNITY_EDITOR
            GKLog.Log(LogState.ProcessFlow, $"PROCESS STARTED : {Id}");
    #endif
            IsCompleted = false;
    
            await AsyncExecute(ctx).ContinueWith(s => ContinuationFunction(ctx))
                .AttachExternalCancellation(cancellationToken);
        }
    
        /// <summary>
        /// Asynchronously executes the main logic of the process flow.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="ctx">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx);
    
        /// <summary>
        /// Handles the continuation logic after the main process execution,
        /// including executing appended process flows sequentially.
        /// </summary>
        /// <param name="ctx">Cancellation token.</param>
        private async UniTask ContinuationFunction(CancellationToken ctx = default)
        {
            var checkList = _processFlows.ToArray();
    
            if (checkList.Any())
            {
                var length = checkList.Length;
    
                for (var i = 0; i < length; i++)
                {
                    var flow = checkList[i];
                    flow.Execute(ctx);
                    if (flow.IsCompleted)
                    {
                        continue;
                    }
    
                    await UniTask.WaitUntilValueChanged(flow, x => x.IsCompleted, cancellationToken: ctx);
                }
            }
    
            IsCompleted = true;
            onComplete?.Invoke(this);
        }
    
        /// <summary>
        /// Cancels the process flow execution.
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource?.Cancel();
        }
    
        /// <summary>
        /// Disposes the process flow and suppresses finalization.
        /// </summary>
        public void Dispose()
        {
    #if UNITY_EDITOR
            GKLog.Log(LogState.ProcessFlow, $"PROCESS DISPOSED : {Id}");
    #endif
    
            GC.SuppressFinalize(this);
        }
    }
}