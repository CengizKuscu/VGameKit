# VGameKit

VGameKit is a modular Unity framework designed to streamline game development by providing a collection of pre-built tools and systems.

> Full documentation: **[VGameKit Wiki](https://github.com/CengizKuscu/VGameKit/wiki)**

---

## Modules

### VGameKit

The core module. Provides application lifecycle management, DI-based architecture, UI systems, process flows, object pooling, logging, and utilities.

**Install via `Packages/manifest.json`:**
```json
"com.cngz.vgamekit": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.4"
```

[Reference](https://github.com/CengizKuscu/VGameKit/wiki/en-Reference-App-Manager) · [Getting Started](https://github.com/CengizKuscu/VGameKit/wiki/en-Tutorial-Getting-Started) · [Install Guide](https://github.com/CengizKuscu/VGameKit/wiki/en-HowTo-Install-Modules)

---

### VGameKit.IO

JSON serialization and deserialization helpers. Provides `JSonKit` for consistent read/write operations across platforms.

**Install via `Packages/manifest.json`:**
```json
"com.cngz.vgamekit.io": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.IO#v0.0.4"
```

[Reference](https://github.com/CengizKuscu/VGameKit/wiki/en-Reference-JSONKit)

---

### VGameKit.GA

GameAnalytics integration. Tracks player behavior and game events via `GA_Initialization`.

**Install via `Packages/manifest.json`:**
```json
"com.cngz.vgamekit.ga": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GA#v0.0.4"
```

[Reference](https://github.com/CengizKuscu/VGameKit/wiki/en-Reference-Game-Analytics) · [Integration Guide](https://github.com/CengizKuscu/VGameKit/wiki/en-HowTo-Integrate-Analytics)

---

### VGameKit.GoogleAds

Google Mobile Ads integration. Supports Banner, Interstitial, and Rewarded ads with built-in consent management (GDPR) and Unity Editor tooling.

**Install via `Packages/manifest.json`:**
```json
"com.cngz.vgamekit.googleads": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GoogleAds#v0.0.4"
```

[Reference](https://github.com/CengizKuscu/VGameKit/wiki/en-Reference-Google-Ads) · [Integration Guide](https://github.com/CengizKuscu/VGameKit/wiki/en-HowTo-Integrate-Ads)
