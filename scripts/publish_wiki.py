#!/usr/bin/env python3
"""
publish_wiki.py — VGameKit docs/ → GitHub Wiki publisher

Usage:
    python3 scripts/publish_wiki.py [--dry-run]

What it does:
    1. Reads every .md file listed in FILE_MAP under docs/en/ and docs/tr/.
    2. Renames each file to a flat wiki-safe name (e.g. en-Reference-App-Manager).
    3. Rewrites all relative markdown links inside each file so they point to the
       new flat wiki names instead of directory-relative paths.
    4. Generates _Sidebar.md (navigation panel, both languages).
    5. Clones the wiki git repo into a temp directory.
    6. Copies the generated files in, commits, and pushes.

Source of truth for each wiki page:
    en/wiki_home.md  → Home.md        (GitHub Wiki default landing page)
    tr/wiki_home.md  → tr-Anasayfa.md (Turkish landing page)
    All other docs/  → flat wiki page names per FILE_MAP

Requirements:
    - Python 3.9+
    - git available on PATH
    - Write access to https://github.com/CengizKuscu/VGameKit.wiki.git

    Before first run:
        1. Enable Wikis in repo Settings → Features → Wikis.
        2. Create at least one page manually via the GitHub UI so the wiki repo
           exists (GitHub does not create the repo until the first page is saved).
"""

from __future__ import annotations

import argparse
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

REPO_ROOT = Path(__file__).resolve().parent.parent
DOCS_ROOT = REPO_ROOT / "docs"
WIKI_REMOTE = "https://github.com/CengizKuscu/VGameKit.wiki.git"

# Maps every source file (relative to DOCS_ROOT) → flat wiki page name (no .md)
#
# Rule: <lang>-<Section>-<Slug>
#   lang    : "en" | "tr"
#   Section : Reference | HowTo | Tutorial | Explanation  (or blank for home)
#   Slug    : PascalCase words joined with hyphens
#
# Special:
#   en/wiki_home.md  → Home          (GitHub Wiki main page)
#   tr/wiki_home.md  → tr-Anasayfa

