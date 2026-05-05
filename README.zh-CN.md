[English](README.md) | [简体中文](README.zh-CN.md)

# vFrame 资源工具套件

适用于 Unity 的编辑器资源工具集合，聚焦资源检查、批量修复、导入规则管理与资源迁移分析。

## 功能特性

- 基于 Odin Inspector 构建的编辑器工具窗口与右键菜单
- 自动替换 FBX 内置材质与 Unity 内置资源引用
- 优化 `AnimationClip` 曲线精度，支持移除缩放曲线
- 批量检查并清理资源与场景中的丢失引用
- 为指定资源重新生成 GUID，并同步更新引用文件
- 输出资源依赖、GUID、File ID 与序列化数据
- 通过 `Asset ID Mapper` 建立稳定的资源 ID 到路径映射
- 通过 `Asset Importer` 管理可扩展的导入规则与哈希缓存
- 通过 `Asset Migrator` 分析依赖并执行移动、替换、复制、删除操作

## 前置要求

- Unity 2022.3.62f3 已用于当前仓库验证；包清单 `Assets/vFrame.ResourceToolset/package.json` 声明最低 Unity 版本为 `2018.4`
- 仅支持 Unity Editor，程序集 `vFrame.ResourceToolset.Editor` 只在 `Editor` 平台启用
- 依赖付费插件 Odin Inspector；当前工程通过 `com.sirenix.odin.inspector` 提供
- 依赖 `com.unity.editorcoroutines`，用于导入与迁移界面的协程流程

## 安装

该仓库中的 UPM 包位于 `Assets/vFrame.ResourceToolset/package.json`，包名为 `com.vyronlee.vframe.resourcetoolset`。

将以下依赖添加到项目的 `Packages/manifest.json`：

```json
{
  "dependencies": {
    "com.vyronlee.vframe.resourcetoolset": "https://github.com/VyronLee/vFrame.ResourceToolset.git?path=/Assets/vFrame.ResourceToolset#1.0.37"
  }
}
```

也可以在 Unity 中通过 `Window > Package Manager > Add package from git URL...` 添加同一地址。

如果你使用的是本仓库当前分支而不是已打标签版本，可将 `#1.0.37` 替换为具体分支或提交。

## 快速开始

1. 先确认工程中已经导入 Odin Inspector。
2. 安装包后，打开 `Tools > vFrame > Resource Toolset > Settings`。
3. 在设置窗口中创建总配置资源。
4. 按需创建以下配置资源：
   - `AnimationOptimizationConfig`
   - `BuiltinAssetConfig`
   - `AssetImportConfig`
5. 点击 `Save()` 保存配置。
6. 回到 `Assets/vFrame/Resource Toolset/...` 或 `Tools/vFrame/Resource Toolset/...` 菜单执行具体工具。

## 工具列表

### 设置窗口

- 菜单：`Tools/vFrame/Resource Toolset/Settings`
- 窗口类：`ResourceToolsetSettingWindow`
- 配置聚合类：`ResourceToolsetSettingConfig`

`ResourceToolsetSettingConfig` 可统一持有并保存：

- `AnimationOptimizationConfig`
- `BuiltinAssetConfig`
- `AssetImportConfig`

关键配置项：

- `AnimationOptimizationConfig.Precision`：动画曲线小数精度，默认 `3`
- `BuiltinAssetConfig.BuiltinReplacementMaterialsDir`：内置材质替换目录
- `BuiltinAssetConfig.BuiltinReplacementTextureDir`：内置贴图替换目录
- `BuiltinAssetConfig.FBXInternalMaterialReplacement`：FBX 内置材质替代材质
- `BuiltinAssetConfig.AutoReplaceFBXInternalMaterialOnImport`：是否在导入时自动替换
- `AssetImportConfig.AssetHashCacheDirectory`：资源哈希缓存目录
- `AssetImportConfig.RuleHashCacheFile`：规则哈希缓存文件

