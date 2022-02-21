using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;
using vFrame.ResourceToolset.Editor.Windows.Importer;

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

            var rulesApplied = new HashSet<AssetImporterRuleBase>();
            try {
                var index = 0f;
                foreach (var path in paths) {
                    EditorUtility.DisplayProgressBar("Importing", path, ++index/paths.Length);
                    var ret = AssetImportUtils.ImportAsset(path, false);
                    ret.ForEach(r => rulesApplied.Add(r));
                }

                index = 0f;
                foreach (var rule in rulesApplied) {
                    EditorUtility.DisplayProgressBar("Saving", rule.GetSummary(), ++index/rulesApplied.Count);
                    rule.Save();
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool ReimportAlert() {
            return EditorUtility.DisplayDialog("Warning", ReimportAlertMessage, "Continue", "Cancel");
        }
    }
}