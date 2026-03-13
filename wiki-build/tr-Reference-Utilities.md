# Utilities Referansı

**Ad Alanı:** `VGameKit.Runtime.Utilities` / `VGameKit.Runtime.Utilities.Camera`
**Derleme:** `VGameKit.Runtime`
**Dosyalar:** `Assets/VGameKit/Runtime/Utilities/`

---

## Genel Bakış

Animasyon, renk, listeler, rastgelelik, titreşim, sayı biçimlendirme, bileşen yönetimi, HSB renk matematiği ve ortografik kamera düzeni konularını kapsayan statik uzantı sınıfları ve yardımcılar koleksiyonu.

---

## `AnimatorExtensions`

```csharp
public static class AnimatorExtensions
```

| Metot | Açıklama |
|---|---|
| `ResetAllTriggers(this Animator animator)` | Tüm animatör parametrelerini dolaşır ve her `Trigger` parametresinde `ResetTrigger()` çağırır |

```csharp
_animator.ResetAllTriggers();
```

---

## `MatrixId`

```csharp
[Serializable]
public class MatrixId
```

2 boyutlu tam sayı ızgara koordinatı.

| Özellik | Tip | Erişim |
|---|---|---|
| `RowId` | `int` | `public get / private set` (`[field: SerializeField]`) |
| `ColId` | `int` | `public get / private set` (`[field: SerializeField]`) |

| Operatör | Açıklama |
|---|---|
| `+` | Bileşen bazlı toplama |
| `*` (skalar) | Bileşen bazlı skalar çarpma |
| `==` | Her iki `RowId` ve `ColId` eşit |
| `!=` | **Bilinen hata:** gövde `==` ile aynı; eşit bileşenler için her zaman `true` döndürür |

> **Hata:** `!=` operatörü bozuk — `==` ile aynı sonucu döndürür. `MatrixId` karşılaştırmalarında `!=` operatörüne güvenmeyin.

```csharp
var a = new MatrixId(1, 2);
var b = new MatrixId(1, 2);
bool equal    = a == b;  // true  (doğru)
bool notEqual = a != b;  // true  (HATA — false olmalı)
```

---

## `SerializableVector3` + `Vector3Extensions`

```csharp
[Serializable]
public class SerializableVector3  // alanlar: public float x, y, z
```

```csharp
public static class Vector3Extensions
```

| Metot | Açıklama |
|---|---|
| `ToVector3(this SerializableVector3)` | `UnityEngine.Vector3`'e dönüştürür |
| `FromVector3(this Vector3)` | `SerializableVector3`'e dönüştürür |

`JsonConvert` tarafından seri hale getirilemeyen `Vector3`'ün yerini alması için ScriptableObject'lerde veya JSON yüklerinde `SerializableVector3` kullanın.

---

## `ListExtensions`

```csharp
public static class ListExtensions
```

| Metot | Açıklama |
|---|---|
| `Shuffle<T>(this IList<T>)` | `UnityEngine.Random` kullanan Fisher-Yates karıştırma |
| `Shuffle<T>(this IList<T>, int seed)` | Tohumlu `System.Random` ile Fisher-Yates |
| `ShuffleRestricted<T>(this IList<T>, int restrictionIndex)` | `>= restrictionIndex` indeksindeki öğeleri karıştırır |
| `ShuffleRestricted<T>(this IList<T>, int restrictionIndex, int seed)` | Aynısı, tohumlu |
| `GetRandomItem<T>(this IList<T>, int seed)` | Tohumlu `System.Random` kullanarak rastgele öğe döndürür |
| `DictReverse<T,K>(this Dictionary<T,K>)` | Değerleri ters sırada olan yeni sözlük döndürür |

---

## `ColorExtensions`

```csharp
public static class ColorExtensions
```

| Metot | Açıklama |
|---|---|
| `SetAlpha(this Graphic graphic, float alpha)` | `graphic.color.a` değerini ayarlar |
| `SetAlpha(this Material material, float alpha)` | `material.color.a` değerini ayarlar |

```csharp
_image.SetAlpha(0.5f);
_material.SetAlpha(0f);
```

---

## `RandomExtensions`

`int` üzerindeki tüm uzantı metotları (tohum olarak kullanılır).

```csharp
public static class RandomExtensions
```

