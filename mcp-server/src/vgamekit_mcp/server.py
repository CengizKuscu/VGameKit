"""MCP server definition: tools and resources for VGameKit documentation."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import mcp.types as types
from mcp.server import Server
from mcp.server.models import InitializationOptions

from vgamekit_mcp.docs import (
    DATA_DIR,
    CATEGORY_MAP,
    DocMeta,
    load_all_docs,
    load_doc_content,
)

# Lazy imports — loaded only when semantic search is first used
_search_module: Any = None
_docs_cache: list[DocMeta] | None = None


def _get_docs() -> list[DocMeta]:
    global _docs_cache
    if _docs_cache is None:
        _docs_cache = load_all_docs()
    return _docs_cache


def _get_search():
    global _search_module
    if _search_module is None:
        from vgamekit_mcp import search as _s
        _search_module = _s
    return _search_module


def _load_known_bugs() -> list[dict]:
    path = DATA_DIR / "known_bugs.json"
    if not path.exists():
        return []
    return json.loads(path.read_text(encoding="utf-8"))


def _doc_to_dict(doc: DocMeta) -> dict:
    return {
        "doc_id": doc.doc_id,
        "title": doc.title,
        "category": doc.category,
        "summary": doc.summary,
        "uri": f"vgamekit://docs/{doc.doc_id}",
    }


def create_server() -> Server:
    server = Server("vgamekit-mcp")

    # ──────────────────────────────────────────────
    # RESOURCES
    # ──────────────────────────────────────────────

    @server.list_resources()
    async def list_resources() -> list[types.Resource]:
        docs = _get_docs()
        return [
            types.Resource(
                uri=f"vgamekit://docs/{doc.doc_id}",
                name=doc.title,
                description=doc.summary or doc.category,
                mimeType="text/markdown",
            )
            for doc in docs
        ]

    @server.read_resource()
    async def read_resource(uri: str) -> str:  # type: ignore[override]
        # uri format: vgamekit://docs/{doc_id}
        prefix = "vgamekit://docs/"
        if not uri.startswith(prefix):
            raise ValueError(f"Unknown URI: {uri}")
        doc_id = uri[len(prefix):]
        content = load_doc_content(doc_id)
        if content is None:
            raise ValueError(f"Doc not found: {doc_id}")
        return content

    # ──────────────────────────────────────────────
    # TOOLS
    # ──────────────────────────────────────────────

    @server.list_tools()
    async def list_tools() -> list[types.Tool]:
        return [
            types.Tool(
                name="list_docs",
                description=(
                    "List all available VGameKit documentation pages. "
                    "Optionally filter by Diátaxis category."
                ),
                inputSchema={
                    "type": "object",
                    "properties": {
                        "category": {
                            "type": "string",
                            "description": (
                                "Filter by category: "
                                "'tutorial' | 'how-to' | 'reference' | 'explanation' | 'home'"
                            ),
                        }
                    },
                },
            ),
            types.Tool(
                name="get_doc",
                description=(
                    "Retrieve the full markdown content of a specific VGameKit documentation page. "
                    "Accepts the doc_id (e.g. 'reference/api/app-manager') or a short slug "
                    "(e.g. 'app-manager')."
                ),
                inputSchema={
                    "type": "object",
                    "properties": {
                        "doc_id": {
                            "type": "string",
                            "description": "The doc identifier or short slug.",
                        }
                    },
                    "required": ["doc_id"],
                },
            ),
            types.Tool(
                name="search_docs",
                description=(
                    "Semantically search VGameKit documentation using natural language. "
                    "Returns the most relevant documentation chunks. "
                    "Example queries: 'how do I register a service', "
                    "'cancellation token patterns', 'open a menu'."
                ),
                inputSchema={
                    "type": "object",
                    "properties": {
                        "query": {
                            "type": "string",
                            "description": "Natural language question or keywords.",
                        },
                        "top_k": {
                            "type": "integer",
                            "description": "Number of results to return (default 5).",
                            "default": 5,
                        },
                        "category": {
                            "type": "string",
                            "description": (
                                "Optional category filter: "
                                "'tutorial' | 'how-to' | 'reference' | 'explanation'"
                            ),
                        },
                    },
                    "required": ["query"],
                },
            ),
            types.Tool(
                name="search_code_patterns",
                description=(
                    "Find canonical VGameKit C# code patterns for a specific task. "
                    "Searches only code blocks extracted from documentation. "
                    "Example tasks: 'register a singleton service', "
                    "'create a process flow', 'subscribe to an event'."
                ),
                inputSchema={
                    "type": "object",
                    "properties": {
                        "task": {
                            "type": "string",
                            "description": "What you want to do in C#.",
                        },
                        "top_k": {
                            "type": "integer",
                            "description": "Number of results to return (default 3).",
                            "default": 3,
                        },
                    },
                    "required": ["task"],
                },
            ),
            types.Tool(
                name="get_known_bugs",
                description=(
                    "Return the list of known bugs and anti-patterns in VGameKit. "
                    "Useful during code review and code generation to avoid propagating known issues."
                ),
                inputSchema={
                    "type": "object",
                    "properties": {},
                },
            ),
        ]

    @server.call_tool()
    async def call_tool(name: str, arguments: dict) -> list[types.TextContent]:
        if name == "list_docs":
            return await _tool_list_docs(arguments)
        if name == "get_doc":
            return await _tool_get_doc(arguments)
        if name == "search_docs":
            return await _tool_search_docs(arguments)
        if name == "search_code_patterns":
            return await _tool_search_code_patterns(arguments)
        if name == "get_known_bugs":
            return await _tool_get_known_bugs()
        raise ValueError(f"Unknown tool: {name}")

    return server


# ──────────────────────────────────────────────
# TOOL HANDLERS
# ──────────────────────────────────────────────

async def _tool_list_docs(args: dict) -> list[types.TextContent]:
    docs = _get_docs()
    category_filter = args.get("category")
    if category_filter:
        docs = [d for d in docs if d.category == category_filter]
    result = [_doc_to_dict(d) for d in docs]
    return [types.TextContent(type="text", text=json.dumps(result, indent=2))]


async def _tool_get_doc(args: dict) -> list[types.TextContent]:
    doc_id: str = args["doc_id"]
    content = load_doc_content(doc_id)

    # If not found by doc_id, try slug match via metadata
    if content is None:
        docs = _get_docs()
        slug = doc_id.split("/")[-1]
        matches = [d for d in docs if d.doc_id.split("/")[-1] == slug]
        if matches:
            doc_id = matches[0].doc_id
            content = load_doc_content(doc_id)

    if content is None:
        available = [d.doc_id for d in _get_docs()]
        return [types.TextContent(
            type="text",
            text=json.dumps({
                "error": f"Doc '{doc_id}' not found.",
                "available_doc_ids": available,
            }, indent=2),
        )]

    # Find metadata for related docs
    docs = _get_docs()
    meta = next((d for d in docs if d.doc_id == doc_id), None)

    result = {
        "doc_id": doc_id,
        "title": meta.title if meta else doc_id,
        "category": meta.category if meta else "unknown",
        "uri": f"vgamekit://docs/{doc_id}",
        "related": meta.related if meta else [],
        "content": content,
    }
    return [types.TextContent(type="text", text=json.dumps(result, indent=2))]


async def _tool_search_docs(args: dict) -> list[types.TextContent]:
    query: str = args["query"]
    top_k: int = int(args.get("top_k", 5))
    category: str | None = args.get("category")

    search = _get_search()
    results = search.search_docs(query, top_k=top_k, category=category)
    return [types.TextContent(type="text", text=json.dumps(results, indent=2))]


async def _tool_search_code_patterns(args: dict) -> list[types.TextContent]:
    task: str = args["task"]
    top_k: int = int(args.get("top_k", 3))

    search = _get_search()
    results = search.search_code(task, top_k=top_k)
    return [types.TextContent(type="text", text=json.dumps(results, indent=2))]


async def _tool_get_known_bugs() -> list[types.TextContent]:
    bugs = _load_known_bugs()
    return [types.TextContent(type="text", text=json.dumps(bugs, indent=2))]
