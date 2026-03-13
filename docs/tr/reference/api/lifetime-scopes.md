# Lifetime Scopes Referansları

Türkçe dokümantasyon için AbsBaseLifetimeScope ve AbsMainLifetimeScope kapsar.

## Genel Bakış
VGameKit yaşam döngüsü kapsamları, VContainer LifetimeScope ile uyumlu olarak hazır/başlatma durumunu yönetir ve logging/FrameRate ayarlarını içerir.

## AbsBaseLifetimeScope
- IsReady: scope hazır mı?
- Configure(IContainerBuilder): ready callbacki kaydedilir; yapılandırma sırasında `IsReady` false başlar ve tamamlandığında true yapılır. GKLog ile loglanır.

## AbsMainLifetimeScope
- `_targetFrameRate` inspector üzerinden ayarlanabilir
- `_messagePipeOpts` ile MessagePipe kurulumu
- `Configure(IContainerBuilder)` override ile logging açma ve frame rate ayarları

## Örnek Kullanım
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
