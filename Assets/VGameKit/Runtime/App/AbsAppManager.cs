using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.App.Events;
using VGameKit.Runtime.Core;

namespace VGameKit.Runtime.App
{
    /// <summary>
    /// Abstract base class for managing application lifecycle events.
    /// Handles subscription to the <see cref="AppReadyEvent"/> and publishes it when the app is ready.
    /// </summary>
    public abstract class AbsAppManager : SubscribableConcrete, IAsyncStartable
    {
        /// <summary>
        /// Publishes <see cref="AppReadyEvent"/> when the application is ready.
        /// </summary>
        [Inject] private readonly IPublisher<AppReadyEvent> _appReadyPublisher;
    
        /// <summary>
        /// Subscribes to <see cref="AppReadyEvent"/>.
        /// </summary>
        [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;
        
        
        /// <summary>
        /// Registers subscriptions for application events.
        /// </summary>
        public override void Subscriptions()
        {
            _appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
        }
        
        /// <summary>
        /// Asynchronously starts the application, initializes the game, and publishes <see cref="AppReadyEvent"/>.
        /// </summary>
        /// <param name="token">Cancellation token for async operations.</param>
        public async UniTask StartAsync(CancellationToken token)
        {
            await InitializeGame(token).AttachExternalCancellation(token).SuppressCancellationThrow();
            
            _appReadyPublisher.Publish(new AppReadyEvent());
        }
        
        /// <summary>
        /// Initializes the game asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token for async operations.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        protected abstract UniTask InitializeGame(CancellationToken token);
        
        /// <summary>
        /// Handles the <see cref="AppReadyEvent"/> when the application is ready.
        /// </summary>
        /// <param name="event">The <see cref="AppReadyEvent"/> instance.</param>
        protected abstract void OnAppReady(AppReadyEvent @event);
    }
}