using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
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
        [ShowInInspector]
        [HideLabel]
        [DisplayAsString]
        [VerticalGroup("Rules")]
        private string _ruleName;

        [ShowInInspector]
        [HideLabel]
        [TableList(HideToolbar = true, AlwaysExpanded = true, ShowPaging = false, DrawScrollView = false, IsReadOnly = true, ShowIndexLabels = true)]
        [VerticalGroup("Rules")]
        private List<CollapsableRule> _rules;

        public AssetImporterRuleGroup(IEnumerable<AssetImporterRuleBase> rules) {
            var rulesAry = rules.ToList();
            var first = rulesAry.FirstOrDefault();
            if (!first) {
                return;
            }
            _ruleName = "> " + first.GetType().Name;

            _rules = rulesAry
                .Select(rule => new CollapsableRule(rule))
                .ToList();
        }

        public void ResetDisplay() {
            if (null == _rules) {
                return;
            }
            foreach (var rule in _rules) {
                rule.Value.ResetDisplay();
            }
        }

        public AssetImporterRuleBase[] Rules => _rules.Select(v => v.Value).ToArray();

        public void Save() {
        }

        public IEnumerator Import() {
            if (null == _rules) {
                yield break;
            }
            foreach (var rule in _rules) {
                yield return rule.Value.CoImport();
            }
        }

        public IEnumerator ForceImport() {
            if (null == _rules) {
                yield break;
            }
            foreach (var rule in _rules) {
                yield return rule.Value.CoForceImport();
            }
        }

        [Serializable]
        private class CollapsableRule
        {
            [ShowInInspector]
            [VerticalGroup("RuleSettings")]
            private CollapsableField<AssetImporterRuleBase> _rule;

            public CollapsableRule(AssetImporterRuleBase rule) {
                _rule = new CollapsableField<AssetImporterRuleBase>(rule);
            }

            public AssetImporterRuleBase Value => _rule.Value;
        }
    }
}