### FBX 内置材质处理

- 菜单：
  - `Assets/vFrame/Resource Toolset/FBX/Replace FBX Internal Materials`
  - `Assets/vFrame/Resource Toolset/FBX/Remove FBX External Objects`
- 主要类：`FBXInternalMaterialReplacementUtils`
- 自动导入处理器：`ReassignFBXInternalMaterial`

可直接调用的 API：

```csharp
FBXInternalMaterialReplacementUtils.ReplaceMaterial(path, newMaterial);
FBXInternalMaterialReplacementUtils.RemoveExternalObject(path);
```

当 `BuiltinAssetConfig.AutoReplaceFBXInternalMaterialOnImport` 为 `true` 时，`ReassignFBXInternalMaterial` 会在模型导入阶段自动把材质替换为 `BuiltinAssetConfig.FBXInternalMaterialReplacement`。

### 动画曲线优化

- 菜单：
  - `Assets/vFrame/Resource Toolset/Animation/Modify Animation Curve Precision`
  - `Assets/vFrame/Resource Toolset/Animation/Remove Animation Scale Curve`
- 主要类：`AnimationOptimizationUtils`

可直接调用的 API：

```csharp
AnimationOptimizationUtils.ModifyCurveValuesPrecision(clip, precision);
AnimationOptimizationUtils.RemoveScaleCurve(clip);
```

### Unity 内置资源替换

- 菜单：`Assets/vFrame/Resource Toolset/Replace Builtin Assets`
- 主要类：`BuiltinAssetsReplacementUtils`

可直接调用的 API：

```csharp
BuiltinAssetsReplacementUtils.ReplaceBuiltinAssets(obj);
```

该工具会处理 prefab、scene、material 中的内置资源引用，并尝试替换 `Resources/unity_builtin_extra` 下的材质、贴图与 UI Sprite。

### GUID 重新生成

- 菜单：`Assets/vFrame/Resource Toolset/Regenerate Asset GUIDs`
- 主要类：`GuidRegenerationUtils`

可直接调用的 API：

```csharp
GuidRegenerationUtils.RegenerateGuidsInDirectory(targetDirectory, referencesDirectory);
GuidRegenerationUtils.RegenerateGuidsOfFiles(targetFilePaths, referencesDirectory);
GuidRegenerationUtils.RegenerateGuidsOfFiles(targetFilePaths, referenceFilePaths);
GuidRegenerationUtils.RegenerateGuidsOfObjects(targetObjects, referencesObjects);
```

### 丢失引用检查与清理

- 菜单：
  - `Assets/vFrame/Resource Toolset/Missing Reference/Find Missing Reference`
  - `Assets/vFrame/Resource Toolset/Missing Reference/Remove Missing Reference`
- 主要类：`MissingReferenceValidationUtils`

可直接调用的 API：

```csharp
MissingReferenceValidationUtils.ValidateAsset(obj, out var missing);
MissingReferenceValidationUtils.ValidateActiveScene(out var missing);
MissingReferenceValidationUtils.RemoveMissingReference(obj, out var missing);
```

### 资源信息打印

