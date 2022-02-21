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
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    public class AssetImporterRules : ResourceToolsetConfig
    {
        [SerializeField]
        [HideInInspector]
        private List<AssetImporterRuleBase> _rules = new List<AssetImporterRuleBase>();

        [ShowInInspector]
        [HideLabel]
        [TableList(AlwaysExpanded = true, ShowPaging = false, DrawScrollView = false, ShowIndexLabels = true)]
        [ListDrawerSettings(DraggableItems = true, HideAddButton = true, ShowPaging = false, CustomRemoveIndexFunction = "RemoveRuleAtIndex")]
        [InlineProperty]
        private List<CollapsableRule> _ruleItems;

        public IReadOnlyCollection<AssetImporterRuleBase> Rules => _rules;

        internal void Initialize() {
            _ruleItems = _rules.ConvertAll(x => new CollapsableRule(x)).ToList();
        }

        internal void Add(AssetImporterRuleBase rule) {
            _rules.Add(rule);
            _ruleItems.Add(new CollapsableRule(rule));
        }

        public bool Empty() {
            return null == _rules || _rules.Count == 0;
        }

        public IEnumerator CoImport() {
            foreach (var rule in _rules) {
                yield return rule.CoImport();
            }
        }

        public IEnumerator CoForceImport() {
            foreach (var rule in _rules) {
                yield return rule.CoForceImport();
            }
        }

        public void Import() {
            foreach (var rule in _rules) {
                rule.Import();
            }
        }

        public void ForceImport() {
            foreach (var rule in _rules) {
                rule.ForceImport();
            }
        }

        public void Save() {
            if (Empty()) {
                return;
            }

            _rules = _ruleItems.Select(v => v.Value).ToList();

            // Mark all rules dirty
            var index = 0f;
            foreach (var rule in _rules) {
                var rulePath = AssetDatabase.GetAssetPath(rule);
                EditorUtility.DisplayProgressBar("Saving", rulePath, ++index/_rules.Count);
                EditorUtility.SetDirty(rule);
            }
            EditorUtility.ClearProgressBar();

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
            EditorUtility.SetDirty(ruleHash);
            AssetDatabase.SaveAssets();
        }

        public void ResetDisplay() {
            if (Empty()) {
                return;
            }
            _rules.ForEach(v => v.ResetDisplay());
        }

        public void ResearchRule() {
            var ruleGuids = AssetDatabase.FindAssets($"t:{nameof(AssetImporterRuleBase)}");
            var ret = new List<AssetImporterRuleBase>();
            foreach (var ruleGuid in ruleGuids) {
                var rulePath = AssetDatabase.GUIDToAssetPath(ruleGuid);
                var rule = AssetDatabase.LoadAssetAtPath<AssetImporterRuleBase>(rulePath);
                if (!rule) {
                    continue;
                }
                ret.Add(rule);
            }

            foreach (var rule in ret) {
                if (_rules.Contains(rule)) {
                    continue;
                }
                _rules.Add(rule);
            }

            for (var i = _rules.Count - 1; i >= 0; i--) {
                if (!_rules[i]) {
                    _rules.RemoveAt(i);
                }
            }

            _ruleItems = _rules.ConvertAll(x => new CollapsableRule(x)).ToList();
        }

        private void RemoveRuleAtIndex(int index) {
            if (index < _ruleItems.Count) {
                _ruleItems.RemoveAt(index);
            }
        }

        private static AssetHashData GetOrCreateRuleHashData() {
            var path = GetRuleHashFilePath();
            var asset = AssetDatabase.LoadAssetAtPath<AssetHashData>(path);
            if (!asset) {
                asset = ScriptableObjectUtils.CreateScriptableObjectAtPath<AssetHashData>(path);
            }
            return asset;
        }

        private static string GetRuleHashFilePath() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImportConfig>();
            if (!config || string.IsNullOrEmpty(config.RuleHashCacheFile)) {
                return ToolsetConst.DefaultRuleCacheFilePath;
            }
            return config.RuleHashCacheFile;
        }

        [Serializable]
        private class CollapsableRule
        {
            [ShowInInspector]
            [VerticalGroup("RuleSettings")]
            [ShowIf("@_rule != null")]
            [HideReferenceObjectPicker]
            private CollapsableField<AssetImporterRuleBase> _rule;

            [ShowInInspector]
            [VerticalGroup("RuleSettings")]
            [ShowIf("@_rule == null")]
            [HideLabel]
            [DisplayAsString]
            private string _tips = "<Missing>";

            public CollapsableRule(AssetImporterRuleBase rule) {
                if (!rule) {
                    return;
                }
                _rule = new CollapsableField<AssetImporterRuleBase>(rule);
            }

            public AssetImporterRuleBase Value => _rule.Value;
        }
    }
}