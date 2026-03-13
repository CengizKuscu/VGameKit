## Lifetime Scopes Reference

English documentation for AbsBaseLifetimeScope and AbsMainLifetimeScope.

## Overview
VGameKit lifetime scopes work with VContainer's LifetimeScope to manage readiness, logging, and frame rate settings.

## AbsBaseLifetimeScope
- IsReady: is the scope ready?
- Configure(IContainerBuilder): registers a build callback to flip IsReady to true when build completes; logs readiness.

## AbsMainLifetimeScope
- _targetFrameRate: serialized field for target frame rate
- _messagePipeOpts: holds MessagePipe options registered in container
- Configure override applies logging and frame rate setup

## Usage Example
```csharp
public sealed class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

## See Also
- AbsBaseLifetimeScope
- AbsMainLifetimeScope