- 菜单：
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Dependencies`
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Guid`
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Guid And File Id`
  - `Assets/vFrame/Resource Toolset/Print/Print Asset Serialized Data`
- 主要类：`InformationDumpUtils`

可直接调用的 API：

```csharp
InformationDumpUtils.PrintAssetDependencies(path);
InformationDumpUtils.PrintAssetGuid(path);
InformationDumpUtils.PrintAssetGuidAndFileId(obj);
InformationDumpUtils.PrintAssetSerializedData(path);
InformationDumpUtils.PrintAssetSerializedData(obj);
```

### Asset ID Mapper

- 菜单：`Tools/vFrame/Resource Toolset/Asset ID Mapper`
- 窗口类：`AssetIdsMapperWindow`
- 导出工具：`AssetIdsMapperUtils.Export(...)`

窗口支持创建多种资源分组，包括 `Texture`、`Sprite`、`Material`、`GameObject`、`AudioClip`、`AnimationClip`、`Shader` 与默认 `Object` 分组。完成映射后可执行 `Save & Export` 导出 JSON 映射数据。

### Asset Importer

- 菜单：`Tools/vFrame/Resource Toolset/Asset Importer`
- 右键快捷入口：`Assets/vFrame/Resource Toolset/Reimport`
- 窗口类：`AssetImporterWindow`
- 扩展基类：`AssetImporterRuleBase<T>`

自定义导入规则时，继承 `AssetImporterRuleBase<T>` 并实现 `ProcessImport(T assetImporter)`：

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

工具会根据 `AssetImportConfig` 中的缓存路径记录资源哈希与规则哈希，以减少重复导入。

### Asset Migrator

- 菜单：`Tools/vFrame/Resource Toolset/Asset Migrator`
- 窗口类：`AssetMigratorWindow`
- 底层工具：`AssetMigrationUtils`

`AssetMigratorWindow` 支持：

- 拖入资源或文件夹作为处理目标
- 执行 `Filter Process Targets()` 过滤真实处理对象
- 执行 `RefreshDependencies()` 收集依赖
- 对依赖项执行移动、复制、替换、删除

底层也提供可直接调用的替换 API：

```csharp
AssetMigrationUtils.ReplaceAsset(targetToProcess, assetToReplace, assetReplaceWith);
AssetMigrationUtils.ReplaceAsset(targetsToProcess, assetToReplace, assetReplaceWith);
AssetMigrationUtils.ReplaceAsset(targetsToProcess, assetPathToReplace, assetPathReplaceWith);
```

## 使用场景

### 场景 1：导入 FBX 时自动替换默认材质

1. 打开 `Tools/vFrame/Resource Toolset/Settings`
2. 创建或编辑 `BuiltinAssetConfig`
3. 设置 `FBXInternalMaterialReplacement`
4. 勾选 `AutoReplaceFBXInternalMaterialOnImport`
5. 重新导入 FBX，`ReassignFBXInternalMaterial` 会自动生效

### 场景 2：批量整理动画资源

1. 选中一个或多个 `.anim` 资源
2. 执行 `Assets/vFrame/Resource Toolset/Animation/Modify Animation Curve Precision`
3. 如需进一步减小曲线数量，再执行 `Remove Animation Scale Curve`

### 场景 3：建立资源导入规则

1. 打开 `Tools/vFrame/Resource Toolset/Asset Importer`
2. 点击 `Create Importer Rule`
3. 选择自定义规则类型
4. 配置过滤条件与规则参数
5. 使用 `Import All` 或 `Force Import All` 应用到匹配资源
6. 点击 `Save` 保存规则与缓存

### 场景 4：分析依赖并替换资源引用

1. 打开 `Tools/vFrame/Resource Toolset/Asset Migrator`
2. 拖入目标资源
3. 点击 `Filter Process Targets()`
4. 点击 `RefreshDependencies()`
5. 对依赖列表中的资源执行 `Replace` 或批量替换

## 注意事项

- 本包没有 Runtime 程序集，所有功能都运行在 Unity 编辑器内
- 多数功能会直接修改资源文件、场景文件或 `.meta` 文件，执行前请先备份或提交版本控制
- GUID 重建是高风险操作，尤其在跨目录或跨包引用较多时更要谨慎
- `Replace Builtin Assets` 依赖 `BuiltinAssetConfig` 中的替换目录配置；缺少替代资源时会在控制台报错
- `Asset Importer` 的扩展规则示例位于 `Assets/Sample~/AssetImport/`
- `Asset Migrator` 删除操作会弹出确认框，但不会替你恢复误删资源

## 许可证

本项目基于 [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) 许可协议发布。
