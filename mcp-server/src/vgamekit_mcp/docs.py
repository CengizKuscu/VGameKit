"""Documentation loading, metadata parsing, and chunking."""

from __future__ import annotations

import json
import re
from dataclasses import dataclass, field
from pathlib import Path

DATA_DIR = Path(__file__).parent / "data"
DOCS_DIR = DATA_DIR / "docs"

CATEGORY_MAP = {
    "tutorials": "tutorial",
    "how-to": "how-to",
    "reference": "reference",
    "explanation": "explanation",
}

# Tokens are approximated as word count * 1.3
_WORDS_PER_TOKEN = 1.3
_MIN_CHUNK_WORDS = 60   # merge chunks shorter than this with previous
_MAX_CHUNK_WORDS = 380  # hard cap (~500 tokens)


@dataclass
class DocMeta:
    doc_id: str       # e.g. "reference/api/app-manager"
    title: str
    category: str     # tutorial | how-to | reference | explanation | home
    path: Path
    summary: str = ""
    related: list[str] = field(default_factory=list)


@dataclass
class Chunk:
    chunk_id: str       # "{doc_id}#{index}"
    doc_id: str
    heading: str        # level-2 heading text (empty for intro chunk)
    text: str           # full chunk markdown including heading line
    is_code_block: bool = False
    code_language: str = ""


def _doc_id_from_path(path: Path) -> str:
    rel = path.relative_to(DOCS_DIR)
    parts = list(rel.parts)
    # Strip .md extension from last part
    parts[-1] = parts[-1].removesuffix(".md")
    if parts == ["wiki_home"]:
        return "home"
    return "/".join(parts)


def _category_from_doc_id(doc_id: str) -> str:
    if doc_id == "home":
        return "home"
    top = doc_id.split("/")[0]
    return CATEGORY_MAP.get(top, "reference")


def _extract_title(content: str) -> str:
    for line in content.splitlines():
        line = line.strip()
        if line.startswith("# "):
            return line[2:].strip()
    return ""


def _extract_summary(content: str) -> str:
    """First non-heading, non-empty paragraph after the title."""
    lines = content.splitlines()
    para_lines: list[str] = []
    for line in lines:
        stripped = line.strip()
        if stripped.startswith("#"):
            if para_lines:
                break
            continue
        if stripped.startswith(">"):
            continue  # skip blockquotes (e.g. language switcher)
        if stripped == "---":
            if para_lines:
                break
            continue
        if stripped == "":
            if para_lines:
                break
            continue
        para_lines.append(stripped)
    return " ".join(para_lines)


def _extract_related(content: str) -> list[str]:
    """Parse 'See Also' section for internal doc links."""
    related: list[str] = []
    in_see_also = False
    for line in content.splitlines():
        stripped = line.strip()
        if re.match(r"^##\s*See [Aa]lso", stripped):
            in_see_also = True
            continue
        if in_see_also:
            if stripped.startswith("##"):
                break
            # Match markdown links like [Title](path) where path doesn't start with http
            for m in re.finditer(r"\[([^\]]+)\]\(([^)]+)\)", stripped):
                href = m.group(2)
                if href.startswith("http"):
                    continue
                # Convert relative href to doc_id
                # Remove .md, strip leading ../
                href = re.sub(r"^(\.\./)+", "", href)
                href = href.removesuffix(".md")
                # Normalize: if it points into reference/api/ etc., keep as-is
                related.append(href)
    return related


def _split_into_chunks(doc_id: str, content: str) -> list[Chunk]:
    """
    Split a document into chunks on level-2 headings.
    Short consecutive chunks are merged. Code fences are preserved.
    """
    # Split on ## headings
    raw_sections: list[tuple[str, str]] = []  # (heading, body)
    current_heading = ""
    current_lines: list[str] = []

    for line in content.splitlines(keepends=True):
        if re.match(r"^## ", line):
            raw_sections.append((current_heading, "".join(current_lines)))
            current_heading = line.lstrip("#").strip().rstrip()
            current_lines = [line]
        else:
            current_lines.append(line)
    raw_sections.append((current_heading, "".join(current_lines)))

    # Merge short sections with the previous one
    merged: list[tuple[str, str]] = []
    for heading, body in raw_sections:
        words = len(body.split())
        if merged and words < _MIN_CHUNK_WORDS:
            prev_h, prev_b = merged[-1]
            merged[-1] = (prev_h, prev_b + body)
        else:
            merged.append((heading, body))

    # Build Chunk objects
    chunks: list[Chunk] = []
    for i, (heading, body) in enumerate(merged):
        if not body.strip():
            continue
        chunk_id = f"{doc_id}#{i}"
        chunks.append(Chunk(
            chunk_id=chunk_id,
            doc_id=doc_id,
            heading=heading,
            text=body,
        ))

    return chunks


