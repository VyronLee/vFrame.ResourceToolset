using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Windows.IdMapper
{
    internal static class AssetIdsMapperUtils
    {
        public static T[] GetSelectObjectsRecursive<T>(bool wantsSubAssets) where T : Object {
            var objects = Selection.objects;
            var ret = new HashSet<T>();
            foreach (var obj in objects) {
                // Avoid error: Do not use ReadObjectThreaded on scene objects!
                if (obj is SceneAsset) {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path)) {
                    GetObjectsInDirectory(path, wantsSubAssets, ret);
                }
                else if(obj is T t) {
                    ret.Add(t);
                }

                if (!wantsSubAssets) {
                    continue;
                }
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                ret.AddRange(subAssets.Where(v => v is T).Cast<T>());
            }
            return ret.ToArray();
        }

        private static void GetObjectsInDirectory<T>(string dirPath, bool wantsSubAssets, HashSet<T> objects) where T : Object {
            var guids = AssetDatabase.FindAssets("t: " + typeof(T).Name, new[] { dirPath });
            foreach (var guid in guids) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // Avoid error: Do not use ReadObjectThreaded on scene objects!
                if (wantsSubAssets && !assetPath.EndsWith(".unity")) {
                    var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    objects.AddRange(assets.Where(v => v is T).Cast<T>());
                }
                else {
                    var obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (obj == null)
                        continue;
                    objects.Add(obj);
                }
            }
        }

        public static void ShowNotification(string message) {
            EditorWindow.focusedWindow.ShowNotification(new GUIContent(message));
        }

        public static void Export(AssetIdsMapperSO aimSO, string exportJsonPath) {
            if (string.IsNullOrEmpty(exportJsonPath)) {
                return;
            }

            CollectAssetIdsAndConflictInfo(aimSO, out var ret, out var conflicted);

            if (conflicted.Count > 0) {
                PrintConflictedAssetsInfo(conflicted);
                ShowNotification("Export failed, see console for more info.");
            }
            else {
                ExportJsonAndWriteToFile(ret, exportJsonPath);
                ShowNotification("Export succeeded!");
            }
        }

        private static void CollectAssetIdsAndConflictInfo(AssetIdsMapperSO aimSO,
            out AssetIdsMapperSerializable ret,
            out Dictionary<string, List<string>> conflicted)
        {
            ret = new AssetIdsMapperSerializable();
            conflicted = new Dictionary<string, List<string>>();

            foreach (var mapperGroup in aimSO.Groups) {
                foreach (var groupItem in mapperGroup.Assets) {
                    var path = AssetDatabase.GetAssetPath(groupItem.Asset);
                    if (!ret.ContainsKey(groupItem.AssetId)) {
                        ret.Add(groupItem.AssetId, path);
                        continue;
                    }
                    // asset id conflict
                    if (!conflicted.TryGetValue(groupItem.AssetId, out var list)) {
                        list = conflicted[groupItem.AssetId] = new List<string>();
                        list.Add(ret[groupItem.AssetId]);
                    }
                    list.Add(path);
                }
            }
        }

        private static void PrintConflictedAssetsInfo(Dictionary<string, List<string>> conflicted) {
            if (conflicted.Count <= 0) {
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Conflict asset ids found:");
            foreach (var kv in conflicted) {
                sb.AppendLine(kv.Key);
                foreach (var path in kv.Value) {
                    sb.AppendLine($"    - {path}");
                }
            }
            Debug.LogWarning(sb.ToString());
        }

        private static void ExportJsonAndWriteToFile(AssetIdsMapperSerializable val, string exportJsonPath) {
            var jsonData = JsonUtility.ToJson(val);
            var dir = Path.GetDirectoryName(exportJsonPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(exportJsonPath, jsonData);
        }
    }
}