# vgamekit-mcp

MCP server for [VGameKit](https://github.com/cengizkuscu/VGameKit) — provides searchable documentation and tools for Unity developers using VGameKit.

No API keys required. Semantic search uses a local model (`all-MiniLM-L6-v2`, ~90 MB downloaded once on first use).

---

## Installation

### Requirements

- [uv](https://docs.astral.sh/uv/getting-started/installation/) installed
- Claude Code, Cursor, or any other MCP-compatible AI tool

### 1. Install uv (if you haven't already)

**macOS / Linux:**
```bash
curl -LsSf https://astral.sh/uv/install.sh | sh
```

**Windows:**
```powershell
powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
```

### 2. Add to your `.mcp.json`

In your project's root directory, add the following to `.mcp.json` (create the file if it doesn't exist):

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

If you already have other MCP servers configured, just add the `"vgamekit"` block inside your existing `"mcpServers"` object.

### 3. Restart your AI tool

Restart Claude Code or whichever tool you use for the `.mcp.json` changes to take effect.

On first run, the embedding model is downloaded (~90 MB, one time only). Subsequent starts are instant.

---

## Tools

| Tool | Description |
|---|---|
| `search_docs` | Semantic search across all documentation ("how do I register a service") |
| `get_doc` | Retrieve the full content of a specific documentation page |
| `list_docs` | List all pages, optionally filtered by category |
| `search_code_patterns` | Semantic search over C# code examples ("create a process flow") |
| `get_known_bugs` | Return the list of known bugs and anti-patterns in VGameKit |

---

## Example usage

You can ask your AI assistant things like:

- *"How do I register a service in VGameKit?"*
- *"Show me how to use AbsAppManager."*
- *"Give me a code example for creating a process flow."*
- *"What are the known bugs in VGameKit?"*

---

## Troubleshooting

**`uvx: command not found`** — uv is not installed. Follow the installation step above.

**Slow first search** — The embedding model is being downloaded (~90 MB). This happens once. Subsequent queries are instant.

**Server not starting** — Check your `.mcp.json` syntax. The file must be valid JSON.
