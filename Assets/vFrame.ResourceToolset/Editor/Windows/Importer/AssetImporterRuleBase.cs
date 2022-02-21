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
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;
using Debug = UnityEngine.Debug;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [HideReferenceObjectPicker]
    public abstract class AssetImporterRuleBase : ResourceToolsetConfig, ICollapsableFieldSummary
    {
        #region Public API

        public bool FilterTest(string path) {
            return _filter?.FilterTest(path) ?? false;
        }

        public void ApplyTo(string path, bool save = true) {
            ApplyToInternal(path, save);
        }

        public void Import() {
            CoImportInternal(false).RunAndWait();
        }

        public void ForceImport() {
            CoImportInternal(true).RunAndWait();
        }

        public void Save() {
            SaveInternal();
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
        [LabelWidth(130)]
        private string _comment;

        [SerializeField]
        [VerticalGroup(GroupName)]
        [InlineProperty]
        [HideLabel]
        private AssetFilter _filter = new AssetFilter();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [ToggleLeft]
        private bool _ignoreRuleHashOnImport;

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
        private AssetHashData _assetHashData;
        private string _ruleHashCacheFilePath;
        private readonly List<string> _dirtyFiles = new List<string>();

#pragma warning restore 414

        private void ApplyToInternal(string path, bool save) {
            var hashData = GetOrCreateAssetHashData();
            var importer = AssetImporter.GetAtPath(path);

            var ret = ProcessImport(importer);
            if (!ret) {
                return;
            }

            var md5 = AssetProcessorUtils.CalculateAssetHash(path);
            hashData[path] = md5;

            if (!save) {
                _dirtyFiles.Add(path);
                return;
            }

            importer.SaveAndReimport();
            Debug.Log("Asset re-imported: " + path);

            SaveAssetHashData(hashData);
        }

        private void SaveInternal() {
            var index = 0f;
            try {
                foreach (var dirtyFile in _dirtyFiles) {
                    EditorUtility.DisplayProgressBar("Saving", dirtyFile, ++index/_dirtyFiles.Count);
                    var importer = AssetImporter.GetAtPath(dirtyFile);
                    if (!importer)
                        continue;
                    importer.SaveAndReimport();
                    Debug.Log("Asset re-imported: " + dirtyFile);
                }
                _dirtyFiles.Clear();
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            var hashData = GetOrCreateAssetHashData();
            SaveAssetHashData(hashData);
        }

        private IEnumerator CoImportInternal(bool force) {
            var hashData = GetOrCreateAssetHashData();
            var ruleHash = GetRuleHash();
            var updated = new List<string>();

            void OnTraversal(string path) {
                var assetHash = AssetProcessorUtils.CalculateAssetHash(path);
                // Validate hash changed?
                if (!force && hashData.ContainsKey(path)) {
                    var hash = hashData[path];
                    var hashAry = hash.Split(',');
                    if (hashAry.Length >= 2) {
                        var cacheRuleHash = hashAry[0];
                        var cacheAssetHash = hashAry[1];
                        if (_ignoreRuleHashOnImport) {
                            if (cacheAssetHash == assetHash) {
                                return;
                            }
                        }
                        else {
                            if (cacheRuleHash == ruleHash && cacheAssetHash == assetHash) {
                                return;
                            }
                        }
                    }
                }

                var importer = AssetImporter.GetAtPath(path);
                if (ProcessImport(importer)) {
                    Debug.Log("Asset re-import: " + path);
                    updated.Add(path);
                }

                hashData[path] = assetHash;
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
                    OnTraversal(path);
                    yield return null;
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            if (updated.Count <= 0) {
                _importProgress = 1f;
                Debug.Log("Reimport finished, no updated.");
                SaveAssetHashData(hashData);
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
                hashData[path] = $"{ruleHash},{md5}";
            }

            EditorUtility.ClearProgressBar();
            SaveAssetHashData(hashData);

            Resources.UnloadUnusedAssets();
            GC.Collect();

            Debug.Log("Reimport finished.");
        }

        private AssetHashData GetOrCreateAssetHashData() {
            var path = GetHashCacheFilePath();
            _assetHashData = _assetHashData ? _assetHashData : AssetDatabase.LoadAssetAtPath<AssetHashData>(path);
            _assetHashData = _assetHashData ? _assetHashData : CreateInstance<AssetHashData>();
            return _assetHashData;
        }

        private static string GetRuleHashFilePath() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImportConfig>();
            if (!config || string.IsNullOrEmpty(config.RuleHashCacheFile)) {
                return ToolsetConst.DefaultRuleCacheFilePath;
            }
            return config.RuleHashCacheFile;
        }

        private string GetRuleHash() {
            var hashFilePath = GetRuleHashFilePath();
            if (string.IsNullOrEmpty(hashFilePath)) {
                return "";
            }
            var data = AssetDatabase.LoadAssetAtPath<AssetHashData>(hashFilePath);
            if (!data) {
                return "";
            }
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out var guid, out long localId)) {
                return "";
            }
            if (!data.TryGetValue(guid, out var hash)) {
                return "";
            }
            return hash;
        }

        private void SaveAssetHashData(AssetHashData hashData, bool removeNonExist = true) {
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
            try {
                var dirName = Path.GetDirectoryName(path);
                if (null != dirName && !Directory.Exists(dirName)) {
                    Directory.CreateDirectory(dirName);
                }
            }
            catch (ArgumentException e) {
                Debug.LogError(path);
                throw;
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
            if (!string.IsNullOrEmpty(_ruleHashCacheFilePath)) {
                return _ruleHashCacheFilePath;
            }
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImportConfig>();
            if (!config) {
                return _ruleHashCacheFilePath = $"{Application.dataPath}/{GetType().FullName}.asset";
            }
            return _ruleHashCacheFilePath = $"{config.AssetHashCacheDirectory}/{GetType().FullName}.asset";
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

        public string GetSummary() {
            if (!string.IsNullOrEmpty(_comment)) {
                return _comment;
            }
            var ret = _filter.GetSummary();
            if (!string.IsNullOrEmpty(ret)) {
                return ret;
            }
            return string.Empty;
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