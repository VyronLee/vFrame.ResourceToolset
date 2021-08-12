using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class AssetProcessorUtils
    {
        private const int UnloadAssetsOnProcessedCount = 200;

        public static List<string> GetSelectedObjects(string[] extensions) {
            var selection = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(selection);
            if (!AssetDatabase.IsValidFolder(path))
                return new List<string> {AssetDatabase.AssetPathToGUID(path)};

            return GetObjectsInDirectory(path, extensions);
        }

        public static List<string> GetObjectsInDirectory(string dir, string[] extensions) {
            if (string.IsNullOrEmpty(dir)) {
                throw new ArgumentException("Argument cannot be null", nameof(dir));
            }

            var assets = AssetDatabase.FindAssets("t:Object", new[] {dir});
            var objects = new List<string>();
            var index = 0f;
            foreach (var asset in assets) {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                EditorUtility.DisplayProgressBar("Filtering Assets", p, ++index / assets.Length);

                if (!extensions.Any(v => p.ToLower().EndsWith(v))) {
                    continue;
                }

                objects.Add(asset);
            }

            EditorUtility.ClearProgressBar();
            return objects;
        }

        public static void TravelAndProcessSelectedObjects(string[] extensions, string title, Func<Object, bool> processor) {
            var objects = GetSelectedObjects(extensions);
            TravelAndProcessObjects(objects.ToArray(), title, processor);
        }

        public static void TravelAndProcessObjectsInDirectory(string dir, string[] extensions, string title,
            Func<Object, bool> processor) {
            var objects = GetObjectsInDirectory(dir, extensions);
            TravelAndProcessObjects(objects.ToArray(), title, processor);
        }

        public static void TravelSelectedObjects(string[] extensions, string title, Action<Object> processor) {
            var objects = GetSelectedObjects(extensions);
            TravelObjects(objects.ToArray(), title, processor);
        }

        public static void TravelObjectsInDirectory(string dir, string[] extensions, string title, Action<Object> processor) {
            var objects = GetObjectsInDirectory(dir, extensions);
            TravelObjects(objects.ToArray(), title, processor);
        }

        private static void TravelAndProcessObjects(IReadOnlyCollection<string> objects, string title, Func<Object, bool> processor) {
            var index = 0f;
            var changed = new List<string>();
            try {
                foreach (var objGuid in objects) {
                    if (index % UnloadAssetsOnProcessedCount == 0) {
                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                    }

                    var objPath = AssetDatabase.GUIDToAssetPath(objGuid);
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(objPath);
                    if (!obj) {
                        continue;
                    }

                    var path = AssetDatabase.GetAssetPath(obj);
                    if (EditorUtility.DisplayCancelableProgressBar(title, path, ++index / objects.Count)) {
                        break;
                    }

                    var ret = processor.Invoke(obj);
                    if (ret) {
                        changed.Add(path);
                    }
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            if (changed.Count <= 0) {
                Debug.Log($"{title} finished, nothing changed.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Resources.UnloadUnusedAssets();
            GC.Collect();

            Debug.Log($"{title} finished, asset files list below has been processed: \n"
                + string.Join("\n", changed.ToArray()));
        }

        private static void TravelObjects(IReadOnlyCollection<string> objects, string title, Action<Object> processor) {
            var index = 0f;
            try {
                foreach (var objGuid in objects) {
                    if (index % UnloadAssetsOnProcessedCount == 0) {
                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                    }

                    var objPath = AssetDatabase.GUIDToAssetPath(objGuid);
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(objPath);
                    if (!obj) {
                        continue;
                    }

                    if (EditorUtility.DisplayCancelableProgressBar(title, objPath, ++index / objects.Count)) {
                        break;
                    }

                    processor.Invoke(obj);
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"{title} finished.");
        }

        public static void ForceSaveAsset(Object asset) {
            switch (asset) {
                case GameObject prefab: {
                    PrefabUtility.SavePrefabAsset(prefab);
                    break;
                }
                case SceneAsset sceneAsset: {
                    var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                    break;
                }
                default: {
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                    break;
                }
            }
        }
    }
}