using UnityEditor;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Exceptions;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class FBXModifiers
    {
        private static readonly string[] ManagedAssetExtensions = {".fbx"};

        [MenuItem(ToolsetConst.AssetsMenuDir + "FBX/Replace FBX Internal Materials")]
        private static void ReplaceMaterials() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<BuiltinAssetConfig>();
            if (!config) {
                return;
            }
            var newMaterial = config.FBXInternalMaterialReplacement;
            if (!newMaterial) {
                throw new BuiltinReplacementAssetNotAssignedException("FBX internal material must be specified.");
            }

            AssetProcessorUtils.TravelAndProcessSelectedObjects(ManagedAssetExtensions,
                "Replace FBX Internal Materials",
                v => FBXInternalMaterialReplacementUtils.ReplaceMaterial(AssetDatabase.GetAssetPath(v), newMaterial));
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "FBX/Remove FBX External Objects")]
        private static void RemoveExternalObjects() {
            AssetProcessorUtils.TravelAndProcessSelectedObjects(ManagedAssetExtensions,
                "Remove FBX External Objects",
                v => FBXInternalMaterialReplacementUtils.RemoveExternalObject(AssetDatabase.GetAssetPath(v)));
        }
    }
}