# VGameKit Dokümantasyon Planı

> **Son Güncelleme:** 10 Mart 2026
> **Durum:** Plan aşamasında - Reference ile başlanacak

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

### 1. Tutorials (Öğrenme) - Sonraki Aşamada
- [ ] `tutorials/getting-started.md` - Kurulum ve ilk adımlar
- [ ] `tutorials/first-game.md` - Basit oyun ile öğrenme
- [ ] `tutorials/ui-system.md` - UI sistemi öğrenme
- [ ] `tutorials/spawner-basics.md` - Spawner temelleri
- [ ] `tutorials/processflows.md` - ProcessFlows öğrenme

### 2. How-to Guides (Pratik Rehberler) - Sonraki Aşamada
- [ ] `how-to-guides/install-modules.md` - Modül kurulumları
- [ ] `how-to-guides/create-menu.md` - Menu oluşturma
- [ ] `how-to-guides/use-spawner.md` - Spawner kullanımı
- [ ] `how-to-guides/manage-processflows.md` - ProcessFlows yönetimi
- [ ] `how-to-guides/configure-logging.md` - Logging yapılandırma
- [ ] `how-to-guides/integrate-ads.md` - Google Ads entegrasyonu
- [ ] `how-to-guides/integrate-analytics.md` - GameAnalytics entegrasyonu

### 3. Reference (API Referansı) - **ŞİMDİ BAŞLANACAK**
#### Priority: Yüksek

| Doküman | Dosya | Durum |
|---------|-------|-------|
| App Manager | `reference/api/app-manager.md` | ⏳ Planlandı |
| Lifetime Scopes | `reference/api/lifetime-scopes.md` | ⏳ Planlandı |
| Subscription System | `reference/api/subscribable.md` | ⏳ Planlandı |
| Menu System | `reference/api/menu-system.md` | ⏳ Planlandı |
| Popup System | `reference/api/popup-system.md` | ⏳ Planlandı |
| Spawner System | `reference/api/spawner.md` | ⏳ Planlandı |
| ProcessFlows | `reference/api/processflows.md` | ⏳ Planlandı |
| Logging | `reference/api/logging.md` | ⏳ Planlandı |
| Configuration | `reference/api/configuration.md` | ⏳ Planlandı |
| Utilities | `reference/api/utilities.md` | ⏳ Planlandı |
| Google Ads | `reference/api/google-ads.md` | ⏳ Planlandı |
| GameAnalytics | `reference/api/game-analytics.md` | ⏳ Planlandı |
| IO/JSON | `reference/api/jsonkit.md` | ⏳ Planlandı |

### 4. Explanation (Kavramsal) - Sonraki Aşamada
- [ ] `explanation/vcontainer-di.md` - VContainer ve DI
- [ ] `explanation/messagepipe-events.md` - Event sistemi
- [ ] `explanation/spawner-pooling.md` - Object pooling
- [ ] `explanation/async-patterns.md` - UniTask kullanımı
- [ ] `explanation/project-structure.md` - Klasör yapısı

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
2. ⏳ Reference dokümanları yazılacak (Priority)
3. ⏳ GitHub Wiki ayarlanacak
4. ⏳ How-to Guides yazılacak
5. ⏳ Tutorials yazılacak
6. ⏳ Explanation yazılacak

---

## Katkıda Bulunma

Dokümantasyona katkı için:
1. Bu plan dosyasını referans alın
2. Diátaxis formatına uyun
3. Her dokümanı Türkçe ve İngilizce hazırlayın
4. PR oluşturun
