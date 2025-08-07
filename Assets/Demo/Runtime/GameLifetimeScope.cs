using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Spawner;

namespace Demo.Runtime
{
    public class GameLifetimeScope : AbsBaseLifetimeScope
    {
        [SerializeField] private DemoSpawnItem _demoSpawnItemPrefab;
        [SerializeField] private Transform _demoSpawnItemParent;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_demoSpawnItemParent);
            
            builder.Register<GameAppManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterObjectSpawner<DemoSpawnItemModel, Transform, DemoSpawnItem>(_demoSpawnItemPrefab,
                Lifetime.Singleton, true);
        }
    }
}