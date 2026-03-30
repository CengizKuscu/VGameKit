"""
Developer CLI: rebuilds the vector index from docs/en/.

Usage (from mcp-server/):
    python -m vgamekit_mcp.build_index [--source <path/to/docs/en>]

The source defaults to ../../docs/en relative to this file.
Outputs:
    data/docs/         — snapshot copy of the source docs
    data/index.npz     — chunk vectors + chunk texts for semantic search
    data/metadata.json — per-chunk metadata (doc_id, title, category, heading)
    data/code_index.npz    — code-block-only vectors
    data/code_metadata.json
"""

from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path

import numpy as np

from vgamekit_mcp.docs import (
    DATA_DIR,
    DOCS_DIR,
    _doc_id_from_path,
    _category_from_doc_id,
    _extract_title,
    _extract_summary,
    _extract_related,
    chunks_for_doc,
    code_chunks_for_doc,
)

# Default source: repo's docs/en/ relative to this package (works from source tree)
# parents[0]=vgamekit_mcp/, [1]=src/, [2]=mcp-server/, [3]=repo root
_DEFAULT_SOURCE = Path(__file__).parents[3] / "docs" / "en"
_MODEL_NAME = "sentence-transformers/all-MiniLM-L6-v2"


def _copy_docs(source: Path) -> None:
    """Sync source docs into data/docs/."""
    if DOCS_DIR.exists():
        shutil.rmtree(DOCS_DIR)
    shutil.copytree(source, DOCS_DIR)
    print(f"  Copied {len(list(DOCS_DIR.rglob('*.md')))} markdown files → {DOCS_DIR}")


def _collect_chunks(
    docs_dir: Path,
) -> tuple[list[dict], list[str], list[dict], list[str]]:
    """
    Walk docs_dir and produce:
      - doc_chunks_meta: list of metadata dicts for semantic index
      - doc_texts: list of chunk texts (parallel to doc_chunks_meta)
      - code_chunks_meta: metadata for code index
      - code_texts: code chunk texts
    """
    doc_chunks_meta: list[dict] = []
    doc_texts: list[str] = []
    code_chunks_meta: list[dict] = []
    code_texts: list[str] = []

    for md_path in sorted(docs_dir.rglob("*.md")):
        content = md_path.read_text(encoding="utf-8")
        doc_id = _doc_id_from_path(md_path)
        category = _category_from_doc_id(doc_id)
        title = _extract_title(content) or doc_id

        # Semantic chunks (split on ## headings)
        for chunk in chunks_for_doc(doc_id, content):
            doc_chunks_meta.append({
                "chunk_id": chunk.chunk_id,
                "doc_id": doc_id,
                "title": title,
                "category": category,
                "heading": chunk.heading,
            })
            doc_texts.append(chunk.text)

        # Code-only chunks
        for chunk in code_chunks_for_doc(doc_id, content):
            code_chunks_meta.append({
                "chunk_id": chunk.chunk_id,
                "doc_id": doc_id,
                "title": title,
                "category": category,
                "heading": chunk.heading,
                "code_language": chunk.code_language,
            })
            code_texts.append(chunk.text)

    return doc_chunks_meta, doc_texts, code_chunks_meta, code_texts


def _encode(texts: list[str], model) -> np.ndarray:
    """Encode texts using the sentence-transformers model."""
    vectors = model.encode(
        texts,
        batch_size=32,
        show_progress_bar=True,
        convert_to_numpy=True,
        normalize_embeddings=True,  # unit vectors → cosine sim == dot product
    )
    return vectors.astype(np.float32)


def _save_index(
    vectors: np.ndarray,
    texts: list[str],
    meta: list[dict],
    npz_path: Path,
    json_path: Path,
) -> None:
    chunks_array = np.array(texts, dtype=object)
    np.savez_compressed(str(npz_path), vectors=vectors, chunks=chunks_array)
    json_path.write_text(json.dumps(meta, indent=2, ensure_ascii=False), encoding="utf-8")
    size_kb = npz_path.stat().st_size // 1024
    print(f"  Saved {len(texts)} vectors → {npz_path.name} ({size_kb} KB)")


def main() -> None:
    parser = argparse.ArgumentParser(description="Rebuild VGameKit MCP search index.")
    parser.add_argument(
        "--source",
        type=Path,
        default=_DEFAULT_SOURCE,
        help=f"Path to docs/en directory (default: {_DEFAULT_SOURCE})",
    )
    args = parser.parse_args()

    source: Path = args.source.resolve()
    if not source.exists():
        print(f"ERROR: Source directory not found: {source}", file=sys.stderr)
        sys.exit(1)

    print(f"Source: {source}")
    print(f"Output: {DATA_DIR}")
    DATA_DIR.mkdir(parents=True, exist_ok=True)

    # 1. Copy docs snapshot
    print("\n[1/4] Copying docs snapshot...")
    _copy_docs(source)

    # 2. Collect chunks
    print("\n[2/4] Collecting and chunking documents...")
    doc_meta, doc_texts, code_meta, code_texts = _collect_chunks(DOCS_DIR)
    print(f"  Doc chunks: {len(doc_texts)}")
    print(f"  Code chunks: {len(code_texts)}")

    # 3. Load model and encode
    print(f"\n[3/4] Loading model '{_MODEL_NAME}' and encoding chunks...")
    print("  (First run downloads ~90 MB to ~/.cache/huggingface/)")
    try:
        from sentence_transformers import SentenceTransformer
    except ImportError:
        print("ERROR: sentence-transformers not installed.", file=sys.stderr)
        print("Run: pip install sentence-transformers", file=sys.stderr)
        sys.exit(1)

    model = SentenceTransformer(_MODEL_NAME)

    if doc_texts:
        print("  Encoding doc chunks...")
        doc_vectors = _encode(doc_texts, model)
    else:
        doc_vectors = np.zeros((0, 384), dtype=np.float32)

    if code_texts:
        print("  Encoding code chunks...")
        code_vectors = _encode(code_texts, model)
    else:
        code_vectors = np.zeros((0, 384), dtype=np.float32)

    # 4. Save indexes
    print("\n[4/4] Saving indexes...")
    _save_index(
        doc_vectors, doc_texts, doc_meta,
        DATA_DIR / "index.npz",
        DATA_DIR / "metadata.json",
    )
    _save_index(
        code_vectors, code_texts, code_meta,
        DATA_DIR / "code_index.npz",
        DATA_DIR / "code_metadata.json",
    )

    # Also build a doc summary catalogue for list_docs (no model needed at runtime)
    catalogue: list[dict] = []
    for md_path in sorted(DOCS_DIR.rglob("*.md")):
        content = md_path.read_text(encoding="utf-8")
        doc_id = _doc_id_from_path(md_path)
        catalogue.append({
            "doc_id": doc_id,
            "title": _extract_title(content) or doc_id,
            "category": _category_from_doc_id(doc_id),
            "summary": _extract_summary(content),
            "related": _extract_related(content),
        })
    catalogue_path = DATA_DIR / "catalogue.json"
    catalogue_path.write_text(
        json.dumps(catalogue, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    print(f"  Saved catalogue → catalogue.json ({len(catalogue)} docs)")

    print("\nDone. Commit the data/ directory:")
    print("  git add src/vgamekit_mcp/data/")


if __name__ == "__main__":
    main()
