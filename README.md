[English](README.md) | [简体中文](README.zh-CN.md)

# vFrame Resource Toolset

A Unity editor-only toolset for asset inspection, batch fixing, import rule management, and dependency-aware asset migration.

## Features

- Odin Inspector-based editor windows and asset context menu tools
- Automatic FBX internal material replacement and Unity built-in asset replacement
- `AnimationClip` curve precision optimization and scale-curve removal
- Batch validation and cleanup of missing references in assets and scenes
- GUID regeneration for selected assets with reference file updates
- Asset dependency, GUID, File ID, and serialized data dumping utilities
- `Asset ID Mapper` for stable ID-to-path asset mapping
- `Asset Importer` for extensible import rules with hash-based caching
- `Asset Migrator` for dependency analysis and move/replace/duplicate/delete workflows

## Prerequisites

- The current repository is validated with Unity `2022.3.62f3`; the embedded package manifest at `Assets/vFrame.ResourceToolset/package.json` declares a minimum Unity version of `2018.4`
- Editor only: the `vFrame.ResourceToolset.Editor` assembly is limited to the `Editor` platform
- Requires the paid Odin Inspector plugin; this project currently provides it via `com.sirenix.odin.inspector`
- Requires `com.unity.editorcoroutines` for coroutine-driven import and migration workflows

## Installation

This repository contains an embedded UPM package at `Assets/vFrame.ResourceToolset/package.json` with package name `com.vyronlee.vframe.resourcetoolset`.

Add the dependency to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vyronlee.vframe.resourcetoolset": "https://github.com/VyronLee/vFrame.ResourceToolset.git?path=/Assets/vFrame.ResourceToolset#1.0.37"
  }
}
```

You can also add the same URL through `Window > Package Manager > Add package from git URL...`.

If you want to track a branch or a commit instead of a tagged version, replace `#1.0.37` with the desired ref.

## Quick Start

1. Make sure Odin Inspector is already installed in your Unity project.
2. Install the package.
3. Open `Tools > vFrame > Resource Toolset > Settings`.
4. Create the main settings asset in the settings window.
5. Create the config assets you need:
   - `AnimationOptimizationConfig`
   - `BuiltinAssetConfig`
   - `AssetImportConfig`
6. Click `Save()`.
7. Use the tools from `Assets/vFrame/Resource Toolset/...` or `Tools/vFrame/Resource Toolset/...`.

## Tools

### Settings Window

- Menu: `Tools/vFrame/Resource Toolset/Settings`
- Window class: `ResourceToolsetSettingWindow`
- Root config class: `ResourceToolsetSettingConfig`

`ResourceToolsetSettingConfig` can create and persist:

- `AnimationOptimizationConfig`
- `BuiltinAssetConfig`
- `AssetImportConfig`

Key settings include:

- `AnimationOptimizationConfig.Precision`: decimal precision for animation curve values, default `3`
- `BuiltinAssetConfig.BuiltinReplacementMaterialsDir`: directory for replacement built-in materials
- `BuiltinAssetConfig.BuiltinReplacementTextureDir`: directory for replacement built-in textures
- `BuiltinAssetConfig.FBXInternalMaterialReplacement`: replacement material for FBX internal materials
- `BuiltinAssetConfig.AutoReplaceFBXInternalMaterialOnImport`: auto-replace toggle during model import
- `AssetImportConfig.AssetHashCacheDirectory`: asset hash cache directory
- `AssetImportConfig.RuleHashCacheFile`: rule hash cache asset path

### FBX Internal Material Tools

- Menus:
  - `Assets/vFrame/Resource Toolset/FBX/Replace FBX Internal Materials`
  - `Assets/vFrame/Resource Toolset/FBX/Remove FBX External Objects`
- Utility class: `FBXInternalMaterialReplacementUtils`
- Import hook: `ReassignFBXInternalMaterial`

Direct API:

```csharp
FBXInternalMaterialReplacementUtils.ReplaceMaterial(path, newMaterial);
FBXInternalMaterialReplacementUtils.RemoveExternalObject(path);
```

When `BuiltinAssetConfig.AutoReplaceFBXInternalMaterialOnImport` is enabled, `ReassignFBXInternalMaterial` automatically replaces imported model materials with `BuiltinAssetConfig.FBXInternalMaterialReplacement`.

### Animation Optimization

- Menus:
  - `Assets/vFrame/Resource Toolset/Animation/Modify Animation Curve Precision`
  - `Assets/vFrame/Resource Toolset/Animation/Remove Animation Scale Curve`
- Utility class: `AnimationOptimizationUtils`

Direct API:

```csharp
AnimationOptimizationUtils.ModifyCurveValuesPrecision(clip, precision);
AnimationOptimizationUtils.RemoveScaleCurve(clip);
```

### Built-in Asset Replacement

- Menu: `Assets/vFrame/Resource Toolset/Replace Builtin Assets`
- Utility class: `BuiltinAssetsReplacementUtils`

Direct API:

```csharp
BuiltinAssetsReplacementUtils.ReplaceBuiltinAssets(obj);
```

This replaces built-in references inside prefabs, scenes, and materials, including assets from `Resources/unity_builtin_extra` such as materials, textures, and UI sprites.

### GUID Regeneration

- Menu: `Assets/vFrame/Resource Toolset/Regenerate Asset GUIDs`
- Utility class: `GuidRegenerationUtils`

Direct API:

