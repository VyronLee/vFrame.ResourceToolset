using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Exceptions;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class AssetProcessorUtils
    {
        private const int UnloadAssetsOnProcessedCount = 200;
        private const string WildcardExt = ".*";

        /// <summary>
        /// Return all asset file paths of selection EXCLUDE directory objects.
        /// </summary>
        /// <returns></returns>
        public static string[] GetSelectedObjectPaths() {
             return GetSelectedObjectPaths(new[] { WildcardExt });
        }

        /// <summary>
        /// Return specified asset file paths with extensions of selection EXCLUDE directory objects.
        /// </summary>
        /// <returns></returns>
        public static string[] GetSelectedObjectPaths(string[] extensions) {
             return GetSelectedObjectGUIDs(extensions).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        }

        /// <summary>
        /// Return all asset guids of selection EXCLUDE directory objects.
        /// </summary>
        /// <returns></returns>
        public static string[] GetSelectedObjectGUIDs() {
            return GetSelectedObjectGUIDs(new[] { WildcardExt });
        }

        /// <summary>
        /// Return specified asset guids with extensions of selection EXCLUDE directory objects.
        /// </summary>
        /// <returns></returns>
        public static string[] GetSelectedObjectGUIDs(string[] extensions) {
            var selection = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(selection);
            if (!AssetDatabase.IsValidFolder(path))
                return new[] {AssetDatabase.AssetPathToGUID(path)};

            return GetObjectGUIDsInDirectory(path, extensions);
        }

        public static string[] GetObjectGUIDsInDirectory(string dir, string[] extensions) {
            if (string.IsNullOrEmpty(dir)) {
                throw new ArgumentException("Argument cannot be null", nameof(dir));
            }

            var assets = AssetDatabase.FindAssets("t:Object", new[] {dir});
            var objects = new HashSet<string>();
            var index = 0f;
            foreach (var asset in assets) {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                EditorUtility.DisplayProgressBar("Filtering Assets", p, ++index / assets.Length);

                if (extensions.All(ext => ext != WildcardExt) && !extensions.Any(v => p.ToLower().EndsWith(v))) {
                    continue;
                }

                objects.Add(asset);
            }

            EditorUtility.ClearProgressBar();
            return objects.ToArray();
        }

        public static void TravelAndProcessSelectedObjects(string[] extensions, string title, Func<Object, bool> processor) {
            var objects = GetSelectedObjectGUIDs(extensions);
            TravelAndProcessObjects(objects, title, processor);
        }

        public static void TravelAndProcessObjectsInDirectory(string dir, string[] extensions, string title,
            Func<Object, bool> processor) {
            var objects = GetObjectGUIDsInDirectory(dir, extensions);
            TravelAndProcessObjects(objects, title, processor);
        }

        public static void TravelSelectedObjects(string[] extensions, string title, Action<Object> processor) {
            var objects = GetSelectedObjectGUIDs(extensions);
            TravelObjects(objects, title, processor);
        }

        public static void TravelObjectsInDirectory(string dir, string[] extensions, string title, Action<Object> processor) {
            var objects = GetObjectGUIDsInDirectory(dir, extensions);
            TravelObjects(objects, title, processor);
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

        public static void ForceSaveAsset(this Object asset) {
            switch (asset) {
                case GameObject gameObject: {
                    if (!gameObject.IsPrefab()) {
                        throw new ResourceToolsetException("Save failed, target asset not prefab.");
                    }
                    PrefabUtility.SavePrefabAsset(gameObject);
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

        public static string CalculateAssetHash(string path) {
            if (!File.Exists(path)) {
                return string.Empty;
            }

            var metaMd5 = "";
            var meta = path + ".meta";
            if (File.Exists(meta)) {
                metaMd5 = CalculateFileMD5(meta);
            }

            var fileMd5 = CalculateFileMD5(path);
            if (string.IsNullOrEmpty(metaMd5)) {
                return fileMd5;
            }
            return CalculateMD5(fileMd5 + metaMd5);
        }

        internal static string CalculateFileMD5(string path) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(path)) {
                    var hash = md5.ComputeHash(stream);
                    var hashValue = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return hashValue;
                }
            }
        }

        internal static string CalculateMD5(string content) {
            using (var md5 = MD5.Create()) {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                    var hash = md5.ComputeHash(stream);
                    var hashValue = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return hashValue;
                }
            }
        }

        internal static string MakeRelativePath(this string fromPath, string toPath) {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = toUri.MakeRelativeUri(fromUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }

        internal static void RunAndWait(this IEnumerator enumerator) {
            while (enumerator.MoveNext()) {
                if (enumerator.Current is IEnumerator current) {
                    current.RunAndWait();
                }
            }
        }

        internal static bool IsPrefab(this GameObject go) {
            return !go.scene.IsValid()
                   || go.scene.name == go.name
                   || go.scene.name == null
                   || go.scene.name == "";
        }
    }
}