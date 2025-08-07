using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using VGameKit.Runtime.App.Events;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;

namespace Demo.Runtime
{
    public class GameAppManager : SubscribableConcrete
    {
        [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;
        
        [Inject] private readonly Func<DemoSpawnItemModel, Transform, DemoSpawnItem> _demoSpawnItemSpawner;

        [Inject] private readonly Transform _demoParent;
        
        private DemoSpawnItemPool _demoSpawnItemPool;

        public override void Init()
        {
            GKLog.Log(LogState.Development, "GameAppManager: Initializing...");
            _demoSpawnItemPool = new DemoSpawnItemPool(_demoSpawnItemSpawner, 4, _demoParent, true);
        }

        public override void Subscriptions()
        {
            _appReadySubscriber.Subscribe(e=> OnAppReady(e)).AddTo(_bagBuilder);
        }

        private void OnAppReady(AppReadyEvent @event)
        {
            GKLog.Log(LogState.Development, "GameAppManager: OnAppReady");
            
            SpawnItemAsync().Forget();
        }

        private async UniTaskVoid SpawnItemAsync()
        {
            await UniTask.Delay(2000); // Simulate some delay before spawning items
            var length = 10;
            
            for (int i = 0; i < length; i++)
            {
                var itemModel = new DemoSpawnItemModel();
                var item = _demoSpawnItemPool.GetItem(itemModel, _demoParent);
                //item.ReInitialize(itemModel);
                GKLog.Log(LogState.Game, $"GameAppManager: Spawned item {i + 1}/{length}");
                
                // Simulate some delay
                await UniTask.Delay(500);
            }
            
            _demoSpawnItemPool.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            _demoSpawnItemPool?.Dispose();
        }
    }
}