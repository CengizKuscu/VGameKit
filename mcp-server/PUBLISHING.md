# MCP Server Güncelleme & Yayımlama

## Docs değiştiğinde yapılacaklar

### 1. Index'i yenile

```bash
cd mcp-server
uv run python -m vgamekit_mcp.build_index
```

Çıktı:
```
[1/4] Copying docs snapshot...
[2/4] Collecting and chunking documents...
[3/4] Loading model and encoding chunks...
[4/4] Saving indexes...
Done.
```

### 2. Versiyonu güncelle

**İki dosyada** aynı anda güncelle:

`mcp-server/pyproject.toml` satır 7:
```toml
version = "0.1.1"
```

`mcp-server/src/vgamekit_mcp/__init__.py` satır 3:
```python
__version__ = "0.1.1"
```

Versiyon kuralı:
- `0.1.x` → sadece docs içeriği değişti
- `0.x.0` → yeni tool veya özellik eklendi
- `x.0.0` → mevcut tool davranışı değişti (breaking)

### 3. Değişiklikleri stage et

```bash
git add src/vgamekit_mcp/data/ pyproject.toml src/vgamekit_mcp/__init__.py
```

### 4. Paketi derle

```bash
uv build
```

### 5. PyPI'ye yayımla

```bash
uv publish --token pypi-TOKENIN
```

---

## Token nerede?

pypi.org → Account Settings → API tokens → `vgamekit-mcp-publish`

Token'ı ortam değişkeni olarak kaydetmek istersen (her seferinde yazmamak için):

```bash
# ~/.zshrc veya ~/.zprofile içine ekle
export UV_PUBLISH_TOKEN="pypi-TOKENIN"
```

Sonra sadece `uv publish` yeterli olur.

---

