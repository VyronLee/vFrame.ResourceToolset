using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Common;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [HideReferenceObjectPicker]
    public abstract class AssetImporterRuleBase : ResourceToolsetConfig
    {
        #region Public API

        public bool FilterTest(string path) {
            return _filter?.FilterTest(path) ?? false;
        }

        public void ApplyTo(string path) {
            ApplyToInternal(path);
        }

        public void Import() {
            CoImportInternal(false).RunAndWait();
        }

        public void ForceImport() {
            CoImportInternal(true).RunAndWait();
        }

        #endregion

        #region Abstract

        protected abstract bool ProcessImport(AssetImporter assetImporter);

        #endregion

        #region Drawers

        protected const string GroupName = "RuleSettings";
        protected const string GroupName2 = GroupName + "/Processor";
        protected const int LabelWidth = 200;

        [SerializeField]
        [VerticalGroup(GroupName)]
        [InlineProperty]
        [HideLabel]
        [LabelWidth(LabelWidth)]
        private AssetFilter _filter = new AssetFilter();

        [ShowInInspector]
        [VerticalGroup(GroupName)]
        [HorizontalGroup(GroupName2)]
        [HideLabel]
        [ShowIf("_imported")]
        [ProgressBar(0f, 1f, 0.1695888f, 0.5964116f, 0.9716981f)]
        [ReadOnly]
        private float _importProgress;

        [VerticalGroup(GroupName)]
        [HorizontalGroup(GroupName2, Width = 100)]
        [Button(ButtonSizes.Small)]
        [LabelText("Import")]
        private void ImportAsset() {
            _imported = true;
            EditorCoroutineUtility.StartCoroutine(CoImportInternal(false), this);
        }

        [VerticalGroup(GroupName)]
        [HorizontalGroup(GroupName2, Width = 100)]
        [Button(ButtonSizes.Small)]
        [LabelText("Force Import")]
        private void ForceImportAsset() {
            _imported = true;
            EditorCoroutineUtility.StartCoroutine(CoImportInternal(true), this);
        }

        #endregion

#pragma warning disable 414

        private bool _imported;

#pragma warning restore 414

        private void ApplyToInternal(string path) {
            var hashData = GetOrCreateAssetHashData();
            var importer = AssetImporter.GetAtPath(path);
            if (!ProcessImport(importer)) {
                return;
            }
            Debug.Log("Asset re-import: " + path);

            var md5 = AssetProcessorUtils.CalculateAssetHash(path);
            hashData[path] = md5;
            SaveHashData(hashData);
        }

        private IEnumerator CoImportInternal(bool force) {
            var hashData = GetOrCreateAssetHashData();
            var updated = new List<string>();

            void OnTravel(string path) {
                var md5 = AssetProcessorUtils.CalculateAssetHash(path);
                if (!force && hashData.ContainsKey(path) && hashData[path] == md5) {
                    return;
                }

                var importer = AssetImporter.GetAtPath(path);
                if (ProcessImport(importer)) {
                    Debug.Log("Asset re-import: " + path);
                    updated.Add(path);
                }

                hashData[path] = md5;
            }

            var files = _filter.GetFiles();
            if (files.Length <= 0) {
                Debug.Log("Assets list empty, nothing to do.");
                yield break;
            }

            var index = 0f;
            try {
                foreach (var path in files) {
                    var progress = ++index / files.Length;
                    _importProgress = progress / 2;
                    EditorUtility.DisplayProgressBar("Importing", path, progress);
                    OnTravel(path);

                    if (index % 5 == 0) { // Wait for 1 frame to render GUI every 5 frames.
                        yield return null;
                    }
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            if (updated.Count <= 0) {
                _importProgress = 1f;
                Debug.Log("Reimport finished, no updated.");
                SaveHashData(hashData);
                yield break;
            }

            index = 0f;
            foreach (var path in updated) {
                var progress = ++index / updated.Count;
                _importProgress = 0.5f + progress / 2;
                EditorUtility.DisplayProgressBar("Refreshing", path, progress);
                AssetDatabase.ImportAsset(path);
                yield return null;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            foreach (var path in updated) {
                EditorUtility.DisplayProgressBar("Updating hash", path, ++index / updated.Count);
                var md5 = AssetProcessorUtils.CalculateAssetHash(path);
                hashData[path] = md5;
            }

            EditorUtility.ClearProgressBar();
            SaveHashData(hashData);

            Resources.UnloadUnusedAssets();
            GC.Collect();

            Debug.Log("Reimport finished.");
        }

        private AssetHashData GetOrCreateAssetHashData() {
            var path = GetHashCacheFilePath();
            var ret = AssetDatabase.LoadAssetAtPath<AssetHashData>(path);
            return ret ? ret : CreateInstance<AssetHashData>();
        }

        private void SaveHashData(AssetHashData hashData, bool removeNonExist = true) {
            if (null == hashData) {
                return;
            }

            if (removeNonExist) {
                var toRemove = hashData
                    .Select(kv => kv.Key)
                    .Where(filePath => !File.Exists(filePath))
                    .ToList();
                toRemove.ForEach(v => hashData.Remove(v));
            }

            var path = GetHashCacheFilePath();
            var dirName = Path.GetDirectoryName(path);
            if (null != dirName && !Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }

            if (!File.Exists(path)) {
                AssetDatabase.CreateAsset(hashData, path);
            }
            else {
                hashData.ForceSaveAsset();
            }

            Debug.Log("Assets reimport hash data saved: " + path);
        }

        private string GetHashCacheFilePath() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImportConfig>();
            if (!config) {
                return Application.dataPath;
            }

            return $"{config.AssetHashCacheDirectory}/{GetType().FullName}.asset";
        }

        internal void ResetDisplay() {
            _importProgress = 0;
            _imported = false;
        }

        internal IEnumerator CoImport() {
            yield return CoImportInternal(false);
        }

        internal IEnumerator CoForceImport() {
            yield return CoImportInternal(true);
        }
    }

    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [HideReferenceObjectPicker]
    public abstract class AssetImporterRuleBase<T> : AssetImporterRuleBase where T: AssetImporter
    {
        protected override bool ProcessImport(AssetImporter assetImporter) {
            if (assetImporter is T importer) {
                return ProcessImport(importer);
            }
            return false;
        }

        protected abstract bool ProcessImport(T assetImporter);
    }
}