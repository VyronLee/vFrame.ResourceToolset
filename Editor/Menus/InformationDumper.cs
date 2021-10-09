using UnityEditor;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class InformationDumper
    {
        [MenuItem(ToolsetConst.AssetsMenuDir + "Print/Print Asset Dependencies")]
        private static void PrintAssetDependencies() {
            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            InformationDumpUtils.PrintAssetDependencies(path);
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Print/Print Asset Guid")]
        private static void PrintAssetGuid() {
            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            InformationDumpUtils.PrintAssetGuid(path);
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Print/Print Asset Guid And FileId")]
        private static void PrintAssetGuidAndFileId() {
            var obj = Selection.activeObject;
            InformationDumpUtils.PrintAssetGuidAndFileId(obj);
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Print/Print Asset Serialized Data")]
        private static void PrintAssetSerializedData() {
            var selection = Selection.activeObject;
            if (!selection) {
                return;
            }
            var path = AssetDatabase.GetAssetPath(selection);
            var objects = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in objects) {
                InformationDumpUtils.PrintAssetSerializedData(obj);
            }
        }
    }
}