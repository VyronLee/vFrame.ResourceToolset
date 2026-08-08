# vFrame.ResourceToolset

Editor-only Unity package for asset inspection, batch processing, import rules, and dependency-aware migration.

**Cross-package conventions** are defined in workspace `D:/Workspace/vFrame/.claude/rules/*.md`.

## Purpose

Provides Odin-based editor windows and asset menu tools for:
- Automatic FBX internal material replacement
- AnimationClip curve precision optimization
- Built-in asset replacement (materials, textures, UI sprites)
- Missing reference validation and cleanup
- GUID regeneration with reference updates
- Asset dependency / GUID / FileID dumping utilities
- Asset ID Mapper for stable ID-to-path mapping
- Asset Importer for extensible import rules with hash caching
- Asset Migrator for dependency-aware move/replace/duplicate/delete

## Assemblies & Dependencies

| Assembly | Platform | References |
|----------|----------|------------|
| `vFrame.ResourceToolset.Editor` | Editor only | `Unity.EditorCoroutines.Editor` |

**Critical: Requires Odin Inspector (paid plugin).**
- Odin is NOT declared in package.json or asmdef — you must install it separately
- Used extensively in Windows/ (`AssetMigratorWindow`, `AssetImporterWindow`, `AssetIdsMapperWindow`) and Odin/ attribute drawers
- Package provides Odin via `com.sirenix.odin.inspector` in this workspace

## Tool Catalog

**Tools menu** (`Tools/vFrame/Resource Toolset/`):
- `Settings` — Main config window (`ResourceToolsetSettingWindow`) creates `AnimationOptimizationConfig`, `BuiltinAssetConfig`, `AssetImportConfig`
- `Asset ID Mapper` — Export stable ID-to-path mappings for Texture, Sprite, Material, GameObject, AudioClip, AnimationClip, Shader
- `Asset Importer` — Extensible import rules with hash-based caching; inherit from `AssetImporterRuleBase<T>`
- `Asset Migrator` — Drop assets, resolve dependencies, move/replace/duplicate/delete with reference updates

**Asset context menu** (`Assets/vFrame/Resource Toolset/`):
- `Reimport` — Reimport via AssetImporter rules
- `Animation/` — Modify curve precision, Remove scale curves
- `FBX/` — Replace internal materials, Remove external objects
- `Replace Builtin Assets` — Replace Unity built-in references (materials, textures, UI sprites)
- `Regenerate Asset GUIDs` — GUID regen with reference updates
- `Print/` — Asset dependencies, GUID, GUID+FileID, serialized data
- `Missing Reference/` — Find, Remove missing refs

## Build & Test

**No test assemblies exist**. Validation is manual:
1. Open Unity Editor (`2022.3.62f3` validated; minimum `2018.4`)
2. Ensure Odin Inspector is installed
3. Test each tool from its menu path
4. For import hooks, reimport an FBX and verify `ReassignFBXInternalMaterial` runs

## Gotchas

- **Odin NOT in dependencies** — Must install `com.sirenix.odin.inspector` manually; tools fail without it
- **GUID regeneration is destructive** — Updates `.meta` files and all referencing assets; version control strongly recommended
- **Default config paths** — Settings creates configs at `Assets/Editor/ResourceToolset/`
- **Hash caching** — AssetImporter uses `AssetImportConfig.AssetHashCacheDirectory` for incremental processing
- **FBX auto-replacement** — Requires `BuiltinAssetConfig.FBXInternalMaterialReplacement` assigned and `AutoReplaceFBXInternalMaterialOnImport` enabled
- **Delete operations ask for confirmation** — Asset Migrator deletes are your responsibility to back up
