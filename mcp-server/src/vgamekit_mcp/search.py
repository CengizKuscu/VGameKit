"""Semantic search over VGameKit documentation using numpy cosine similarity."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import numpy as np

from vgamekit_mcp.docs import DATA_DIR

# ──────────────────────────────────────────────
# Index state (loaded once at first search call)
# ──────────────────────────────────────────────

_doc_vectors: np.ndarray | None = None
_doc_chunks: np.ndarray | None = None      # object array of chunk texts
_doc_meta: list[dict] | None = None

_code_vectors: np.ndarray | None = None
_code_chunks: np.ndarray | None = None
_code_meta: list[dict] | None = None

# Sentence-transformers model (loaded lazily)
_model: Any = None
_MODEL_NAME = "sentence-transformers/all-MiniLM-L6-v2"


def _load_index() -> None:
    global _doc_vectors, _doc_chunks, _doc_meta
    global _code_vectors, _code_chunks, _code_meta

    npz_path = DATA_DIR / "index.npz"
    meta_path = DATA_DIR / "metadata.json"
    code_npz_path = DATA_DIR / "code_index.npz"
    code_meta_path = DATA_DIR / "code_metadata.json"

    if not npz_path.exists():
        raise FileNotFoundError(
            f"Search index not found at {npz_path}. "
            "Run: python -m vgamekit_mcp.build_index"
        )

    data = np.load(str(npz_path), allow_pickle=True)
    _doc_vectors = data["vectors"].astype(np.float32)
    _doc_chunks = data["chunks"]
    _doc_meta = json.loads(meta_path.read_text(encoding="utf-8"))

    if code_npz_path.exists():
        cdata = np.load(str(code_npz_path), allow_pickle=True)
        _code_vectors = cdata["vectors"].astype(np.float32)
        _code_chunks = cdata["chunks"]
        _code_meta = json.loads(code_meta_path.read_text(encoding="utf-8"))
    else:
        _code_vectors = np.zeros((0, 384), dtype=np.float32)
        _code_chunks = np.array([], dtype=object)
        _code_meta = []


def _ensure_index() -> None:
    if _doc_vectors is None:
        _load_index()


def _get_model():
    global _model
    if _model is None:
        try:
            from sentence_transformers import SentenceTransformer
        except ImportError as e:
            raise ImportError(
                "sentence-transformers is required for semantic search. "
                "Install it: pip install sentence-transformers"
            ) from e
        _model = SentenceTransformer(_MODEL_NAME)
    return _model


def _safe_truncate(text: str, max_chars: int) -> str:
    """Truncate text without breaking open code fences."""
    if len(text) <= max_chars:
        return text
    truncated = text[:max_chars].rsplit(" ", 1)[0]
    # Count open code fences — if odd number, close the last one
    fence_count = truncated.count("```")
    if fence_count % 2 != 0:
        # Find the opening fence language tag to close properly
        last_fence = truncated.rfind("```")
        lang_end = truncated.find("\n", last_fence)
        truncated += "\n```"
    return truncated + "…"


def _embed_query(query: str) -> np.ndarray:
    model = _get_model()
    vec = model.encode([query], normalize_embeddings=True, convert_to_numpy=True)
    return vec[0].astype(np.float32)


def _cosine_top_k(
    query_vec: np.ndarray,
    vectors: np.ndarray,
    top_k: int,
    category_filter: str | None,
    meta: list[dict],
) -> list[tuple[float, int]]:
    """Return (score, index) pairs for the top_k most similar chunks."""
    if vectors.shape[0] == 0:
        return []

    # Dot product == cosine similarity because vectors are already normalised
    scores = vectors @ query_vec  # shape (N,)

    if category_filter:
        mask = np.array(
            [m.get("category") == category_filter for m in meta], dtype=bool
        )
        scores = np.where(mask, scores, -1.0)

    # argsort descending, take top_k
    indices = np.argpartition(scores, -min(top_k, len(scores)))[-top_k:]
    indices = indices[np.argsort(scores[indices])[::-1]]

    return [(float(scores[i]), int(i)) for i in indices if scores[i] > -0.5]


# ──────────────────────────────────────────────
# Public API
# ──────────────────────────────────────────────

def search_docs(
    query: str,
    top_k: int = 5,
    category: str | None = None,
) -> list[dict]:
    """
    Semantic search over documentation chunks.
    Returns a list of result dicts with score, doc_id, title, excerpt, etc.
    """
    _ensure_index()
    query_vec = _embed_query(query)
    hits = _cosine_top_k(query_vec, _doc_vectors, top_k, category, _doc_meta)

    results = []
    seen_doc_ids: set[str] = set()
    for score, idx in hits:
        m = _doc_meta[idx]
        chunk_text: str = str(_doc_chunks[idx])
        excerpt = _safe_truncate(chunk_text.strip(), 600)
        results.append({
            "score": round(score, 4),
            "doc_id": m["doc_id"],
            "title": m["title"],
            "category": m["category"],
            "heading": m.get("heading", ""),
            "excerpt": excerpt,
            "uri": f"vgamekit://docs/{m['doc_id']}",
        })
        seen_doc_ids.add(m["doc_id"])
    return results


def search_code(
    task: str,
    top_k: int = 3,
) -> list[dict]:
    """
    Semantic search over code-block chunks.
    Returns results with the code snippet and surrounding context.
    """
    _ensure_index()
    query_vec = _embed_query(task)
    hits = _cosine_top_k(query_vec, _code_vectors, top_k, None, _code_meta)

    results = []
    for score, idx in hits:
        m = _code_meta[idx]
        chunk_text: str = str(_code_chunks[idx])
        results.append({
            "score": round(score, 4),
            "doc_id": m["doc_id"],
            "title": m["title"],
            "language": m.get("code_language", "csharp"),
            "context": m.get("heading", ""),
            "snippet": chunk_text.strip(),
            "uri": f"vgamekit://docs/{m['doc_id']}",
        })
    return results
