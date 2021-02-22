using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class AssetProcessorUtils
    {
        public static List<Object> GetSelectedObjects(string[] extensions) {
            var selection = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(selection);
            if (!AssetDatabase.IsValidFolder(path))
                return new List<Object> {selection};

            return GetObjectsInDirectory(path, extensions);
        }

        public static List<Object> GetObjectsInDirectory(string dir, string[] extensions) {
            if (string.IsNullOrEmpty(dir)) {
                throw new ArgumentException("Argument cannot be null", nameof(dir));
            }

            var assets = AssetDatabase.FindAssets("t:Object", new[] {dir});
            var objects = new List<Object>();
            var index = 0f;
            foreach (var asset in assets) {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                EditorUtility.DisplayProgressBar("Filtering Assets", p, ++index / assets.Length);

                if (!extensions.Any(v => p.ToLower().EndsWith(v))) {
                    continue;
                }
                objects.Add(AssetDatabase.LoadAssetAtPath<Object>(p));
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

        private static void TravelAndProcessObjects(IReadOnlyCollection<Object> objects, string title, Func<Object, bool> processor) {
            var index = 0f;
            var changed = new List<string>();
            try {
                foreach (var obj in objects) {
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
    }
}