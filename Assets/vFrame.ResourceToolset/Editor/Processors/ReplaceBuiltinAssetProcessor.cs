using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Processors
{
    internal class ReplaceBuiltinAssetProcessor : AssetPostprocessor
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
                EditorUtility.DisplayProgressBar("Loading assets", p, ++index / assets.Length);

                if (!ManagedAssetTypes.Any(v => p.ToLower().EndsWith(v))) {
                    continue;
                }
                objects.Add(AssetDatabase.LoadAssetAtPath<Object>(p));
            }

            EditorUtility.ClearProgressBar();
            return objects;
        }

        [MenuItem(ToolsetConst.AssetProcessorMenuDir + "Replace Builtin Assets")]
        private static void ReplaceBuiltinAssets() {
            var objects = GetSelectedObjects();
            var index = 0f;
            var ret = false;
            try {
                foreach (var obj in objects) {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Replacing Builtin Assets", path, ++index / objects.Count)) {
                        break;
                    }

                    ret |= BuiltinAssetsReplacementUtils.ReplaceBuiltinAssets(obj);
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            if (!ret)
                return;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Resources.UnloadUnusedAssets();
        }
    }
}