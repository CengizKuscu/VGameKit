# GKConfig (Ayarlar) Referans

Bu doküman GKConfig sınıfını Türkçe olarak açıklar.

## Genel Bakış / Overview
GKConfig, global ayarları yöneten bir ScriptableObject singleton’udur. LogState gibi ayarları içerir.

## Ana Özellikler / Key Features
- `LogState` konfigürasyonu
- Editor ve runtime için asset edinme/yüketme mekanizması
- Preloaded assets ile Unity proje açılışında yükleme

## Kullanım / Usage
```csharp
GKConfig.Instance.LogState = LogState.Info;
```

## See Also
- GKLog
