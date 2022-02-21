using UnityEditor;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class BuiltinAssetReplacer
    {
        private static readonly string[] ManagedAssetExtensions = {".unity", ".mat", ".prefab" };

        [MenuItem(ToolsetConst.AssetsMenuDir + "Replace Builtin Assets")]
        private static void ReplaceBuiltinAssets() {
            AssetProcessorUtils.TraversalAndProcessSelectedObjects(ManagedAssetExtensions,
                "Replace Builtin Assets",
                BuiltinAssetsReplacementUtils.ReplaceBuiltinAssets);
        }
    }
}