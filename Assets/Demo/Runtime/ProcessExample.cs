using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;
using Object = UnityEngine.Object;

namespace Demo.Runtime
{
    [CreateAssetMenu(fileName = "LevelPrefabs", menuName = "Demo/LevelPrefabs")]
    public class LevelPrefabs : ScriptableObject
    {
        public List<LevelObj> Levels;
    }

    public class LevelObj : MonoBehaviour
    {
        public void Initialize()
        {
            // Do Something;
        }
    }

    public class MainAppLifetimeScope : AbsMainLifetimeScope
    {
        [SerializeField] private LevelPrefabs _levelPrefabs;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<DemoManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.RegisterComponent(_levelPrefabs);
            builder.RegisterProcessFlow<LoadLevelFlowArgs, LoadLevelFlow>(Lifetime.Singleton);
        }
    }

    public class LoadLevelFlowArgs : IProcessFlowArgs
    {
        public int LevelIndex { get; private set; }

        public LoadLevelFlowArgs(int levelIndex)
        {
            LevelIndex = levelIndex;
        }
    }
    
    public class LoadLevelFlow : BaseProcessFlow<LoadLevelFlowArgs>
    {
        [Inject] private readonly LevelPrefabs _levelPrefabs;
        
        public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
        {
            var levelIndex = Args.LevelIndex;
            
            var levelPrefab = _levelPrefabs.Levels[levelIndex];

            LevelObj level = null;

            await UniTask.WaitUntil(() => level = Object.Instantiate(levelPrefab).GetComponent<LevelObj>(), cancellationToken: ctx);
            
            level.Initialize();
            
            return this;
        }
    }

    public class DemoManager : IInitializable, IDisposable
    {
        [Inject] private readonly Func<LoadLevelFlowArgs, LoadLevelFlow> _loadLevelFlow;

        private CancellationTokenSource _cts;

        public void LoadLevel(int levelIndex)
        {
            var args = new LoadLevelFlowArgs(levelIndex);
            var flow = _loadLevelFlow(args);
            flow.OnComplete(f =>
            {
                if (f is LoadLevelFlow fp)
                {
                    Debug.Log($"Level {fp.Args.LevelIndex} loaded successfully.");
                }
            });
            flow.Execute(_cts.Token);
        }

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cts?.Cancel();
        }
    }
}