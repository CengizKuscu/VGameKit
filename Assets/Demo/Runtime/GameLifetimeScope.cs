using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Spawner;
using VGameKit.Runtime.UI.Menu;

namespace Demo.Runtime
{
    public class GameLifetimeScope : AbsBaseLifetimeScope
    {
        [SerializeField] private DemoSpawnItem _demoSpawnItemPrefab;
        [SerializeField] private Transform _demoSpawnItemParent;
        
        [SerializeField] private DemoPopupBuilder _demoPopupBuilder;
        
        [SerializeField] private LevelCompleteView _levelCompleteViewPrefab;
        
        [SerializeField] private MenuManager _menuManager;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_levelCompleteViewPrefab);

            builder.RegisterComponent(_menuManager);
            
            builder.Register<LevelCompletePresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            
            builder.RegisterMenuFactory<GameMenuName, LevelCompletePresenter, LevelCompleteView>(_levelCompleteViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);
            
            

            builder.RegisterComponent(_demoSpawnItemParent);
            builder.RegisterComponent(_demoPopupBuilder);
            
            builder.Register<GameAppManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterObjectSpawner<DemoSpawnItemModel, Transform, DemoSpawnItem>(_demoSpawnItemPrefab,
                Lifetime.Singleton, true);
            
        }
    }
}