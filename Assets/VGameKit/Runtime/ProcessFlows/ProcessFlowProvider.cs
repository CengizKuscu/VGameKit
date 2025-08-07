using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;
using VGameKit.Runtime.Log;

namespace VGameKit.Runtime.ProcessFlows
{
    /// <summary>
    /// Provides management for process flows, including creation, removal, and lifecycle handling.
    /// </summary>
    public class ProcessFlowProvider : IDisposable
    {
        /// <summary>
        /// Dependency-injected object resolver.
        /// </summary>
        [Inject] private readonly IObjectResolver _resolver;
    
        /// <summary>
        /// Gets the object resolver.
        /// </summary>
        public IObjectResolver Resolver => _resolver;
    
        /// <summary>
        /// Internal list of active process flows.
        /// </summary>
        private readonly List<IProcessFlow> _flows = new List<IProcessFlow>();
    
        /// <summary>
        /// Gets the first process flow of the specified type.
        /// </summary>
        /// <typeparam name="TProcessFlow">Type of process flow.</typeparam>
        /// <returns>The process flow instance if found; otherwise, null.</returns>
        public IProcessFlow GetProcessFlow<TProcessFlow>() where TProcessFlow : IProcessFlow
        {
            return _flows.Find(flow => flow.GetType() == typeof(TProcessFlow));
        }
    
        /// <summary>
        /// Gets all process flows of the specified type.
        /// </summary>
        /// <typeparam name="TProcessFlow">Type of process flow.</typeparam>
        /// <returns>Enumerable of process flows.</returns>
        public IEnumerable<IProcessFlow> GetProcessFlows<TProcessFlow>() where TProcessFlow : IProcessFlow
        {
            return _flows.FindAll(flow => flow.GetType() == typeof(TProcessFlow));
        }
    
        /// <summary>
        /// Indicates whether any process flows are present.
        /// </summary>
        public bool IsAnyProcessFlow => _flows.Count > 0;
    
        /// <summary>
        /// Removes the first process flow of the specified type.
        /// </summary>
        /// <typeparam name="TProcessFlow">Type of process flow.</typeparam>
        public void RemoveProcessFlow<TProcessFlow>() where TProcessFlow : IProcessFlow
        {
            var processFlow = GetProcessFlow<TProcessFlow>();
            if (processFlow != null)
            {
                processFlow.Cancel();
                processFlow.Dispose();
                _flows.Remove(processFlow);
            }
        }
    
        /// <summary>
        /// Removes all process flows of the specified type.
        /// </summary>
        /// <typeparam name="TProcessFlow">Type of process flow.</typeparam>
        public void RemoveProcessFlows<TProcessFlow>() where TProcessFlow : IProcessFlow
        {
            var processFlows = GetProcessFlows<TProcessFlow>();
            var length = processFlows is ICollection<IProcessFlow> collection ? collection.Count : 0;
            for (int i = 0; i < length; i++)
            {
                var flow = processFlows.ElementAt(i);
                flow.Cancel();
                flow.Dispose();
                _flows.Remove(flow);
                GKLog.Log(LogState.ProcessFlow, $"ProcessFlowProvider: Process removed! {flow.Id}");
            }
        }
    
        /// <summary>
        /// Removes and disposes all process flows.
        /// </summary>
        public void RemoveAllProcessFlows()
        {
            foreach (var processFlow in _flows)
            {
                processFlow.Cancel();
                processFlow.Dispose();
            }
    
            GKLog.Log(LogState.ProcessFlow, "ProcessFlowProvider: All processes removed!");
            _flows.Clear();
        }
    
        /// <summary>
        /// Disposes the provider and all managed process flows.
        /// </summary>
        public void Dispose()
        {
            RemoveAllProcessFlows();
        }
    
        /// <summary>
        /// Adds a process flow and registers its completion callback.
        /// </summary>
        /// <param name="flow">The process flow to add.</param>
        public void AddProcessFlow(IProcessFlow flow)
        {
            flow.OnComplete(processFlow =>
            {
                _flows.Remove(processFlow);
                GKLog.Log(LogState.ProcessFlow, $"ProcessFlowProvider: Process removed! {processFlow.Id}");
                processFlow.Dispose();
                processFlow.cancellationTokenSource.Cancel();
            });
            _flows.Add(flow);
        }
    
        /// <summary>
        /// Creates, initializes, and adds a new process flow of the specified type.
        /// </summary>
        /// <typeparam name="TProcessFlowArgs">Type of process flow arguments.</typeparam>
        /// <typeparam name="TProcessFlow">Type of process flow.</typeparam>
        /// <param name="args">Arguments for the process flow.</param>
        /// <returns>The created process flow.</returns>
        public TProcessFlow CreateProcessFlow<TProcessFlowArgs, TProcessFlow>(TProcessFlowArgs args)
            where TProcessFlowArgs : IProcessFlowArgs where TProcessFlow : IProcessFlow<TProcessFlowArgs>, new()
        {
            var processFlow = new TProcessFlow();
            _resolver.Inject(processFlow);
            processFlow.Initialize(args);
            AddProcessFlow(processFlow);
            return processFlow;
        }
    }
}