FILE_MAP: dict[str, str] = {
    # ── Home pages ──────────────────────────────────────────────────────────
    "en/wiki_home.md": "Home",
    "tr/wiki_home.md": "tr-Anasayfa",
    # ── EN Reference ────────────────────────────────────────────────────────
    "en/reference/api/app-manager.md": "en-Reference-App-Manager",
    "en/reference/api/lifetime-scopes.md": "en-Reference-Lifetime-Scopes",
    "en/reference/api/subscribable.md": "en-Reference-Subscribable",
    "en/reference/api/menu-system.md": "en-Reference-Menu-System",
    "en/reference/api/popup-system.md": "en-Reference-Popup-System",
    "en/reference/api/spawner.md": "en-Reference-Spawner",
    "en/reference/api/processflows.md": "en-Reference-ProcessFlows",
    "en/reference/api/logging.md": "en-Reference-Logging",
    "en/reference/api/configuration.md": "en-Reference-Configuration",
    "en/reference/api/utilities.md": "en-Reference-Utilities",
    "en/reference/api/google-ads.md": "en-Reference-Google-Ads",
    "en/reference/api/game-analytics.md": "en-Reference-Game-Analytics",
    "en/reference/api/jsonkit.md": "en-Reference-JSONKit",
    # ── EN How-to ───────────────────────────────────────────────────────────
    "en/how-to/install-modules.md": "en-HowTo-Install-Modules",
    "en/how-to/create-menu.md": "en-HowTo-Create-Menu",
    "en/how-to/use-spawner.md": "en-HowTo-Use-Spawner",
    "en/how-to/manage-processflows.md": "en-HowTo-Manage-ProcessFlows",
    "en/how-to/configure-logging.md": "en-HowTo-Configure-Logging",
    "en/how-to/integrate-ads.md": "en-HowTo-Integrate-Ads",
    "en/how-to/integrate-analytics.md": "en-HowTo-Integrate-Analytics",
    # ── EN Tutorials ────────────────────────────────────────────────────────
    "en/tutorials/getting-started.md": "en-Tutorial-Getting-Started",
    "en/tutorials/first-game.md": "en-Tutorial-First-Game",
    "en/tutorials/ui-system.md": "en-Tutorial-UI-System",
    "en/tutorials/spawner-basics.md": "en-Tutorial-Spawner-Basics",
    "en/tutorials/processflows.md": "en-Tutorial-ProcessFlows",
    # ── EN Explanation ──────────────────────────────────────────────────────
    "en/explanation/vcontainer-di.md": "en-Explanation-VContainer-DI",
    "en/explanation/messagepipe-events.md": "en-Explanation-MessagePipe-Events",
    "en/explanation/spawner-pooling.md": "en-Explanation-Spawner-Pooling",
    "en/explanation/async-patterns.md": "en-Explanation-Async-Patterns",
    "en/explanation/project-structure.md": "en-Explanation-Project-Structure",
    # ── TR Reference ────────────────────────────────────────────────────────
    "tr/reference/api/app-manager.md": "tr-Reference-App-Manager",
    "tr/reference/api/lifetime-scopes.md": "tr-Reference-Lifetime-Scopes",
    "tr/reference/api/subscribable.md": "tr-Reference-Subscribable",
    "tr/reference/api/menu-system.md": "tr-Reference-Menu-System",
    "tr/reference/api/popup-system.md": "tr-Reference-Popup-System",
    "tr/reference/api/spawner.md": "tr-Reference-Spawner",
    "tr/reference/api/processflows.md": "tr-Reference-ProcessFlows",
    "tr/reference/api/logging.md": "tr-Reference-Logging",
    "tr/reference/api/configuration.md": "tr-Reference-Configuration",
    "tr/reference/api/utilities.md": "tr-Reference-Utilities",
    "tr/reference/api/google-ads.md": "tr-Reference-Google-Ads",
    "tr/reference/api/game-analytics.md": "tr-Reference-Game-Analytics",
    "tr/reference/api/jsonkit.md": "tr-Reference-JSONKit",
    # ── TR How-to ───────────────────────────────────────────────────────────
    "tr/how-to/install-modules.md": "tr-HowTo-Install-Modules",
    "tr/how-to/create-menu.md": "tr-HowTo-Create-Menu",
    "tr/how-to/use-spawner.md": "tr-HowTo-Use-Spawner",
    "tr/how-to/manage-processflows.md": "tr-HowTo-Manage-ProcessFlows",
    "tr/how-to/configure-logging.md": "tr-HowTo-Configure-Logging",
    "tr/how-to/integrate-ads.md": "tr-HowTo-Integrate-Ads",
    "tr/how-to/integrate-analytics.md": "tr-HowTo-Integrate-Analytics",
    # ── TR Tutorials ────────────────────────────────────────────────────────
    "tr/tutorials/getting-started.md": "tr-Tutorial-Getting-Started",
    "tr/tutorials/first-game.md": "tr-Tutorial-First-Game",
    "tr/tutorials/ui-system.md": "tr-Tutorial-UI-System",
    "tr/tutorials/spawner-basics.md": "tr-Tutorial-Spawner-Basics",
    "tr/tutorials/processflows.md": "tr-Tutorial-ProcessFlows",
    # ── TR Explanation ──────────────────────────────────────────────────────
    "tr/explanation/vcontainer-di.md": "tr-Explanation-VContainer-DI",
    "tr/explanation/messagepipe-events.md": "tr-Explanation-MessagePipe-Events",
    "tr/explanation/spawner-pooling.md": "tr-Explanation-Spawner-Pooling",
    "tr/explanation/async-patterns.md": "tr-Explanation-Async-Patterns",
    "tr/explanation/project-structure.md": "tr-Explanation-Project-Structure",
}

# Reverse map: DOCS_ROOT-relative path → wiki page name. Used for link rewriting.
_REVERSE: dict[str, str] = {k: v for k, v in FILE_MAP.items()}

# ---------------------------------------------------------------------------
# _Sidebar.md — generated (both languages, hardcoded labels)
# ---------------------------------------------------------------------------

