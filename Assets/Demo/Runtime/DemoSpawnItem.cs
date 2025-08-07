using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.Spawner;
using Random = UnityEngine.Random;

namespace Demo.Runtime
{
    public class DemoSpawnItem : MonoBehaviour, ISpawnItem<DemoSpawnItemModel>
    {
        public void Dispose()
        {
        }

        public BaseSpawnPool SpawnPool { get; set; }

        public void ReleaseItem(ISpawnItem item)
        {
            SpawnPool?.ReleaseItem(item);
        }

        public DemoSpawnItemModel ItemModel { get; set; }

        public void ReInitialize(DemoSpawnItemModel itemModel)
        {
            ItemModel = itemModel;
            GKLog.Log(LogState.Game, $"DemoSpawnItem: ReInitialized with model {itemModel}");
            ReleaseItemAsync().AttachExternalCancellation(destroyCancellationToken).Forget();
        }

        private async UniTask ReleaseItemAsync()
        {
            var rndomDelay = Random.Range(1000, 5000);
            await UniTask.Delay(rndomDelay);
            ReleaseItem(this);
            GKLog.Log(LogState.Game, $"DemoSpawnItem: Released after {rndomDelay/1000f} second");
        }
    }


    public class DemoSpawnItemModel : ISpawnItemModel
    {
    }

    public class DemoSpawnItemPool : BaseSpawnPool<DemoSpawnItemModel, DemoSpawnItem>
    {
        public DemoSpawnItemPool(Func<DemoSpawnItemModel, Transform, DemoSpawnItem> spawnFunc, int poolSize,
            Transform poolTarget, bool willGrow) : base(spawnFunc, poolSize, poolTarget, willGrow)
        {
        }
    }
}