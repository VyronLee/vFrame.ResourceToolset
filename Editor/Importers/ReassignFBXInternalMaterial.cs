using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Configs;

namespace vFrame.ResourceToolset.Editor.Importers
{
    public class ModelImporter : AssetPostprocessor
    {
        protected Material OnAssignMaterialModel(Material previousMaterial, Renderer renderer) {
            if (!BuiltinAssetConfig.Instance.AutoReplaceFBXInternalMaterialOnImport) {
                return previousMaterial;
            }
            var material = BuiltinAssetConfig.Instance.FBXInternalMaterialReplacement;
            if (material) {
                return material;
            }
            Debug.LogError("FBX internal material must be specified.");
            return previousMaterial;
        }
    }
}