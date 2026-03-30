# VGameKit

VGameKit is a modular Unity framework designed to streamline game development by providing a collection of pre-built tools and systems. It is built on top of three best-in-class libraries for Unity game development:

| Library | Role |
|---------|------|
| [VContainer](https://vcontainer.hadashikick.jp/) | Dependency injection and object lifetime management |
| [UniTask](https://github.com/Cysharp/UniTask) | High-performance async/await for Unity |
| [MessagePipe](https://github.com/Cysharp/MessagePipe) | High-performance in-process messaging and pub/sub |

VGameKit handles the wiring between these libraries so you can focus on game logic. For advanced usage beyond what VGameKit exposes, visiting each library's documentation directly is recommended.

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

---

## AI Assistant (MCP Server)

`vgamekit-mcp` is an MCP server that gives AI tools (Claude Code, Cursor, etc.) direct access to VGameKit documentation and code patterns. Instead of guessing API signatures or copy-pasting from the wiki, your AI assistant can search and retrieve accurate, up-to-date information automatically.

**What it provides:**

| Tool | Description |
|------|-------------|
| `search_docs` | Semantic search across all documentation ("how do I register a service") |
| `get_doc` | Retrieve the full content of a specific documentation page |
| `list_docs` | List all pages, optionally filtered by category |
| `search_code_patterns` | Semantic search over C# code examples ("create a process flow") |
| `get_known_bugs` | Return the list of known bugs and anti-patterns |

No API keys required. Semantic search uses a local model (`all-MiniLM-L6-v2`, ~90 MB downloaded once on first use).

### Installation

Requires [uv](https://docs.astral.sh/uv/getting-started/installation/). Add the following to your project's `.mcp.json`:

```json
{
  "mcpServers": {
    "vgamekit": {
      "command": "uvx",
      "args": ["vgamekit-mcp"]
    }
  }
}
```

Restart your AI tool. On first run the embedding model is downloaded (~90 MB, one time only).

### Example queries

- *"How do I register a service in VGameKit?"*
- *"Show me how to use AbsAppManager."*
- *"Give me a code example for creating a process flow."*
- *"What are the known bugs in VGameKit?"*
