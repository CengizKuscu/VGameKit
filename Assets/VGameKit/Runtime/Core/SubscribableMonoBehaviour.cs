using System;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace VGameKit.Runtime.Core
{
    /// <summary>
    /// Base MonoBehaviour class that supports subscription management using MessagePipe.
    /// Implements <see cref="ISubscribableObject"/> for disposable subscription handling.
    /// </summary>
    public class SubscribableMonoBehaviour : MonoBehaviour, ISubscribableObject
    {
        /// <summary>
        /// Holds the built disposable subscription.
        /// </summary>
        private IDisposable _subscription;
    
        /// <summary>
        /// Builder for collecting disposables before building the subscription.
        /// </summary>
        protected DisposableBagBuilder _bagBuilder;
    
        /// <summary>
        /// Called by dependency injection to construct the object.
        /// Initializes the disposable bag, calls Init and Subscriptions, and builds the subscription.
        /// </summary>
        [Inject]
        public virtual void Construct()
        {
            _bagBuilder = DisposableBag.CreateBuilder();
            Init();
            Subscriptions();
            _subscription = _bagBuilder.Build();
        }
    
        /// <summary>
        /// Override to perform initialization logic.
        /// </summary>
        public virtual void Init()
        {
        }
    
        /// <summary>
        /// Override to add subscriptions to the disposable bag.
        /// </summary>
        public virtual void Subscriptions()
        {
        }
    
        /// <summary>
        /// Disposes the subscription when the object is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
        }
    
        /// <summary>
        /// Override for logic when the object is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
        }
    
        /// <summary>
        /// Override for logic when the object is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
        }
    
        /// <summary>
        /// Override for logic during Awake phase.
        /// </summary>
        protected virtual void Awake()
        {
        }
    
        /// <summary>
        /// Disposes the subscription manually.
        /// </summary>
        public virtual void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}