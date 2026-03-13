# GKLogging Referans

Bu belge GKLog sınıfı ve loglama davranışını Türkçe olarak açıklar.

## Genel Bakış / Overview
GKLog, belirli seviyelerde log üreten ve Unity Console’da renkli çıktılarla görselleştirme yapan yardımcı bir sınıftır.

## Ana Özellikler / Key Features
- Farklı LogState seviyeleriyle loglama
- `GAMEKIT_LOG` tanımlı olduğunda çıktı üretir
- Farklı kategorilere göre renklendirme

## Örnek / Example
```csharp
GKLog.Log(LogState.Info, "Bir bilgi mesajı");
```

## See Also
- GKConfig (LogState ayarları)