SIDEBAR_MD = """\
## English

**[Home](Home)**

**Tutorials**
- [Getting Started](en-Tutorial-Getting-Started)
- [First Game](en-Tutorial-First-Game)
- [UI System](en-Tutorial-UI-System)
- [Spawner Basics](en-Tutorial-Spawner-Basics)
- [Process Flows](en-Tutorial-ProcessFlows)

**How-to Guides**
- [Install Modules](en-HowTo-Install-Modules)
- [Create a Menu](en-HowTo-Create-Menu)
- [Use the Spawner](en-HowTo-Use-Spawner)
- [Manage Process Flows](en-HowTo-Manage-ProcessFlows)
- [Configure Logging](en-HowTo-Configure-Logging)
- [Integrate Ads](en-HowTo-Integrate-Ads)
- [Integrate Analytics](en-HowTo-Integrate-Analytics)

**Reference**
- [App Manager](en-Reference-App-Manager)
- [Lifetime Scopes](en-Reference-Lifetime-Scopes)
- [Subscribable](en-Reference-Subscribable)
- [Menu System](en-Reference-Menu-System)
- [Popup System](en-Reference-Popup-System)
- [Spawner](en-Reference-Spawner)
- [Process Flows](en-Reference-ProcessFlows)
- [Logging](en-Reference-Logging)
- [Configuration](en-Reference-Configuration)
- [Utilities](en-Reference-Utilities)
- [Google Ads](en-Reference-Google-Ads)
- [Game Analytics](en-Reference-Game-Analytics)
- [JSONKit](en-Reference-JSONKit)

**Explanation**
- [VContainer & DI](en-Explanation-VContainer-DI)
- [MessagePipe Events](en-Explanation-MessagePipe-Events)
- [Spawner & Pooling](en-Explanation-Spawner-Pooling)
- [Async Patterns](en-Explanation-Async-Patterns)
- [Project Structure](en-Explanation-Project-Structure)

---

## Türkçe

**[Anasayfa](tr-Anasayfa)**

**Tutorials**
- [Başlarken](tr-Tutorial-Getting-Started)
- [İlk Oyun](tr-Tutorial-First-Game)
- [UI Sistemi](tr-Tutorial-UI-System)
- [Spawner Temelleri](tr-Tutorial-Spawner-Basics)
- [Process Flow'lar](tr-Tutorial-ProcessFlows)

**Nasıl Yapılır**
- [Modül Kurulumu](tr-HowTo-Install-Modules)
- [Menü Oluştur](tr-HowTo-Create-Menu)
- [Spawner Kullan](tr-HowTo-Use-Spawner)
- [Process Flow Yönet](tr-HowTo-Manage-ProcessFlows)
- [Logging Yapılandır](tr-HowTo-Configure-Logging)
- [Reklam Entegrasyonu](tr-HowTo-Integrate-Ads)
- [Analitik Entegrasyonu](tr-HowTo-Integrate-Analytics)

**Referans**
- [App Manager](tr-Reference-App-Manager)
- [Lifetime Scopes](tr-Reference-Lifetime-Scopes)
- [Subscribable](tr-Reference-Subscribable)
- [Menü Sistemi](tr-Reference-Menu-System)
- [Popup Sistemi](tr-Reference-Popup-System)
- [Spawner](tr-Reference-Spawner)
- [Process Flows](tr-Reference-ProcessFlows)
- [Loglama](tr-Reference-Logging)
- [Konfigürasyon](tr-Reference-Configuration)
- [Araçlar](tr-Reference-Utilities)
- [Google Ads](tr-Reference-Google-Ads)
- [Game Analytics](tr-Reference-Game-Analytics)
- [JSONKit](tr-Reference-JSONKit)

**Açıklama**
- [VContainer & DI](tr-Explanation-VContainer-DI)
- [MessagePipe Events](tr-Explanation-MessagePipe-Events)
- [Spawner & Pooling](tr-Explanation-Spawner-Pooling)
- [Async Patterns](tr-Explanation-Async-Patterns)
- [Proje Yapısı](tr-Explanation-Project-Structure)
"""

# ---------------------------------------------------------------------------
# Link rewriting
# ---------------------------------------------------------------------------

# Matches standard markdown links: [text](path)
_MD_LINK = re.compile(r"\[([^\]]*)\]\(([^)#\s]+)(#[^)]+)?\)")


