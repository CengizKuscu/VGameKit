using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.App;
using VGameKit.Runtime.App.Events;
using VGameKit.Runtime.Log;

namespace Demo.Runtime
{
    public class AppManager : AbsAppManager
    {
        protected override UniTask InitializeGame(CancellationToken token)
        {
            GKLog.Log(LogState.Development, $"AppManager: Initializing game...");
            //await UniTask.CompletedTask;
            return UniTask.CompletedTask;
        }

        protected override void OnAppReady(AppReadyEvent @event)
        {
            GKLog.Log(LogState.Development, $"AppManager: OnAppReady");
        }
    }
}