# How to Install VGameKit Modules

## Overview

VGameKit is split into four assembly-definition modules. Each module has its own package dependencies. This guide walks through adding them to a Unity 6 project.

**Prerequisites:** Unity 6000.3.7f1, OpenUPM CLI or manual `manifest.json` editing.

---

## Modules

| Module | Assembly | Purpose |
|--------|----------|---------|
| `VGameKit.Runtime` | `VGameKit.Runtime.asmdef` | Core runtime: DI scopes, menus, spawner, process flows, logging |
| `VGameKit.IO.Runtime` | `VGameKit.IO.Runtime.asmdef` | JSON persistence via `JSonKit` |
| `VGameKit.GA.Runtime` | `VGameKit.GA.Runtime.asmdef` | GameAnalytics integration |
| `VGameKit.GoogleAds.Runtime` | `VGameKit.GoogleAds.Runtime.asmdef` | Google Mobile Ads integration |

---

## Step 1 — Add the scoped registry

Open `Packages/manifest.json` and add the OpenUPM scoped registry if it is not already present:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.cysharp",
        "jp.hadashikick.vcontainer",
        "jillejr.newtonsoft.json-for-unity",
        "com.google",
        "com.gameanalytics"
      ]
    }
  ]
}
```

---

## Step 2 — Add core dependencies

All modules depend on the following packages. Add them to the `dependencies` block:

```json
{
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.17.0",
    "com.cysharp.unitask": "2.5.10",
    "com.cysharp.messagepipe": "1.8.1",
    "com.cysharp.messagepipe.vcontainer": "1.8.1",
    "jillejr.newtonsoft.json-for-unity": "13.0.102"
  }
}
```

---

## Step 3 — Add module-specific dependencies

### VGameKit.GA (GameAnalytics)

```json
"com.gameanalytics.sdk": "7.10.5"
```

The `VGameKit.GA.Runtime` asmdef defines `GA_ENABLED` automatically when this package is present.

### VGameKit.GoogleAds (Google Mobile Ads)

```json
"com.google.ads.mobile": "10.3.0",
"com.google.external-dependency-manager": "1.2.186"
```

`VGameKit.GoogleAds.Runtime` also defines `GA_ENABLED` when `com.gameanalytics.sdk` is detected.

---

## Step 4 — Add VGameKit modules

Install modules via **Package Manager > + > Add package from Git URL**, or add them directly to the `dependencies` block in `Packages/manifest.json`.

### Via manifest.json (recommended — add only the modules you need)

```json
{
  "dependencies": {
    "com.cngz.vgamekit":            "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.4",
    "com.cngz.vgamekit.io":         "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.IO#v0.0.4",
    "com.cngz.vgamekit.ga":         "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GA#v0.0.4",
    "com.cngz.vgamekit.googleads":  "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GoogleAds#v0.0.4"
  }
}
```

- `com.cngz.vgamekit` — always required (core runtime)
- `com.cngz.vgamekit.io` — required for JSON persistence
- `com.cngz.vgamekit.ga` — optional, requires GameAnalytics SDK
- `com.cngz.vgamekit.googleads` — optional, requires Google Mobile Ads SDK

### Via Package Manager UI

1. Open **Window > Package Manager**.
2. Click **+** → **Add package from Git URL**.
3. Paste the Git URL for each module you need (from the list above).

Unity will fetch the package directly from GitHub. No files need to be copied manually.

---

## Conditional compilation symbols

| Symbol | Set by | Effect |
|--------|--------|--------|
| `VGameKIT_GA` | `VGameKit.Runtime.asmdef` when `com.gameanalytics.sdk` is present | Enables GA hooks in the core runtime |
| `GA_ENABLED` | `VGameKit.GoogleAds.Runtime.asmdef` when `com.gameanalytics.sdk` is present | Enables GA event logging inside the Ads module |
| `GAMEKIT_LOG` | Must be added manually in **Player Settings > Scripting Define Symbols** | Activates `GKLog.Log` calls; strip from release builds |

---

## See also

- [Logging reference](../reference/api/logging.md)
- [Configuration reference](../reference/api/configuration.md)
- [Lifetime Scopes reference](../reference/api/lifetime-scopes.md)
