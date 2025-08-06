using VContainer;
using VGameKit.Runtime.Core;

namespace Demo.Runtime
{
    public sealed class AppLifetimeScope : AbsMainLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}