using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

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
    }
}