```csharp
GuidRegenerationUtils.RegenerateGuidsInDirectory(targetDirectory, referencesDirectory);
GuidRegenerationUtils.RegenerateGuidsOfFiles(targetFilePaths, referencesDirectory);
GuidRegenerationUtils.RegenerateGuidsOfFiles(targetFilePaths, referenceFilePaths);
GuidRegenerationUtils.RegenerateGuidsOfObjects(targetObjects, referencesObjects);
```

### Missing Reference Validation

- Menus:
  - `Assets/vFrame/Resource Toolset/Missing Reference/Find Missing Reference`
  - `Assets/vFrame/Resource Toolset/Missing Reference/Remove Missing Reference`
- Utility class: `MissingReferenceValidationUtils`

Direct API:

```csharp
MissingReferenceValidationUtils.ValidateAsset(obj, out var missing);
MissingReferenceValidationUtils.ValidateActiveScene(out var missing);
MissingReferenceValidationUtils.RemoveMissingReference(obj, out var missing);
```

### Information Dumping

- Menus:
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Dependencies`
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Guid`
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Guid And File Id`
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Serialized Data`
- Utility class: `InformationDumpUtils`

Direct API:

```csharp
InformationDumpUtils.PrintAssetDependencies(path);
InformationDumpUtils.PrintAssetGuid(path);
InformationDumpUtils.PrintAssetGuidAndFileId(obj);
InformationDumpUtils.PrintAssetSerializedData(path);
InformationDumpUtils.PrintAssetSerializedData(obj);
```

### Asset ID Mapper

- Menu: `Tools/vFrame/Resource Toolset/Asset ID Mapper`
- Window class: `AssetIdsMapperWindow`
- Export helper: `AssetIdsMapperUtils.Export(...)`

The window can create mapping groups for `Texture`, `Sprite`, `Material`, `GameObject`, `AudioClip`, `AnimationClip`, `Shader`, and generic `Object` assets. After editing mappings, use `Save & Export` to write the JSON mapping output.

### Asset Importer

- Menu: `Tools/vFrame/Resource Toolset/Asset Importer`
- Asset context shortcut: `Assets/vFrame/Resource Toolset/Reimport`
- Window class: `AssetImporterWindow`
- Extension base class: `AssetImporterRuleBase<T>`

To create a custom import rule, inherit from `AssetImporterRuleBase<T>` and implement `ProcessImport(T assetImporter)`:

```csharp
using UnityEditor;
using vFrame.ResourceToolset.Editor.Windows.Importer;

public class CustomTextureImporterRule : AssetImporterRuleBase<TextureImporter>
{
    protected override bool ProcessImport(TextureImporter assetImporter)
    {
        return false;
    }
}
```

The importer stores asset hashes and rule hashes based on `AssetImportConfig` so unchanged assets can be skipped on later runs.

### Asset Migrator

- Menu: `Tools/vFrame/Resource Toolset/Asset Migrator`
- Window class: `AssetMigratorWindow`
- Core utility class: `AssetMigrationUtils`

`AssetMigratorWindow` supports:

- dropping assets or folders as process targets
- running `FilterProcessTargets()` to resolve real process objects
- running `RefreshDependencies()` to collect dependencies
- moving, duplicating, replacing, or deleting dependency assets

Direct replacement APIs are also available:

```csharp
AssetMigrationUtils.ReplaceAsset(targetToProcess, assetToReplace, assetReplaceWith);
AssetMigrationUtils.ReplaceAsset(targetsToProcess, assetToReplace, assetReplaceWith);
AssetMigrationUtils.ReplaceAsset(targetsToProcess, assetPathToReplace, assetPathReplaceWith);
```

## Usage

### Scenario 1: Automatically replace FBX default materials on import

1. Open `Tools/vFrame/Resource Toolset/Settings`
2. Create or edit `BuiltinAssetConfig`
3. Assign `FBXInternalMaterialReplacement`
4. Enable `AutoReplaceFBXInternalMaterialOnImport`
5. Reimport an FBX asset and let `ReassignFBXInternalMaterial` handle the replacement

### Scenario 2: Batch-optimize animation clips

1. Select one or more `.anim` assets
2. Run `Assets/vFrame/Resource Toolset/Animation/Modify Animation Curve Precision`
3. Optionally run `Remove Animation Scale Curve` to remove scale curves entirely

### Scenario 3: Build reusable asset import rules

1. Open `Tools/vFrame/Resource Toolset/Asset Importer`
2. Click `Create Importer Rule`
3. Choose your custom rule type
4. Configure filters and rule fields
5. Run `Import All` or `Force Import All`
6. Click `Save` to persist rules and caches

### Scenario 4: Analyze and replace dependency assets

1. Open `Tools/vFrame/Resource Toolset/Asset Migrator`
2. Drag target assets into the window
3. Run `FilterProcessTargets()`
4. Run `RefreshDependencies()`
5. Use `Replace` or the batch actions on the dependency list

## Notes

- There is no runtime assembly; the package is editor-only
- Many tools modify assets, scenes, or `.meta` files directly, so version control or backups are strongly recommended
- GUID regeneration is inherently risky, especially in projects with wide cross-folder references
- `Replace Builtin Assets` depends on correctly configured replacement directories in `BuiltinAssetConfig`; missing replacement assets produce console errors
- Example importer rule scripts are included under `Assets/Sample~/AssetImport/`
- Delete operations in `Asset Migrator` ask for confirmation, but recovery is still your responsibility

## License

This project is licensed under This project is licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).
