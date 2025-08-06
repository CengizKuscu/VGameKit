using MessagePipe;
using VContainer;
using VGameKit.Runtime.App.Events;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;

namespace Demo.Runtime
{
    public class GameAppManager : SubscribableConcrete
    {
        [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;

        public override void Init()
        {
            GKLog.Log(LogState.Development, "GameAppManager: Initializing...");
        }

        public override void Subscriptions()
        {
            _appReadySubscriber.Subscribe(e=> OnAppReady(e)).AddTo(_bagBuilder);
        }

        private void OnAppReady(AppReadyEvent @event)
        {
            GKLog.Log(LogState.Development, "GameAppManager: OnAppReady");
        }
    }
}