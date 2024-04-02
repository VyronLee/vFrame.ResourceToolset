# vFrame 资源工具套件

![vFrame](https://img.shields.io/badge/vFrame-ResourceToolset-blue) [![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![License](https://img.shields.io/badge/License-Apache%202.0-brightgreen.svg)](#License)

随着项目开发进度的推进，工程中的资源文件会变得越来越多，管理将会变得越来越难。这时候往往少不了需要对大量的资源进行整理，找出其中多余的、无用的资源，亦或是需要排查出工程中哪些资源存在着参数设置不合理，亦或是需要批量进行资源的替换、修改操作等。本仓库工具套件为了解决上述问题应运而生。

[English Version (Power by ChatGPT)](./README_en.md)

## 目录

* [概览](#概览)
* [安装](#安装)
* [工具套件设置](#工具套件设置)
* [使用说明](#使用说明)
   + [FBX内置材质替换工具](#FBX内置材质替换工具)
   + [动画曲线精度优化以及曲线裁剪工具](#动画曲线精度优化以及曲线裁剪工具)
      - [曲线精度优化原理](#曲线精度优化原理)
      - [曲线裁剪原理](#曲线裁剪原理)
      - [使用方式](#使用方式)
   + [Unity 内置资源替换工具](#Unity-内置资源替换工具)
      - [使用方式](#使用方式)
   + [资源 GUID 重新生成工具](#资源-GUID-重新生成工具)
      - [使用方式](#使用方式)
   + [资源引用丢失查找工具](#资源引用丢失查找工具)
      - [使用方式](#使用方式)
   + [资源文件信息打印工具](#资源文件信息打印工具)
   + [资源ID映射工具](#资源ID映射工具)
      - [使用方式](#使用方式)
   + [资源导入设置工具](#资源导入设置工具)
      - [使用方式](#使用方式)
      - [规则扩展](#规则扩展)
   + [资源迁移工具](#资源迁移工具)
      - [使用方式](#使用方式)
* [License](#license)

## 概览

仓库中提供的工具主要包含有：
* FBX内置材质替换工具
* 动画曲线精度优化以及曲线裁剪工具
* Unity 内置资源替换工具
* 资源 GUID 重新生成工具
* 资源引用丢失查找工具
* 资源文件信息打印工具
* 资源ID映射工具
* 资源导入设置工具
* 资源迁移工具

## 安装

**注意**：本工具套件依赖于付费插件 [OdinInspector](https://odininspector.com/)，使用前需要自行下载并导入到工程中（人生苦短，我用Odin！）

推荐使用 Unity Package Manager 方式安装本套件，使用下面的链接进行导入：

https://github.com/VyronLee/vFrame.ResourceToolset.git#upm

如需指定版本，链接后面带上版本号即可

## 工具套件设置

使用前需要先对套件参数进行初始化设置

通过菜单 Tools -> vFrame -> Resource Toolset -> Settings 打开设置面板

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_11-49-31.png)

点击按钮1，选择资源保存的路径

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_11-50-10.png)

分别点击按钮1、2、3，生成动画曲线优化设置、内置资源设置、资源导入设置

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_11-52-05.png)

`AnimationOptimizationConfig` 动画优化设置

* `Precision` 动画曲线裁剪精度保留位数，按需要设置，默认3（即保留小数点后3位）

`BuiltinAssetConfig` 内置资源设置

* `Builtin Replacement Materials Dir` 用于替换内置材质的资源目录（可直接使用套件内提供的）
* `Builtin Replacement Texture Dir` 用于替换内置贴图的资源目录（可直接使用套件内提供的）
* `Fbx Internal Material Replacement` FBX文件内部材质球替换
* `Auto Replace FBX Internal Material On Import` FBX导入时是否自动替换内部材质

`AssetImportConfig` 资源导入设置

* `Asset Hash Cache Directory` 资源哈希缓存保存路径
* `Rule Hash Cache File` 导入规则哈希缓存文件路径

## 使用说明

### FBX内置材质替换工具

FBX文件导入后，会引用默认材质`Default-Material`，这个材质使用的是`Standard Shader`。项目中我们一般都不希望打包时引入这个Shader，因为它的变体非常多，会导致运行时ShaderLab内存占用很大。因此我们需要把它的引用替换为比较简单的Shader。

使用的方法很简单，只需要在[工具套件设置](#工具套件设置])中设置好需要替换的Shader文件，然后勾上`Auto Replace FBX Internal Material On Import`即可，下次导入FBX则会自动替换掉。

### 动画曲线精度优化以及曲线裁剪工具

#### 曲线精度优化原理

动画是通过关键帧（keyframes）来记录和播放的。每个关键帧包含了在特定时间点上的属性值，比如位置、旋转或者缩放等。动画曲线（animation curve）则是连接这些关键帧的折线或曲线，它定义了属性值随时间变化的方式。动画制作时（尤其是模型动画）会有大量密集的关键帧，因此会带来大量的动画曲线，但是这些曲线连接的两个关键帧之间的属性值差异有时会非常小（比如：两个缩放比例1.011223f，1.0112551f），实际上这种情况非常之多。我们可以通过裁剪数值的精度来达到把Dense Curve优化成Constant Curve，来减少曲线的数目，从而达到减少内存占用的目的（存储的数值精度实际上是没变的，变化的是曲线的数目）。

#### 曲线裁剪原理

曲线的裁剪，一般指的是把所有`Scale曲线`去除来到达减少曲线的目的。对于一般的模型动画，使用到Scale曲线的几率普遍较少，因此可以使用这种方式来优化。但是，这种副作用需要自行衡量，如果项目中实际上的确需要用到Scale曲线，则不能去除该曲线，否则动画表现上会异常。

#### 使用方式

1. 可以直接通过右键菜单来操作

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_14-49-26.png)

2. 通过自行调用以下接口来实现

   ```csharp
   public static class AnimationOptimizationUtils
   {
       public static bool ModifyCurveValuesPrecision(AnimationClip clip, uint precision);
       public static bool RemoveScaleCurve(AnimationClip clip);
   }
   ```

### Unity 内置资源替换工具

Unity 的内置资源（如：Resources/unity_builtin_extra），在资源的制作过程中我们有时候会无意中引入，比较常见的如：`Materials/Default UI Material`、`Materials/Default-Skybox`、`UI/Skin/Background`等。引入内置资源导致的坏处有两点：

1. 资源重复出现在多个 AssetBundle 中
2. 内置贴图由于无法打成图集，会导致合批失败

因此我们需要把内置资源使用项目中实际存在的资源进行替代，来达到更加自由的控制效果。

#### 使用方式

需要先在[工具套件设置](#工具套件设置)中配置好替换的资源相关目录，也可以直接使用套件提供的从内置资源导出的资源（vFrame.ResourceToolset/Resources）

1. 可以直接通过右键菜单来操作

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_15-23-43.png)

2. 通过自行调用以下接口来实现

   ```csharp
   public static class BuiltinAssetsReplacementUtils
   {
       public static bool ReplaceBuiltinAssets(Object obj);
   }
   ```

### 资源 GUID 重新生成工具

对指定的资源重新生成GUID，**如果有其他资源引用到该资源，会自动替换成新的引用GUID**

#### 使用方式

*注意：由于该操作比较危险，执行之前请自行存档！！*

1. 可以直接通过右键菜单来操作

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_15-31-02.png)

2. 通过自行调用以下接口来实现

   ```csharp
   public static class GuidRegenerationUtils
   {
       public static bool RegenerateGuidsOfFiles(IEnumerable<string> targetFilePaths, string referencesDirectory, string[] fileExtensions = null);
       public static bool RegenerateGuidsOfFiles(IEnumerable<string> targetFilePaths, IEnumerable<string> referenceFilePaths);
   }
   ```


### 资源引用丢失查找工具

随着项目的迭代，会不停地有新资源的引入，以及旧资源的删除。当项目庞大到一定程度时，美术部门删除资源时往往会无法准确判断该资源所被使用的地方有哪些，导致资源的丢失。如果不及时发现，会导致无法预测的显示异常。因此定时对所有资源进行”引用丢失排查“是一项很重要的事情。

#### 使用方式

1. 可以直接通过右键菜单来操作
    ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_16-53-17.png)


2. 通过自行调用以下接口来实现

   ```csharp
   public static class MissingReferenceValidationUtils
   {
       public static bool ValidateAsset(Object obj, out List<string> missing); // 校验传入的资源对象是否有丢失的引用
       public static bool ValidateActiveScene(out List<string> missing); // 校验当前激活的场景内是否有丢失的引用
       public static bool RemoveMissingReference(Object obj, out List<string> missing); // 移除资源对象内所有丢失的引用（置空）
   }
   ```

### 资源文件信息打印工具

主要用于方便地输出资源文件的GUID、File ID、序列化数据以及引用关系

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_16-58-49.png)

### 资源ID映射工具

资源ID映射，主要是为了对需要控制的资源建立一个 固定ID 与 资源 之间的实际映射关系。所有资源ID不会重复，因此可在程序中直接使用ID的方式来扩展资源加载。建立这种映射关系是很有必要的，因为实际项目中我们经常需要对不规范的文件名称进行修正，或者需要挪动资源的位置以建立更加规范的结构等。如果直接使用”文件路径“的方式来进行配置，避免不了需要对所有相关的代码以及配置表进行调整，容易出错而且工作量相当大。

#### 使用方式

可通过菜单 Tools -> vFrame -> Resource Toolset -> Asset ID Mapper 来打开面板

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_17-14-11.png)

面板主要分为两部分：

1. 资源映射分组

   可通过右下角的`Add Asset`按钮新建条目，然后自行拖拽资源上去；或者在 Project 面板中选好需要添加的资源，然后点击`Add Selected`按钮一键添加

2. 操作栏区域

   可根据需要添加不同类型资源的映射分组，每个分组需要指定一个”组索引“，而且不能重复，ID区间需要自行规划

当所有资源映射关系建立或者修改完成后，点击`Save & Export`按钮，会进行保存以及输出映射关系数据。映射数据结构为一个字典，ID -> 文件路径，实际加载资源时仍然是根据资源路径来进行加载。

### 资源导入设置工具

游戏客户端是个很巨大的工程，其中含有各种类型的资源，包括贴图、音频、模型、材质、动画等等。当一个项目迭代了一两年后，工程中的文件往往是数以万计的。要保证每个资源文件都正确的设置好，是个非常困难的任务，要靠规范的流程，要靠严格遵守的人员，更要靠一系列的自动化工具。该资源导入设置工具为了能够方便、快捷地管理工程中各类型资源而设计，能够减少各种重复、繁琐的人工操作。

#### 使用方式

可通过菜单 Tools -> vFrame -> Resource Toolset -> Asset Importer 来打开面板

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_21-13-29.png)

首次打开，需要点击`Create Importer Rule`来创建你的第一个规则，在界面中间弹出的规则类型框选择需要的规则（目前只支持贴图资源的设置）。

上图中虚线框（2）为贴图规则设置部分，可以按需选择需要生效的参数（复选框勾上），然后设置好各参数，点击`Save`保存规则。

如果需要马上应用到对应的资源上，可以点击`Import`/`Force Import`按钮然后等待导入完成即可。为了加快平时资源导入的速度，点击`Import`前会先对资源进行哈希校验，如果发现跟本地缓存中保存的值一致的话，该资源则跳过导入；点击`Force Import`按钮会强制导入所有资源，跳过哈希校验。

你也可以直接通过下图所示菜单进行导入，不必打开该面板：

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_21-36-52.png)

注意跟 Unity 菜单上自带的`Reimport`/`Reimport All`菜单的区别，只有通过`vFrame/Resource Toolset`入口下的`Reimport`才会执行该资源匹配到的规则然后执行导入流程。

#### 规则扩展

你可以按照自己的实际需要自定义导入规则逻辑，只需要在工程中继承并实现下面接口即可

```csharp
public abstract class AssetImporterRuleBase<T> : AssetImporterRuleBase where T: AssetImporter
{
    protected abstract bool ProcessImport(T assetImporter);
}
```

然后在面板中重新点击创建规则，在弹出的类型选项框中选择新的规则类型。

### 资源迁移工具

为了能够在项目开发的后期方便地对资源进行分析、管理与批量操作（包括移动、复制、删除资源），同时保证引用关系不改变，而开发了该资源迁移工具。

#### 使用方式

可通过菜单 Tools -> vFrame -> Resource Toolset -> Asset Migrator 来打开面板

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_22-16-20.png)

1. 在 Project 面板中选择需要进行分析的资源，然后拖动到上图`Drop Any Object Here`处;

2. 点击`Filter Process Targets`，会对目标资源进行过滤分析。如果需要调整过滤范围，可在`Filters`部分修改相应的参数;

3. 点击`Refresh Dependencies`，会对目标资源进行依赖分析，分析结束后会列出引用的各个资源，如下图所示：

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_22-17-00.png)

4. 点击各资源的`Move`、`Duplicate`、`Replace`、`Delete`按钮即可进行相应的操作。如果需要批量进行，可先选择需要的资源，然后使用面板最底端的按钮即可。

## License

[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)