def _extract_code_chunks(doc_id: str, content: str) -> list[Chunk]:
    """Extract each fenced code block as a standalone Chunk for the code index."""
    chunks: list[Chunk] = []
    pattern = re.compile(r"```(\w*)\n(.*?)```", re.DOTALL)

    for i, m in enumerate(pattern.finditer(content)):
        lang = m.group(1) or "text"
        code = m.group(2).strip()
        if not code or len(code.split()) < 5:
            continue
        # Find the paragraph immediately before the code block for context
        before = content[: m.start()].rsplit("\n\n", 1)[-1].strip()
        # Remove markdown heading markers
        context = re.sub(r"^#+\s*", "", before, flags=re.MULTILINE).strip()
        context_snippet = context[-200:] if len(context) > 200 else context
        text = f"{context_snippet}\n```{lang}\n{code}\n```" if context_snippet else f"```{lang}\n{code}\n```"
        chunks.append(Chunk(
            chunk_id=f"{doc_id}#code{i}",
            doc_id=doc_id,
            heading=context_snippet[:80] if context_snippet else "",
            text=text,
            is_code_block=True,
            code_language=lang,
        ))
    return chunks


def load_all_docs() -> list[DocMeta]:
    """
    Load metadata for all docs.
    Uses pre-built catalogue.json when available (fast path),
    falls back to parsing DOCS_DIR at runtime.
    """
    catalogue_path = DATA_DIR / "catalogue.json"
    if catalogue_path.exists():
        entries = json.loads(catalogue_path.read_text(encoding="utf-8"))
        docs = []
        for e in entries:
            doc_id = e["doc_id"]
            if doc_id == "home":
                path = DOCS_DIR / "wiki_home.md"
            else:
                path = DOCS_DIR / (doc_id + ".md")
            docs.append(DocMeta(
                doc_id=doc_id,
                title=e["title"],
                category=e["category"],
                path=path,
                summary=e.get("summary", ""),
                related=e.get("related", []),
            ))
        return docs

    # Fallback: parse DOCS_DIR at runtime
    if not DOCS_DIR.exists():
        return []
    docs = []
    for md_path in sorted(DOCS_DIR.rglob("*.md")):
        content = md_path.read_text(encoding="utf-8")
        doc_id = _doc_id_from_path(md_path)
        category = _category_from_doc_id(doc_id)
        docs.append(DocMeta(
            doc_id=doc_id,
            title=_extract_title(content) or doc_id,
            category=category,
            path=md_path,
            summary=_extract_summary(content),
            related=_extract_related(content),
        ))
    return docs


def load_doc_content(doc_id: str) -> str | None:
    """Return raw markdown content for a doc_id, or None if not found."""
    if doc_id == "home":
        candidate = DOCS_DIR / "wiki_home.md"
    else:
        candidate = DOCS_DIR / (doc_id + ".md")
    if candidate.exists():
        return candidate.read_text(encoding="utf-8")
    # Slug search: look for a filename matching the last segment
    slug = doc_id.split("/")[-1]
    for match in DOCS_DIR.rglob(f"{slug}.md"):
        return match.read_text(encoding="utf-8")
    return None


def chunks_for_doc(doc_id: str, content: str) -> list[Chunk]:
    return _split_into_chunks(doc_id, content)


def code_chunks_for_doc(doc_id: str, content: str) -> list[Chunk]:
    return _extract_code_chunks(doc_id, content)


def get_metadata_catalogue() -> dict[str, dict]:
    """Return metadata catalogue as a plain dict (for JSON serialisation)."""
    catalogue_path = DATA_DIR / "metadata.json"
    if catalogue_path.exists():
        return json.loads(catalogue_path.read_text(encoding="utf-8"))
    return {}
