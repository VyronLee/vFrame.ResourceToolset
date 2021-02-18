using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class ReplaceBuiltinAsset
    {
        private static readonly string[] ManagedAssetTypes = {".unity", ".mat", ".prefab" };

        private static List<Object> GetSelectedObjects() {
            var selection = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(selection);
            if (!AssetDatabase.IsValidFolder(path))
                return new List<Object> {selection};

            var assets = AssetDatabase.FindAssets("t:Object", new[] {path});
            var objects = new List<Object>();
            var index = 0f;
            foreach (var asset in assets) {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                EditorUtility.DisplayProgressBar("Filtering Assets", p, ++index / assets.Length);

                if (!ManagedAssetTypes.Any(v => p.ToLower().EndsWith(v))) {
                    continue;
                }
                objects.Add(AssetDatabase.LoadAssetAtPath<Object>(p));
            }

            EditorUtility.ClearProgressBar();
            return objects;
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Replace Builtin Assets")]
        private static void ReplaceBuiltinAssets() {
            var objects = GetSelectedObjects();
            var index = 0f;
            var ret = false;
            var changed = new List<string>();
            try {
                foreach (var obj in objects) {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Replacing Builtin Assets", path, ++index / objects.Count)) {
                        break;
                    }

                    ret |= BuiltinAssetsReplacementUtils.ReplaceBuiltinAssets(obj);
                    if (ret) {
                        changed.Add(path);
                    }
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            if (!ret) {
                Debug.Log("Replace builtin assets finished, nothing changed.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Resources.UnloadUnusedAssets();

            Debug.Log("Replace builtin assets finished, asset files list below has been processed: \n"
                + string.Join("\n", changed.ToArray()));
        }
    }
}