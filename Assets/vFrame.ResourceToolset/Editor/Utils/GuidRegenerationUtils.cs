using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class GuidRegenerationUtils
    {
        internal static readonly string[] DefaultFileExtensions = {
            "*.meta",
            "*.mat",
            "*.anim",
            "*.prefab",
            "*.unity",
            "*.asset",
            "*.guiskin",
            "*.fontsettings",
            "*.controller",
        };

        public static bool RegenerateGuidsInDirectory(string targetDirectory,
            string referencesDirectory,
            string[] fileExtensions = null)
        {
            if (string.IsNullOrEmpty(targetDirectory)) {
                throw new ArgumentNullException(nameof(targetDirectory));
            }
            if (string.IsNullOrEmpty(referencesDirectory)) {
                throw new ArgumentNullException(nameof(referencesDirectory));
            }

            if (Path.IsPathRooted(targetDirectory)) {
                if (!targetDirectory.StartsWith(Application.dataPath)) {
                    Debug.LogError("Only directory in 'Application.dataPath' supported!");
                    return false;
                }
            }

            if (Path.IsPathRooted(referencesDirectory)) {
                if (!referencesDirectory.StartsWith(Application.dataPath)) {
                    Debug.LogError("Only directory in 'Application.dataPath' supported!");
                    return false;
                }
            }

            if (fileExtensions == null)
                fileExtensions = DefaultFileExtensions;

            var targetFilePaths = GetAllFiles(targetDirectory, fileExtensions);
            var referenceFilePaths = GetAllFiles(referencesDirectory, fileExtensions);
            return RegenerateGuidsOfFiles(targetFilePaths, referenceFilePaths);
        }

        public static bool RegenerateGuidsOfFiles(IEnumerable<string> targetFilePaths,
            string referencesDirectory,
            string[] fileExtensions = null)
        {
            if (null == targetFilePaths) {
                throw new ArgumentNullException(nameof(targetFilePaths));
            }
            if (string.IsNullOrEmpty(referencesDirectory)) {
                throw new ArgumentNullException(nameof(referencesDirectory));
            }

            if (Path.IsPathRooted(referencesDirectory)) {
                if (!referencesDirectory.StartsWith(Application.dataPath)) {
                    Debug.LogError("Only directory in 'Application.dataPath' supported!");
                    return false;
                }
            }

            if (fileExtensions == null)
                fileExtensions = DefaultFileExtensions;

            var referenceFilePaths = GetAllFiles(referencesDirectory, fileExtensions);
            return RegenerateGuidsOfFiles(targetFilePaths, referenceFilePaths);
        }

        public static bool RegenerateGuidsOfFiles(IEnumerable<string> targetFilePaths, IEnumerable<string> referenceFilePaths) {
            if (null == targetFilePaths) {
                throw new ArgumentNullException(nameof(targetFilePaths));
            }
            if (null == referenceFilePaths) {
                throw new ArgumentNullException(nameof(referenceFilePaths));
            }
            try {
                var regenerator = new UnityGuidRegenerator(referenceFilePaths, targetFilePaths);
                return regenerator.RegenerateGuids();
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        public static bool RegenerateGuidsOfObjects(IEnumerable<Object> targetObjects, IEnumerable<Object> referencesObjects) {
            if (null == targetObjects) {
                throw new ArgumentNullException(nameof(targetObjects));
            }
            if (null == referencesObjects) {
                throw new ArgumentNullException(nameof(referencesObjects));
            }

            var targetFilePaths = new HashSet<string>();
            foreach (var obj in targetObjects) {
                var path = AssetDatabase.GetAssetPath(obj);
                targetFilePaths.Add(path);
                targetFilePaths.Add(path + ".meta");
            }

            var referenceFilePaths = new HashSet<string>();
            foreach (var obj in referencesObjects) {
                var path = AssetDatabase.GetAssetPath(obj);
                referenceFilePaths.Add(path);
                referenceFilePaths.Add(path + ".meta");
            }

            return RegenerateGuidsOfFiles(targetFilePaths, referenceFilePaths);
        }

        internal static List<string> GetAllFiles(string directory, string[] fileExtensions) {
            var ret = new List<string>();
            foreach (var extension in fileExtensions) {
                ret.AddRange(Directory.GetFiles(directory, extension, SearchOption.AllDirectories));
            }
            ret = ret.ConvertAll(v => v.Replace("\\", "/"));
            if (Path.IsPathRooted(directory)) {
                ret = ret.ConvertAll(v => v.MakeRelativePath(directory));
            }
            return ret;
        }
    }

    internal class UnityGuidRegenerator
    {
        private readonly List<string> _allAssetsPaths;
        private readonly List<string> _regenerateGuidFilesPaths;

        public UnityGuidRegenerator(IEnumerable<string> allAssetsDirectory, IEnumerable<string> regenerateAssetsDirectory) {
            _allAssetsPaths = allAssetsDirectory.ToList();
            _regenerateGuidFilesPaths = regenerateAssetsDirectory.ToList();
        }

        public bool RegenerateGuids() {
            // Create dictionary to hold old-to-new GUID map
            var guidOldToNewMap = new Dictionary<string, string>();
            var guidsInFileMap = new Dictionary<string, List<string>>();

            // We must only replace GUIDs for Resources present in Assets.
            // Otherwise built-in resources (shader, meshes etc) get overwritten.
            var ownGuids = new HashSet<string>();

            // Traverse all files, remember which GUIDs are in which files and generate new GUIDs
            var counter = 0;
            foreach (var filePath in _allAssetsPaths) {
                EditorUtility.DisplayProgressBar("Scanning Assets folder", filePath,
                    counter / (float) _regenerateGuidFilesPaths.Count);
                var contents = File.ReadAllText(filePath);

                var guids = GetGuids(contents);
                var isFirstGuid = true;
                foreach (var oldGuid in guids) {
                    // First GUID in .meta file is always the GUID of the asset itself
                    if (isFirstGuid && Path.GetExtension(filePath) == ".meta" && _regenerateGuidFilesPaths.Contains(filePath)) {
                        ownGuids.Add(oldGuid);
                        isFirstGuid = false;

                        // Generate and save new GUID if we haven't added it before
                        if (guidOldToNewMap.ContainsKey(oldGuid)) {
                            throw new Exception($"Guid duplicated: {oldGuid}, file: {filePath}");
                        }

                        var newGuid = Guid.NewGuid().ToString("N");
                        guidOldToNewMap.Add(oldGuid, newGuid);

                        Debug.Log($"Guid regenerated, old: {oldGuid}, new : {newGuid}, file: {filePath}");
                    }

                    if (!guidsInFileMap.ContainsKey(filePath))
                        guidsInFileMap[filePath] = new List<string>();

                    if (!guidsInFileMap[filePath].Contains(oldGuid))
                        guidsInFileMap[filePath].Add(oldGuid);
                }

                counter++;
            }

            // Traverse the files again and replace the old GUIDs
            counter = -1;
            var guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
            var modified = false;
            foreach (var filePath in guidsInFileMap.Keys) {
                EditorUtility.DisplayProgressBar("Regenerating GUIDs", filePath,
                    counter / (float) guidsInFileMapKeysCount);
                counter++;

                var contents = File.ReadAllText(filePath);
                foreach (var oldGuid in guidsInFileMap[filePath]) {
                    if (!ownGuids.Contains(oldGuid))
                        continue;

                    if (!guidOldToNewMap.ContainsKey(oldGuid)) {
                        continue;
                    }

                    var newGuid = guidOldToNewMap[oldGuid];
                    if (string.IsNullOrEmpty(newGuid))
                        throw new NullReferenceException("newGuid == null");

                    contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
                    Debug.Log($"Replace guid: {oldGuid} with: {newGuid} in file: {filePath}");
                    modified = true;
                }

                File.WriteAllText(filePath, contents);
            }

            EditorUtility.ClearProgressBar();
            return modified;
        }

        private static IEnumerable<string> GetGuids(string text) {
            const string guidStart = "guid: ";
            const int guidLength = 32;
            var textLength = text.Length;
            var guidStartLength = guidStart.Length;
            var guids = new List<string>();

            var index = 0;
            while (index + guidStartLength + guidLength < textLength) {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                var guid = text.Substring(index, guidLength);
                index += guidLength;

                if (IsGuid(guid))
                    guids.Add(guid);
            }

            return guids;
        }

        private static bool IsGuid(string text) {
            for (var i = 0; i < text.Length; i++) {
                var c = text[i];
                if (
                    !(c >= '0' && c <= '9' ||
                      c >= 'a' && c <= 'z')
                )
                    return false;
            }

            return true;
        }
    }
}