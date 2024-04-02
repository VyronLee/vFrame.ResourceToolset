using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class AssetMigrationUtils
    {
        public static bool ReplaceAsset(Object targetToProcess, Object assetToReplace, Object assetReplaceWith) {
            if (!targetToProcess || !assetToReplace || !assetReplaceWith) {
                return false;
            }

            if (assetToReplace.GetType() != assetReplaceWith.GetType()) {
                Debug.LogError("Cannot replace asset with different type!");
                return false;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assetToReplace,
                out var toReplaceGuid,
                out long toReplaceFileId))
            {
                Debug.LogError("Get source asset guid and fileId failed.");
                return false;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assetReplaceWith,
                out var replaceWithGuid,
                out long replaceWithFileId))
            {
                Debug.LogError("Get target asset guid and fileId failed.");
                return false;
            }

            var targetPath = AssetDatabase.GetAssetPath(targetToProcess);
            if (string.IsNullOrEmpty(targetPath)) {
                Debug.LogError("Target to process not a valid asset!");
                return false;
            }

            return ReplaceAssetInternal(targetPath, toReplaceGuid, toReplaceFileId, replaceWithGuid, replaceWithFileId);
        }

        public static bool ReplaceAsset(Object[] targetsToProcess, Object assetToReplace, Object assetReplaceWith) {
            if (null == targetsToProcess) {
                throw new ArgumentNullException(nameof(targetsToProcess));
            }

            var ret = false;
            var index = 0f;
            try {
                foreach (var obj in targetsToProcess) {
                    var path = AssetDatabase.GetAssetPath(obj);
                    EditorUtility.DisplayProgressBar("Replacing asset", path, ++index / targetsToProcess.Length);
                    ret |= ReplaceAsset(obj, assetToReplace, assetReplaceWith);
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
            return ret;
        }

        public static bool ReplaceAsset(Object[] targetsToProcess, string assetPathToReplace, string assetPathReplaceWith) {
            if (null == targetsToProcess) {
                throw new ArgumentNullException(nameof(targetsToProcess));
            }
            if (string.IsNullOrEmpty(assetPathToReplace)) {
                throw new ArgumentNullException(nameof(assetPathToReplace));
            }
            if (string.IsNullOrEmpty(assetPathReplaceWith)) {
                throw new ArgumentNullException(nameof(assetPathReplaceWith));
            }

            var assetToReplace = AssetDatabase.LoadMainAssetAtPath(assetPathToReplace);
            var assetReplaceWith = AssetDatabase.LoadMainAssetAtPath(assetPathReplaceWith);
            return ReplaceAsset(targetsToProcess, assetToReplace, assetReplaceWith);
        }

        private static bool ReplaceAssetInternal(string targetPath,
            string toReplaceGuid,
            long toReplaceFileId,
            string replaceWithGuid,
            long replaceWithFileId)
        {
            if (!File.Exists(targetPath)) {
                return false;
            }

            var toReplace = $"{{fileID: {toReplaceFileId}, guid: {toReplaceGuid},";
            var replaceWith = $"{{fileID: {replaceWithFileId}, guid: {replaceWithGuid},";

            var content = File.ReadAllText(targetPath);
            if (!content.Contains(toReplace)) {
                return false;
            }

            content = content.Replace(toReplace, replaceWith);
            File.WriteAllText(targetPath, content);
            return true;
        }

        internal static Object LoadAssetAtPathWithFileId(string path, long fileIdToLoad) {
            var objects = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in objects) {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long fileId))
                    continue;
                if (fileId == fileIdToLoad) {
                    return obj;
                }
            }
            return null;
        }

        internal static Object[] LoadAllAssetsAtPath(string path) {
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            // Avoid error: "Do not use ReadObjectThreaded on scene objects!"
            if (mainAsset is SceneAsset) {
                return new[] { mainAsset };
            }
            return AssetDatabase.LoadAllAssetsAtPath(path);
        }

        public static void CollectDependencies(Object obj, ref HashSet<Object> dependencies, bool recursive) {
            var objPath = AssetDatabase.GetAssetPath(obj);
            var resultPaths = AssetDatabase.GetDependencies(objPath, recursive);
            var resultObjects = EditorUtility.CollectDependencies(new [] {obj});
            foreach (var resultObject in resultObjects) {
                if (obj == resultObject) {
                    continue;
                }
                var path = AssetDatabase.GetAssetPath(resultObject);
                if (resultPaths.Contains(path)) {
                    dependencies.Add(resultObject);
                }
            }
        }

        private static bool TryGetGUIDAndFileID(Object obj, out string guid, out long fileId) {
            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out fileId);
        }

    }
}