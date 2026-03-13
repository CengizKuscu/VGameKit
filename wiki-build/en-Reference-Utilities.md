# Utilities Reference

**Namespace:** `VGameKit.Runtime.Utilities` / `VGameKit.Runtime.Utilities.Camera`
**Assembly:** `VGameKit.Runtime`
**Files:** `Assets/VGameKit/Runtime/Utilities/`

---

## Overview

A collection of static extension classes and helpers covering animation, color, lists, randomness, haptics, number formatting, component management, HSB color math, and orthographic camera layout.

---

## `AnimatorExtensions`

```csharp
public static class AnimatorExtensions
```

| Method | Description |
|---|---|
| `ResetAllTriggers(this Animator animator)` | Iterates all animator parameters and calls `ResetTrigger()` on each `Trigger` parameter |

```csharp
_animator.ResetAllTriggers();
```

---

## `MatrixId`

```csharp
[Serializable]
public class MatrixId
```

A 2D integer grid coordinate.

| Property | Type | Access |
|---|---|---|
| `RowId` | `int` | `public get / private set` (`[field: SerializeField]`) |
| `ColId` | `int` | `public get / private set` (`[field: SerializeField]`) |

| Operator | Description |
|---|---|
| `+` | Component-wise addition |
| `*` (scalar) | Component-wise scalar multiplication |
| `==` | Both `RowId` and `ColId` equal |
| `!=` | **Known bug:** body is identical to `==`; always returns `true` when equal |

> **Bug:** The `!=` operator is broken — it returns the same result as `==`. Do not rely on `!=` for `MatrixId` comparisons.

```csharp
var a = new MatrixId(1, 2);
var b = new MatrixId(1, 2);
bool equal   = a == b;  // true  (correct)
bool notEqual = a != b; // true  (BUG — should be false)
```

---

## `SerializableVector3` + `Vector3Extensions`

```csharp
[Serializable]
public class SerializableVector3  // fields: public float x, y, z
```

```csharp
public static class Vector3Extensions
```

| Method | Description |
|---|---|
| `ToVector3(this SerializableVector3)` | Converts to `UnityEngine.Vector3` |
| `FromVector3(this Vector3)` | Converts to `SerializableVector3` |

Use `SerializableVector3` in ScriptableObjects or JSON payloads where `Vector3` is not serializable by `JsonConvert`.

---

## `ListExtensions`

```csharp
public static class ListExtensions
```

| Method | Description |
|---|---|
| `Shuffle<T>(this IList<T>)` | Fisher-Yates shuffle using `UnityEngine.Random` |
| `Shuffle<T>(this IList<T>, int seed)` | Fisher-Yates with seeded `System.Random` |
| `ShuffleRestricted<T>(this IList<T>, int restrictionIndex)` | Shuffles elements at index `>= restrictionIndex` |
| `ShuffleRestricted<T>(this IList<T>, int restrictionIndex, int seed)` | Same, seeded |
| `GetRandomItem<T>(this IList<T>, int seed)` | Returns a random element using seeded `System.Random` |
| `DictReverse<T,K>(this Dictionary<T,K>)` | Returns new dict with values in reversed order |

---

## `ColorExtensions`

```csharp
public static class ColorExtensions
```

| Method | Description |
|---|---|
| `SetAlpha(this Graphic graphic, float alpha)` | Sets `graphic.color.a` |
| `SetAlpha(this Material material, float alpha)` | Sets `material.color.a` |

```csharp
_image.SetAlpha(0.5f);
_material.SetAlpha(0f);
```

---

## `RandomExtensions`

All extension methods on `int` (used as a seed).

```csharp
public static class RandomExtensions
```

| Method | Description |
|---|---|
| `Random(this int seed, int min, int max)` | `System.Random(seed).Next(min, max)` |
| `Random(this int seed, float min, float max)` | Linear float in `[min, max)` |
| `Random(this int seed)` | Float in `[0, 1)` |
| `TriangularRandom(this int seed, int min, int max, double weight)` | Triangular distribution |
| `CumulativeRandom(this int seed, double[] values, double[] weights)` | Weighted cumulative selection |
| `Roll(this int seed, int min, int max, int target, double weight)` | Weighted blend toward target value |

```csharp
int seed = 42;
int roll = seed.Random(1, 7);          // 1d6
float prob = seed.Random(0f, 1f);      // probability
int biased = seed.Roll(1, 10, 5, 0.7); // biased toward 5
```

---

## `HapticSupport`

```csharp
public static class HapticSupport
```

| Property | Description |
|---|---|
| `SupportHaptic` | Lazily cached; reads `SystemInfo.supportsVibration`; always `true` in Editor |

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

| Member | Description |
|---|---|
| `Abbreviations` | `SortedDictionary<int, string>` — `{10000:"K", 1000000:"M", 1000000000:"B"}` |
| `AbbreviateNumber(float number)` | Formats a float as abbreviated string |

```csharp
AbbreviationNumber.AbbreviateNumber(15000f)  // "1.5K"
AbbreviationNumber.AbbreviateNumber(2500000f) // "2.5M"
AbbreviationNumber.AbbreviateNumber(999f)     // "999"
```

---

## `GetOrAddComponentUtility`

```csharp
public static class GetOrAddComponentUtility
```

| Method | Description |
|---|---|
| `GetOrAddComponent<T>(this GameObject)` | Returns existing component or adds a new one |
| `RemoveComponent<T>(this GameObject)` | Gets component; destroys it if found |

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

HSB (Hue, Saturation, Brightness) color with alpha. All components are `float` in `[0, 1]`.

| Method | Description |
|---|---|
| `FromColor(Color)` | RGB → HSB conversion |
| `ToColor(HSBColor)` | HSB → RGB conversion |
| `Lerp(HSBColor a, HSBColor b, float t)` | HSB-space lerp (uses `LerpAngle` for hue) |
| `LerpViaHSB(Color a, Color b, float t)` | Full RGB → HSB → lerp → RGB pipeline |
| `Test()` | Logs known conversion pairs (debug only) |

```csharp
// Smooth color transition in HSB space
Color blended = HSBColor.LerpViaHSB(Color.red, Color.blue, 0.5f);
```

---

## `OrthographicLayout` *(Camera)*

```csharp
public class OrthographicLayout
```

Static grid layout calculator for orthographic cameras.

| Method | Description |
|---|---|
| `Initialize(int row, float aspect, Padding padding, float2 unitSize)` | Calculates `Column` and `OrthographicSize` automatically |
| `Initialize(int row, int column, float aspect, Padding padding, float2 unitSize)` | Fixed column count; calculates `OrthographicSize` |

**Static Properties:** `Aspect`, `OrthographicSize`, `Row`, `Column`, `Padding`, `Origin`, `UnitSize`

> **Note:** All state is static — only one layout can be active at a time.

```csharp
OrthographicLayout.Initialize(
    row: 5,
    aspect: Camera.main.aspect,
    padding: new Padding(0.1f, 0.1f, 0.1f, 0.1f),
    unitSize: new float2(1f, 1f));

Camera.main.orthographicSize = OrthographicLayout.OrthographicSize;
```

---

## `Padding` *(Camera)*

```csharp
[Serializable]
public struct Padding
```

| Property | Type |
|---|---|
| `Top` | `float` |
| `Bottom` | `float` |
| `Left` | `float` |
| `Right` | `float` |

---

## See Also

- `GKLog` — logging utility
- `JSonKit` — JSON persistence with `SerializableVector3`