def _resolve_link(source_rel: str, href: str) -> str | None:
    """
    Given the source file's relative path (from DOCS_ROOT) and a relative href
    found inside it, return the wiki page name, or None if the href is not a
    known docs page (e.g. external URL or anchor-only).
    """
    if href.startswith(("http://", "https://", "mailto:", "#")):
        return None

    source_dir = Path(source_rel).parent

    # Normalise to a DOCS_ROOT-relative string using forward slashes
    try:
        target_rel = str(
            (DOCS_ROOT / source_dir / href).resolve().relative_to(DOCS_ROOT)
        ).replace("\\", "/")
    except ValueError:
        return None  # outside docs root

    return _REVERSE.get(target_rel)


def rewrite_links(source_rel: str, content: str) -> str:
    """Replace all relative .md links in content with wiki page names."""

    def replacer(m: re.Match) -> str:
        text = m.group(1)
        href = m.group(2)
        anchor = m.group(3) or ""

        wiki_name = _resolve_link(source_rel, href)
        if wiki_name is None:
            return m.group(0)  # leave unchanged

        return f"[{text}]({wiki_name}{anchor})"

    return _MD_LINK.sub(replacer, content)


# ---------------------------------------------------------------------------
# Build: generate all wiki files into a directory
# ---------------------------------------------------------------------------


def build(output_dir: Path) -> None:
    output_dir.mkdir(parents=True, exist_ok=True)

    for rel, wiki_name in FILE_MAP.items():
        src = DOCS_ROOT / rel
        if not src.exists():
            print(f"  WARN  missing source: {src}", file=sys.stderr)
            continue

        content = src.read_text(encoding="utf-8")
        content = rewrite_links(rel, content)

        dest = output_dir / f"{wiki_name}.md"
        dest.write_text(content, encoding="utf-8")
        print(f"  write {dest.name}")

    (output_dir / "_Sidebar.md").write_text(SIDEBAR_MD, encoding="utf-8")
    print("  write _Sidebar.md (generated)")


# ---------------------------------------------------------------------------
# Publish: clone wiki repo, replace contents, commit, push
# ---------------------------------------------------------------------------


def run(cmd: list[str], cwd: Path | None = None) -> None:
    result = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True)
    if result.returncode != 0:
        print(result.stdout)
        print(result.stderr, file=sys.stderr)
        sys.exit(result.returncode)


def publish(built_dir: Path) -> None:
    with tempfile.TemporaryDirectory() as tmp:
        wiki_dir = Path(tmp) / "wiki"
        print(f"\nCloning {WIKI_REMOTE} ...")
        run(["git", "clone", WIKI_REMOTE, str(wiki_dir)])

        # Remove all existing .md files (keep .git)
        for f in wiki_dir.glob("*.md"):
            f.unlink()

        # Copy generated files in
        for f in built_dir.glob("*.md"):
            shutil.copy2(f, wiki_dir / f.name)

        # Commit and push
        run(["git", "add", "-A"], cwd=wiki_dir)

        status = subprocess.run(
            ["git", "status", "--porcelain"],
            cwd=wiki_dir,
            capture_output=True,
            text=True,
        )
        if not status.stdout.strip():
            print("Wiki is already up to date — nothing to push.")
            return

        run(["git", "commit", "-m", "docs: sync wiki from docs/"], cwd=wiki_dir)
        run(["git", "push"], cwd=wiki_dir)
        print("Wiki updated successfully.")


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def main() -> None:
    parser = argparse.ArgumentParser(description="Publish docs/ to GitHub Wiki")
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Build files into wiki-build/ but do not clone or push",
    )
    parser.add_argument(
        "--output-dir",
        default=str(REPO_ROOT / "wiki-build"),
        help="Directory to write generated files into (default: wiki-build/)",
    )
    args = parser.parse_args()

    output_dir = Path(args.output_dir)

    print(f"Building wiki pages into {output_dir} ...")
    build(output_dir)
    print(f"\nBuilt {len(list(output_dir.glob('*.md')))} pages.")

    if args.dry_run:
        print("\nDry-run mode — skipping clone/push. Inspect output in:", output_dir)
    else:
        publish(output_dir)


if __name__ == "__main__":
    main()
