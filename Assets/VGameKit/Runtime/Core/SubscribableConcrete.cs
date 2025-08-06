using System;
using MessagePipe;
using VContainer.Unity;

namespace VGameKit.Runtime.Core
{
    /// <summary>
    /// Abstract base class that provides subscription management using MessagePipe.
    /// Implements <see cref="IInitializable"/> and <see cref="ISubscribableObject"/>.
    /// </summary>
    public abstract class SubscribableConcrete : IInitializable, ISubscribableObject
    {
        /// <summary>
        /// Holds the built disposable subscription.
        /// </summary>
        private IDisposable _subscription;
        
        /// <summary>
        /// Builder for managing multiple disposables.
        /// </summary>
        protected DisposableBagBuilder _bagBuilder;
        
        /// <summary>
        /// Initializes the subscribable, sets up the disposable bag, calls initialization and subscription methods.
        /// </summary>
        public void Initialize()
        {
            _bagBuilder = DisposableBag.CreateBuilder();
            Init();
            Subscriptions();
            _subscription = _bagBuilder.Build();
        }
    
        /// <summary>
        /// Optional initialization logic for derived classes.
        /// </summary>
        public virtual void Init()
        {
            
        }
    
        /// <summary>
        /// Optional subscription logic for derived classes.
        /// </summary>
        public virtual void Subscriptions()
        {
            
        }
    
        /// <summary>
        /// Disposes the subscription and releases resources.
        /// </summary>
        public virtual void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}