using UnityEditor;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class AssetImporter
    {
        private const int HugeAmountOfAssets = 100;
        private const string ReimportAlertMessage =
            "Reimport huge amount of assets (>100) may spend lots of time, SURE?";

        [MenuItem(ToolsetConst.AssetsMenuDir + "Reimport", false)]
        private static void ReimportSelectedAssets() {
            var paths = AssetProcessorUtils.GetSelectedObjectPaths();
            if (paths.Length >= HugeAmountOfAssets && !ReimportAlert()) {
                return;
            }
            foreach (var path in paths) {
                AssetImportUtils.ImportAsset(path);
            }
        }

        private static bool ReimportAlert() {
            return EditorUtility.DisplayDialog("Warning", ReimportAlertMessage, "Continue", "Cancel");
        }
    }
}