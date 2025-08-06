using VContainer;
using VGameKit.Runtime.Core;

namespace Demo.Runtime
{
    public class GameLifetimeScope : AbsBaseLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.Register<GameAppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}