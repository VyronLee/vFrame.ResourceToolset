using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Common;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetImporterRuleGroup
    {
        [SerializeField]
        [HideLabel]
        [DisplayAsString]
        [VerticalGroup("Rules")]
        private string _ruleName;

        [SerializeField]
        [HideLabel]
        [TableList(HideToolbar = true, AlwaysExpanded = true, ShowPaging = false, DrawScrollView = false, IsReadOnly = true, ShowIndexLabels = true)]
        [VerticalGroup("Rules")]
        private List<AssetImporterRuleBase> _rules;

        public AssetImporterRuleGroup(IEnumerable<AssetImporterRuleBase> rules) {
            _rules = rules.ToList();
            var first = _rules.FirstOrDefault();
            if (!first) {
                return;
            }
            _ruleName = "> " + first.GetType().Name;
        }

        public void ResetDisplay() {
            if (null == _rules) {
                return;
            }
            foreach (var rule in _rules) {
                rule.ResetDisplay();
            }
        }

        public void Save() {
            if (null == _rules) {
                return;
            }

            foreach (var rule in _rules) {
                EditorUtility.SetDirty(rule);
            }
            AssetDatabase.SaveAssets();

            // Save rule hash
            var ruleHash = GetOrCreateRuleHashData();
            foreach (var rule in _rules) {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(rule, out var guid, out long localId)) {
                    continue;
                }
                var path = AssetDatabase.GetAssetPath(rule);
                if (string.IsNullOrEmpty(path)) {
                    continue;
                }
                ruleHash[guid] = AssetProcessorUtils.CalculateAssetHash(path);
            }
            ruleHash.ForceSaveAsset();
        }

        public IEnumerator Import() {
            if (null == _rules) {
                yield break;
            }
            foreach (var rule in _rules) {
                yield return rule.CoImport();
            }
        }

        public IEnumerator ForceImport() {
            if (null == _rules) {
                yield break;
            }
            foreach (var rule in _rules) {
                yield return rule.CoForceImport();
            }
        }

        private static string GetRuleHashFilePath() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImportConfig>();
            if (!config || string.IsNullOrEmpty(config.RuleHashCacheFile)) {
                return ToolsetConst.DefaultRuleCacheFilePath;
            }
            return config.RuleHashCacheFile;
        }

        private static AssetHashData GetOrCreateRuleHashData() {
            var path = GetRuleHashFilePath();
            var asset = AssetDatabase.LoadAssetAtPath<AssetHashData>(path);
            if (!asset) {
                asset = ScriptableObjectUtils.CreateScriptableObjectAtPath<AssetHashData>(path);
            }
            return asset;
        }

    }
}