# Tutorial: VGameKit'e Başlarken

## Ne İnşa Edeceksiniz

Bu tutorial'ın sonunda, çalışan bir VGameKit `AppManager`'ına sahip minimal bir Unity sahnesi olacak; log çıktısı üretecek ve ek VGameKit sistemleri barındırmaya hazır olacak.

**Süre:** ~20 dakika  
**Ön Koşullar:** Unity 6000.3.7f1 kurulu; temel C# ve Unity editör bilgisi; VContainer ve UniTask paketleri mevcut.

---

## Adım 1 — Yeni Unity projesi oluşturun

1. Unity Hub'ı açın ve Unity 6000.3.7f1 hedefleyen bir **3D (Core)** projesi oluşturun.
2. **Window > Package Manager** açın ve şunları kurun:
   - **VContainer** (Git URL veya OpenUPM aracılığıyla)
   - **UniTask** (Cysharp, Git URL aracılığıyla)
   - **MessagePipe** (Cysharp, Git URL aracılığıyla)

VGameKit modüllerini **Package Manager > + > Add package from Git URL** aracılığıyla kurun veya `Packages/manifest.json` dosyasındaki `dependencies` bloğuna ekleyin:

```json
{
  "dependencies": {
    "com.cngz.vgamekit": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.4"
  }
}
```

JSON kalıcılığı, reklam veya analitik desteği için isteğe bağlı modülleri de ekleyin — Git URL'lerinin tam listesi için [VGameKit Modülleri Nasıl Kurulur](tr-HowTo-Install-Modules) rehberine bakın.

---

## Adım 2 — GAMEKIT_LOG derleme sembolünü etkinleştirin

VGameKit'in `GKLog`'u yalnızca `GAMEKIT_LOG` tanımlı olduğunda çıktı üretir.

1. **Edit > Project Settings > Player > Scripting Define Symbols** bölümüne gidin.
2. `GAMEKIT_LOG` ekleyin ve **Apply**'a tıklayın.

Artık Play Mode sırasında `GKLog` çağrılarını konsolda görmelisiniz.

---

## Adım 3 — App LifetimeScope oluşturun

`AbsMainLifetimeScope`'dan kalıtım alan `MyAppLifetimeScope` adlı yeni bir C# sınıfı oluşturun.

```csharp
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;

public class MyAppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.Register<MyAppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

`base.Configure`, MessagePipe'ı ve VGameKit core servislerini kaydeder. `builder.RegisterMessagePipe()`'ı kendiniz çağırmayın.

---

## Adım 4 — App Manager oluşturun

`AbsAppManager`'dan kalıtım alan `MyAppManager`'ı oluşturun:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.App;
using VGameKit.Runtime.Log;

public class MyAppManager : AbsAppManager
{
    public override async UniTask StartAsync(CancellationToken cancellation)
    {
        GKLog.Log(LogState.Game, "MyAppManager: Uygulama başladı.");
        await UniTask.CompletedTask;
    }
}
```

VContainer, `StartAsync`'ı `IAsyncStartable` aracılığıyla otomatik olarak çağırır — manuel olarak çağırmayın.

---

## Adım 5 — Sahneyi bağlayın

1. Varsayılan sahneyi açın (`SampleScene` veya yenisini oluşturun).
2. **AppScope** adında boş bir GameObject oluşturun.
3. `MyAppLifetimeScope`'u buna ekleyin.
4. Play Mode'a girin.

Konsolda şunu görmelisiniz:

```
[Game] MyAppManager: Uygulama başladı.
```

---

## Adım 6 — Config asset ekleyin

VGameKit, ortam değerlerini `GKConfig` ScriptableObject'lerinde saklar.

1. Project penceresinde sağ tıklayın → **Create > VGameKit > Config**.
2. `AppConfig` olarak adlandırın ve ihtiyacınız olan alanları ayarlayın.
3. `MyAppLifetimeScope`'daki bir alana atayın veya DI aracılığıyla okuyun.

---

## Ne Öğrendiniz

- `AbsMainLifetimeScope` ile VGameKit'in DI container'ını nasıl başlatacağınızı.
- `AbsAppManager` / `IAsyncStartable`'ın güvenli bir async giriş noktası sağladığını.
- `GAMEKIT_LOG` derleme sembolüyle `GKLog` çıktısını nasıl etkinleştireceğinizi.
- `GKConfig` ScriptableObject'lerinin ortama özgü verileri nasıl tuttuğunu.

---

## Sonraki Adımlar

- [Tutorial: İlk mini oyununuzu oluşturun](tr-Tutorial-First-Game)
- [Nasıl yapılır: Logging yapılandırma](tr-HowTo-Configure-Logging)
- [Referans: Lifetime Scopes](tr-Reference-Lifetime-Scopes)