| Metot | Açıklama |
|---|---|
| `Random(this int seed, int min, int max)` | `System.Random(seed).Next(min, max)` |
| `Random(this int seed, float min, float max)` | `[min, max)` aralığında doğrusal float |
| `Random(this int seed)` | `[0, 1)` aralığında float |
| `TriangularRandom(this int seed, int min, int max, double weight)` | Üçgen dağılım |
| `CumulativeRandom(this int seed, double[] values, double[] weights)` | Ağırlıklı kümülatif seçim |
| `Roll(this int seed, int min, int max, int target, double weight)` | Hedef değere doğru ağırlıklı harmanlama |

```csharp
int seed = 42;
int roll = seed.Random(1, 7);          // 1d6
float prob = seed.Random(0f, 1f);      // olasılık
int biased = seed.Roll(1, 10, 5, 0.7); // 5'e doğru önyargılı
```

---

## `HapticSupport`

```csharp
public static class HapticSupport
```

| Özellik | Açıklama |
|---|---|
| `SupportHaptic` | Geç önbelleklenir; `SystemInfo.supportsVibration` okur; Editor'da her zaman `true` |

```csharp
if (HapticSupport.SupportHaptic)
{
    Handheld.Vibrate();
}
```

---

## `AbbreviationNumber`

```csharp
public class AbbreviationNumber
```

| Üye | Açıklama |
|---|---|
| `Abbreviations` | `SortedDictionary<int, string>` — `{10000:"K", 1000000:"M", 1000000000:"B"}` |
| `AbbreviateNumber(float number)` | Float'ı kısaltılmış dizeye biçimlendirir |

```csharp
AbbreviationNumber.AbbreviateNumber(15000f)   // "1.5K"
AbbreviationNumber.AbbreviateNumber(2500000f) // "2.5M"
AbbreviationNumber.AbbreviateNumber(999f)     // "999"
```

---

## `GetOrAddComponentUtility`

```csharp
public static class GetOrAddComponentUtility
```

| Metot | Açıklama |
|---|---|
| `GetOrAddComponent<T>(this GameObject)` | Mevcut bileşeni döndürür ya da yoksa yeni ekler |
| `RemoveComponent<T>(this GameObject)` | Bileşeni alır; bulunursa yok eder |

```csharp
var rb = gameObject.GetOrAddComponent<Rigidbody>();
gameObject.RemoveComponent<Collider>();
```

---

## `HSBColor`

```csharp
[Serializable]
public struct HSBColor
```

Alpha dahil HSB (Ton, Doygunluk, Parlaklık) rengi. Tüm bileşenler `[0, 1]` aralığında `float`'tır.

| Metot | Açıklama |
|---|---|
| `FromColor(Color)` | RGB → HSB dönüşümü |
| `ToColor(HSBColor)` | HSB → RGB dönüşümü |
| `Lerp(HSBColor a, HSBColor b, float t)` | HSB uzayında interpolasyon (ton için `LerpAngle` kullanır) |
| `LerpViaHSB(Color a, Color b, float t)` | Tam RGB → HSB → interpolasyon → RGB işlem hattı |
| `Test()` | Bilinen dönüşüm çiftlerini loglar (yalnızca hata ayıklama) |

```csharp
// HSB uzayında renk geçişi
Color blended = HSBColor.LerpViaHSB(Color.red, Color.blue, 0.5f);
```

---

## `OrthographicLayout` *(Kamera)*

```csharp
public class OrthographicLayout
```

Ortografik kameralar için statik ızgara düzeni hesaplayıcısı.

| Metot | Açıklama |
|---|---|
| `Initialize(int row, float aspect, Padding padding, float2 unitSize)` | `Column` ve `OrthographicSize`'ı otomatik hesaplar |
| `Initialize(int row, int column, float aspect, Padding padding, float2 unitSize)` | Sabit sütun sayısı; `OrthographicSize` hesaplar |

**Statik Özellikler:** `Aspect`, `OrthographicSize`, `Row`, `Column`, `Padding`, `Origin`, `UnitSize`

> **Not:** Tüm durum statiktir — aynı anda yalnızca bir düzen aktif olabilir.

```csharp
OrthographicLayout.Initialize(
    row: 5,
    aspect: Camera.main.aspect,
    padding: new Padding(0.1f, 0.1f, 0.1f, 0.1f),
    unitSize: new float2(1f, 1f));

Camera.main.orthographicSize = OrthographicLayout.OrthographicSize;
```

---

## `Padding` *(Kamera)*

```csharp
[Serializable]
public struct Padding
```

| Özellik | Tip |
|---|---|
| `Top` | `float` |
| `Bottom` | `float` |
| `Left` | `float` |
| `Right` | `float` |

---

## Ayrıca Bakınız

- `GKLog` — kayıt yardımcısı
- `JSonKit` — `SerializableVector3` ile JSON kalıcılığı
