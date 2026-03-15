# VGameKit Dokümantasyon Planı

> **Son Güncelleme:** 13 Mart 2026
> **Durum:** Reference tamamlandı — How-to Guides tamamlandı — Tutorials tamamlandı — Explanation tamamlandı

---

## Hedef Kitle

- **Birincil:** Deneyimli Unity geliştiricileri
- **Uyarı (Yeni Geliştiriciler):** Bu dokümantasyon deneyimli ekipler için hazırlanmıştır. Yeni başlayanlar için [tutorials](./tutorials/) bölümünü takip etmenizi öneririz. VContainer, UniTask, MessagePipe gibi kavramlarda deneyimli olmanız beklenmektedir.

---

## Dokümantasyon Konumları

1. **GitHub Wiki:** `https://github.com/CengizKuscu/VGameKit/wiki`
2. **Docs Klasörü:** `docs/` (bu proje içinde)

---

## Dil

- **Birincil Dil:** Türkçe
- **İkincil Dil:** İngilizce
- **Format:** Her doküman Türkçe ve İngilizce bölümler içerecek

---

## Dokümantasyon Yapısı (Diátaxis)

### 1. Tutorials (Öğrenme) - **TAMAMLANDI** ✅
- [x] `tutorials/getting-started.md` - Kurulum ve ilk adımlar
- [x] `tutorials/first-game.md` - Basit oyun ile öğrenme
- [x] `tutorials/ui-system.md` - UI sistemi öğrenme
- [x] `tutorials/spawner-basics.md` - Spawner temelleri
- [x] `tutorials/processflows.md` - ProcessFlows öğrenme

### 2. How-to Guides (Pratik Rehberler) - **TAMAMLANDI** ✅
- [x] `how-to-guides/install-modules.md` - Modül kurulumları
- [x] `how-to-guides/create-menu.md` - Menu oluşturma
- [x] `how-to-guides/use-spawner.md` - Spawner kullanımı
- [x] `how-to-guides/manage-processflows.md` - ProcessFlows yönetimi
- [x] `how-to-guides/configure-logging.md` - Logging yapılandırma
- [x] `how-to-guides/integrate-ads.md` - Google Ads entegrasyonu
- [x] `how-to-guides/integrate-analytics.md` - GameAnalytics entegrasyonu

### 3. Reference (API Referansı) - **TAMAMLANDI** ✅
#### Priority: Yüksek

| Doküman | Dosya | Durum |
|---------|-------|-------|
| App Manager | `reference/api/app-manager.md` | ✅ Tamamlandı |
| Lifetime Scopes | `reference/api/lifetime-scopes.md` | ✅ Tamamlandı |
| Subscription System | `reference/api/subscribable.md` | ✅ Tamamlandı |
| Menu System | `reference/api/menu-system.md` | ✅ Tamamlandı |
| Popup System | `reference/api/popup-system.md` | ✅ Tamamlandı |
| Spawner System | `reference/api/spawner.md` | ✅ Tamamlandı |
| ProcessFlows | `reference/api/processflows.md` | ✅ Tamamlandı |
| Logging | `reference/api/logging.md` | ✅ Tamamlandı |
| Configuration | `reference/api/configuration.md` | ✅ Tamamlandı |
| Utilities | `reference/api/utilities.md` | ✅ Tamamlandı |
| Google Ads | `reference/api/google-ads.md` | ✅ Tamamlandı |
| GameAnalytics | `reference/api/game-analytics.md` | ✅ Tamamlandı |
| IO/JSON | `reference/api/jsonkit.md` | ✅ Tamamlandı |

### 4. Explanation (Kavramsal) - **TAMAMLANDI** ✅
- [x] `explanation/vcontainer-di.md` - VContainer ve DI
- [x] `explanation/messagepipe-events.md` - Event sistemi
- [x] `explanation/spawner-pooling.md` - Object pooling
- [x] `explanation/async-patterns.md` - UniTask kullanımı
- [x] `explanation/project-structure.md` - Klasör yapısı

---

## Reference Doküman Şablonu

Her Reference dokümanı şu formatı izleyecek:

```markdown
# [Sınıf/Modül Adı]

> Türkçe başlık
> English title

## Genel Bakış
Türkçe açıklama...

## Methodlar / Özellikler

### `[MethodAdı]`
Açıklama...

### `[PropertyAdı]`
Açıklama...

## Kullanım Örnekleri

```csharp
// Türkçe açıklama
```

## Bkz.
- [İlgili sınıf](./other.md)
```

---

## Sonraki Adımlar

1. ✅ Plan kaydedildi
2. ✅ Reference dokümanları yazıldı (13 EN + 13 TR)
3. ⏳ GitHub Wiki ayarlanacak
4. ✅ How-to Guides yazıldı (7 EN + 7 TR)
5. ✅ Tutorials yazıldı (5 EN + 5 TR)
6. ✅ Explanation yazıldı (5 EN + 5 TR)

---

## Katkıda Bulunma

Dokümantasyona katkı için:
1. Bu plan dosyasını referans alın
2. Diátaxis formatına uyun
3. Her dokümanı Türkçe ve İngilizce hazırlayın
4. PR oluşturun
