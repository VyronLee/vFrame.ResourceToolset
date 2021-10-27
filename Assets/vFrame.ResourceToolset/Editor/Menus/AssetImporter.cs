using UnityEditor;
using vFrame.ResourceToolset.Editor.Const;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class AssetImporter
    {
        [MenuItem(ToolsetConst.AssetsMenuDir + "Import/Reimport", false)]
        private static void ReimportSelectedAssets() {

        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Import/Force Reimport", false)]
        private static void ForceReimportSelectedAssets() {

        }
    }
}