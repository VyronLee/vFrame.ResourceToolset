using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    internal class AssetImporterWindow : ResourceToolsetWindow
    {
        [MenuItem(ToolsetConst.ToolsMenuDir + "Asset Importer")]
        private static void OpenSettingWindow() {
            var window = GetWindow<AssetImporterWindow>();
            window.titleContent = new GUIContent("Asset Importer");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(860, 720);
            window.Initialize();
            window.Show();
        }


        // ==========================================================
        // Drawer

        #pragma warning disable 414

        [ShowInInspector]
        [PropertyOrder(1)]
        [TableList(HideToolbar = true, AlwaysExpanded = true, ShowPaging = false, IsReadOnly = true)]
        [TitleGroup("Importer Rules")]
        private List<AssetImporterRuleGroup> _groups = new List<AssetImporterRuleGroup>();

        [ShowInInspector]
        [PropertyOrder(2)]
        [ShowIf("@_groups.Count <= 0")]
        [DisplayAsString]
        [TitleGroup("Importer Rules")]
        [HideLabel]
        private string _emptyTips = "WARNING: Cannot find any importer rule, please click the button below to create your own rule!";

        [ShowInInspector]
        [TitleGroup("Importer Rules")]
        [HorizontalGroup("Importer Rules/Refresh")]
        [PropertyOrder(3)]
        [ShowIf("@false")]
        private int _hiddenPadding;

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Importer Rules")]
        [HorizontalGroup("Importer Rules/Refresh", Width = 150)]
        [PropertyOrder(3)]
        private void ResearchRules() {
            ResearchRuleInternal();
            ShowNotification("Research finished.");
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Importer Rules")]
        [HorizontalGroup("Importer Rules/Refresh", Width = 150)]
        [PropertyOrder(3)]
        private void ResetDisplay() {
            ResetDisplayInternal();
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/1")]
        [PropertyOrder(4)]
        private void CreateImporterRule() {
            var pos = new Rect {
                x = position.width / 2 - 300,
                y = 50,
                width = 600,
            };
            var selector = new AssetImporterRuleSelector();
            selector.SelectionConfirmed += CreateRuleInternal;
            selector.ShowInPopup(pos);
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/1")]
        [PropertyOrder(4)]
        private void SaveAllRules() {
            if (null == _groups) {
                return;
            }
            foreach (var group in _groups) {
                group.Save();
            }
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/2")]
        [PropertyOrder(4)]
        private void ImportAll() {
            if (null == _groups) {
                return;
            }

            IEnumerator ImportInternal() {
                foreach (var group in _groups) {
                    yield return group.Import();
                }
            }
            EditorCoroutineUtility.StartCoroutine(ImportInternal(), this);
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/2")]
        [PropertyOrder(4)]
        private void ForceImportAll() {
            if (null == _groups) {
                return;
            }

            IEnumerator ImportInternal() {
                foreach (var group in _groups) {
                    yield return group.ForceImport();
                }
            }
            EditorCoroutineUtility.StartCoroutine(ImportInternal(), this);
        }

        #pragma warning restore 414

        // ==========================================================
        // Processor

        protected override void Initialize() {
            ResearchRuleInternal();
        }

        private void ResearchRuleInternal() {
            var ruleGuids = AssetDatabase.FindAssets($"t:{nameof(AssetImporterRuleBase)}");
            var groups = new Dictionary<Type, List<AssetImporterRuleBase>>();
            foreach (var ruleGuid in ruleGuids) {
                var rulePath = AssetDatabase.GUIDToAssetPath(ruleGuid);
                var rule = AssetDatabase.LoadAssetAtPath<AssetImporterRuleBase>(rulePath);
                var t = rule.GetType();
                if (!groups.TryGetValue(t, out var list)) {
                    list = groups[t] = new List<AssetImporterRuleBase>();
                }
                list.Add(rule);
            }

            _groups = groups.Select(kv => new AssetImporterRuleGroup(kv.Value)).ToList();
        }

        private void CreateRuleInternal(IEnumerable<Type> types) {
            var t = types.FirstOrDefault();
            if (null == t) {
                return;
            }

            if (ScriptableObjectUtils.ConfirmCreateScriptableObject(t)) {
                ResearchRuleInternal();
            }
        }

        private void ResetDisplayInternal() {
            if (null == _groups) {
                return;
            }
            foreach (var group in _groups) {
                group.ResetDisplay();
            }
        }
    }
}