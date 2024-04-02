# vFrame Resource Toolset

![vFrame](https://img.shields.io/badge/vFrame-ResourceToolset-blue) [![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![License](https://img.shields.io/badge/License-Apache%202.0-brightgreen.svg)](#License)

As the development of a project progresses, the number of resource files in the project will increase, making management more difficult. It often becomes necessary to organize a large number of resources, identify redundant or unused assets, investigate which resources in the project have unreasonable parameter settings, or perform batch replacement and modification operations on resources. This repository's toolset is created to address the aforementioned issues.

[English Version (Power by ChatGPT)](./README_en.md)

## Table of Contents

* [Overview](#overview)
* [Installation](#installation)
* [Toolset Settings](#toolset-settings)
* [Usage Instructions](#usage-instructions)
    + [FBX Built-in Material Replacement Tool](#fbx-built-in-material-replacement-tool)
    + [Animation Curve Precision Optimization and Trimming Tool](#animation-curve-precision-optimization-and-trimming-tool)
        - [Curve Precision Optimization Principle](#curve-precision-optimization-principle)
        - [Curve Trimming Principle](#curve-trimming-principle)
        - [How to Use](#how-to-use)
    + [Unity Built-in Resource Replacement Tool](#unity-built-in-resource-replacement-tool)
        - [How to Use](#how-to-use)
    + [Resource GUID Regeneration Tool](#resource-guid-regeneration-tool)
        - [How to Use](#how-to-use)
    + [Missing Resource Reference Finder Tool](#missing-resource-reference-finder-tool)
        - [How to Use](#how-to-use)
    + [Resource File Information Printing Tool](#resource-file-information-printing-tool)
    + [Resource ID Mapping Tool](#resource-id-mapping-tool)
        - [How to Use](#how-to-use)
    + [Resource Import Settings Tool](#resource-import-settings-tool)
        - [How to Use](#how-to-use)
        - [Rule Extension](#rule-extension)
    + [Resource Migration Tool](#resource-migration-tool)
        - [How to Use](#how-to-use)
* [License](#license)

## Overview

The tools provided in this repository mainly include:
* FBX Built-in Material Replacement Tool
* Animation Curve Precision Optimization and Trimming Tool
* Unity Built-in Resource Replacement Tool
* Resource GUID Regeneration Tool
* Missing Resource Reference Finder Tool
* Resource File Information Printing Tool
* Resource ID Mapping Tool
* Resource Import Settings Tool
* Resource Migration Tool

## Installation

**Note**: This toolset depends on the paid plugin [OdinInspector](https://odininspector.com/), which you need to download and import into your project before use (Life is short, I use Odin!).

It is recommended to install this package using Unity Package Manager with the following link:

https://github.com/VyronLee/vFrame.ResourceToolset.git#upm

To specify a version, simply append the version number to the link.

## Toolset Settings

Before use, you need to initialize the settings of the toolset.

Open the settings panel through the menu Tools -> vFrame -> Resource Toolset -> Settings.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_11-49-31.png)

Click button 1 to select the path where resources are saved.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_11-50-10.png)

Click buttons 1, 2, and 3 respectively to generate settings for animation curve optimization, built-in resources, and resource import.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_11-52-05.png)

`AnimationOptimizationConfig` Animation Optimization Settings

* `Precision` The number of decimal places to keep for animation curve trimming precision. Set as needed, default is 3 (i.e., keep three decimal places).

`BuiltinAssetConfig` Built-in Resource Settings

* `Builtin Replacement Materials Dir` The directory of resources used to replace built-in materials (you can use the resources provided within the toolset directly).
* `Builtin Replacement Texture Dir` The directory of resources used to replace built-in textures (you can use the resources provided within the toolset directly).
* `Fbx Internal Material Replacement` FBX file internal material replacement.
* `Auto Replace FBX Internal Material On Import` Whether to automatically replace the internal materials when importing FBX files.

`AssetImportConfig` Resource Import Settings

* `Asset Hash Cache Directory` The path to save the resource hash cache.
* `Rule Hash Cache File` The path to the import rule hash cache file.

## Usage Instructions

### FBX Built-in Material Replacement Tool

After importing an FBX file, it will reference the default material `Default-Material`, which uses the `Standard Shader`. In our projects, we generally do not want to include this Shader in the build because it has many variants, which can cause a large memory footprint for ShaderLab at runtime. Therefore, we need to replace its reference with a simpler Shader.

The method of use is very simple. Just set the Shader file to be replaced in the [Toolset Settings](#toolset-settings), and then check `Auto Replace FBX Internal Material On Import`. The next time you import an FBX, it will automatically replace it.

### Animation Curve Precision Optimization and Trimming Tool

#### Curve Precision Optimization Principle

Animations record and play through keyframes. Each keyframe contains property values at a specific time point, such as position, rotation, or scale. An animation curve, which is a polyline or curve connecting these keyframes, defines how property values change over time. Animation production (especially model animation) will have a large number of dense keyframes, thus resulting in a large number of animation curves. However, the difference in property values between two connected keyframes is sometimes very small (e.g., two scale ratios 1.011223f, 1.0112551f), and this situation is quite common. We can optimize Dense Curves into Constant Curves by trimming the precision of values, reducing the number of curves and thereby reducing memory usage (the precision of stored values is actually unchanged, what changes is the number of curves).

#### Curve Trimming Principle

Curve trimming generally refers to removing all `Scale curves` to reduce the number of curves. For typical model animations, the use of Scale curves is relatively rare, so this method can be used for optimization. However, the side effects of this method need to be assessed on your own. If Scale curves are indeed needed in the project, they cannot be removed, otherwise, the animation will appear abnormal.

#### How to Use

1. You can operate directly through the context menu.

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_14-49-26.png)

2. Implement by calling the following interfaces on your own.

   ```csharp
   public static class AnimationOptimizationUtils
   {
       public static bool ModifyCurveValuesPrecision(AnimationClip clip, uint precision);
       public static bool RemoveScaleCurve(AnimationClip clip);
   }
   ```

### Unity Built-in Resource Replacement Tool

Unity's built-in resources (such as: Resources/unity_builtin_extra) are sometimes inadvertently introduced during the resource creation process. Common examples include `Materials/Default UI Material`, `Materials/Default-Skybox`, `UI/Skin/Background`, etc. The drawbacks of introducing built-in resources are twofold:

1. Resources appear repeatedly in multiple AssetBundles.
2. Built-in textures cannot be packed into atlases, leading to batching failures.

Therefore, we need to replace built-in resources with actual resources in the project to achieve more flexible control.

#### How to Use

First, you need to configure the directories for replacement resources in the [Toolset Settings](#toolset-settings), or you can directly use the resources exported from built-in resources provided by the toolset (vFrame.ResourceToolset/Resources).

1. You can operate directly through the context menu.

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_15-23-43.png)

2. Implement by calling the following interfaces on your own.

   ```csharp
   public static class BuiltinAssetsReplacementUtils
   {
       public static bool ReplaceBuiltinAssets(Object obj);
   }
   ```

### Resource GUID Regeneration Tool

Regenerate the GUID for a specified resource. **If other resources refer to this resource, the references will automatically be replaced with the new GUID.**

#### How to Use

*Note: Since this operation is quite risky, please back up your files before proceeding!!*

1. You can operate directly through the context menu.

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_15-31-02.png)

2. Implement by calling the following interfaces on your own.

   ```csharp
   public static class GuidRegenerationUtils
   {
       public static bool RegenerateGuidsOfFiles(IEnumerable<string> targetFilePaths, string referencesDirectory, string[] fileExtensions = null);
       public static bool RegenerateGuidsOfFiles(IEnumerable<string> targetFilePaths, IEnumerable<string> referenceFilePaths);
   }
   ```

### Missing Resource Reference Finder Tool

With the iteration of the project, new resources will continuously be introduced and old ones deleted. When a project becomes large, it is often difficult for the art department to accurately determine where a resource is used when deleting it, leading to lost resources. If not discovered in time, this can lead to unpredictable display anomalies. Therefore, it is important to perform a "missing reference check" on all resources regularly.

#### How to Use

1. You can operate directly through the context menu.

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_16-53-17.png)

2. Implement by calling the following interfaces on your own.

   ```csharp
   public static class MissingReferenceValidationUtils
   {
       public static bool ValidateAsset(Object obj, out List<string> missing); // Check if the passed resource object has any missing references.
       public static bool ValidateActiveScene(out List<string> missing); // Check if there are any missing references in the currently active scene.
       public static bool RemoveMissingReference(Object obj, out List<string> missing); // Remove all missing references within the resource object (set to null).
   }
   ```

### Resource File Information Printing Tool

Mainly used for conveniently outputting resource file GUID, File ID, serialized data, and reference relationships.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_16-58-49.png)

### Resource ID Mapping Tool

Resource ID mapping is primarily for establishing a fixed mapping relationship between a fixed ID and the actual resource that needs to be controlled. All resource IDs are unique, so in the program, the resources can be extended by directly using the ID. Establishing this mapping relationship is necessary because, in actual projects, we often need to correct non-standard file names or move resources to establish a more standardized structure. If we configure using the "file path," we inevitably need to adjust all related code and configuration tables, which is prone to errors and involves a significant amount of work.

#### How to Use

You can open the panel through the menu Tools -> vFrame -> Resource Toolset -> Asset ID Mapper.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_17-14-11.png)

The panel is mainly divided into two parts:

1. Resource Mapping Group

   New entries can be created by clicking the `Add Asset` button in the lower right corner and then dragging and dropping resources onto it; or by selecting the resources you want to add in the Project panel and clicking the `Add Selected` button to add them in one go.

2. Operation Bar Area

   You can add different types of resource mapping groups as needed. Each group needs to specify a "group index," which must be unique, and the ID range needs to be planned by yourself.

After all resource mapping relationships are established or modified, click the `Save & Export` button to save and output the mapping relationship data. The mapping data structure is a dictionary, ID -> file path, and actual resource loading is still based on the resource path.

### Resource Import Settings Tool

A game client is a huge project containing various types of resources, including textures, audio, models, materials, animations, etc. After a project has iterated for a year or two, the number of files in the project is often in the tens of thousands. Ensuring that each resource file is correctly set is a very difficult task, which depends on standardized processes, strict compliance by personnel, and a series of automation tools. This resource import settings tool is designed to manage various types of resources in the project conveniently and quickly, reducing various repetitive and tedious manual operations.

#### How to Use

You can open the panel through the menu Tools -> vFrame -> Resource Toolset -> Asset Importer.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_21-13-29.png)

When opening for the first time, you need to click `Create Importer Rule` to create your first rule. In the rule type box that pops up in the middle of the interface, select the rule you need (currently, only texture resource settings are supported).

The dashed box (2) in the image above is the texture rule settings part. You can select the parameters that need to be effective (check the checkbox) and then set each parameter and click `Save` to save the rule.

If you want to apply it to the corresponding resource immediately, you can click the `Import`/`Force Import` button and wait for the import to complete. To speed up the usual resource import, a hash check will be performed before clicking `Import`. If the resource is found to have the same value as the local cache, the resource will be skipped; clicking `Force Import` will force import all resources, skipping the hash check.

You can also import directly through the menu shown in the image below, without having to open the panel:

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_21-36-52.png)

Note the difference from the Unity menu's own `Reimport`/`Reimport All`. Only by going through the `vFrame/Resource Toolset` entry and clicking `Reimport` will the resource's matching rules be executed and the import process be carried out.

#### Rule Extension

You can customize the import rule logic according to your actual needs by inheriting and implementing the following interface in your project:

```csharp
public abstract class AssetImporterRuleBase<T> : AssetImporterRuleBase where T: AssetImporter
{
    protected abstract bool ProcessImport(T assetImporter);
}
```

Then click to create a new rule in the panel, and select the new rule type in the pop-up type selection box.

### Resource Migration Tool

To facilitate the analysis, management, and batch operations (including moving, copying, and deleting resources) of resources in the later stages of project development, while ensuring that the reference relationship remains unchanged, this resource migration tool was developed.

#### How to Use

You can open the panel through the menu Tools -> vFrame -> Resource Toolset -> Asset Migrator.

![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_22-16-20.png)

1. In the Project panel, select the resources you want to analyze and then drag them to the `Drop Any Object Here` area in the image above;

2. Click `Filter Process Targets` to filter and analyze the target resources. If you need to adjust the filtering range, you can modify the corresponding parameters in the `Filters` section;

3. Click `Refresh Dependencies` to analyze the dependencies of the target resources. After the analysis is complete, it will list the referenced resources as shown in the image below:

   ![](https://cdn.vyronlee.com/github-repo-resource-toolset/PixPin_2024-04-02_22-17-00.png)

4. Click the `Move`, `Duplicate`, `Replace`, `Delete` buttons for each resource to perform the corresponding operations. If you need to batch process, first select the required resources, then use the buttons at the bottom of the panel.

## License

[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)