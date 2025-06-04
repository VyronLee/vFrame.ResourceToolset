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
        [ShowIf("@_rules.Count > 0")]
        [HideLabel]
        private List<AssetImporterRules> _rules = new List<AssetImporterRules>();

        [ShowInInspector]
        [PropertyOrder(2)]
        [ShowIf("@_rules.Count <= 0")]
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
        private void SearchRules() {
            if (null != Rules) {
                Rules.SearchRule();
            }
            ShowNotification("Search finished.");
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
            selector.EnableSingleClickToSelect();
            selector.SelectionConfirmed += CreateRuleInternal;
            selector.ShowInPopup(pos);
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/1")]
        [PropertyOrder(4)]
        private void ImportAll() {
            if (null == Rules) {
                return;
            }

            IEnumerator ImportInternal() {
                yield return Rules.CoImport();
            }
            EditorCoroutineUtility.StartCoroutine(ImportInternal(), this);
        }

        [ShowInInspector]
        [Button(ButtonSizes.Medium)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/1")]
        [PropertyOrder(4)]
        private void ForceImportAll() {
            if (null == Rules) {
                return;
            }

            IEnumerator ImportInternal() {
                yield return Rules.CoForceImport();
            }
            EditorCoroutineUtility.StartCoroutine(ImportInternal(), this);
        }

        [ShowInInspector]
        [Button(ButtonSizes.Large)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/2")]
        [PropertyOrder(8)]
        private void Save() {
            if (null == Rules || Rules.Empty()) {
                ShowNotification("No importer rules to save.");
                return;
            }

            Rules.Save();

            ShowNotification("Asset importer rules saved.");
        }

        #pragma warning restore 414

        // ==========================================================
        // Processor

        private AssetImporterRules Rules => _rules.Count > 0 ? _rules[0] : null;

        protected override void Initialize() {
            var rules = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImporterRules>();
            if (!rules) {
                rules = ScriptableObjectUtils.CreateScriptableObjectAtPath<AssetImporterRules>(
                    ToolsetConst.DefaultAssetImporterRulesFilePath);
            }
            rules.Initialize();

            _rules.Clear();
            _rules.Add(rules);
        }

        private void CreateRuleInternal(IEnumerable<Type> types) {
            var t = types.FirstOrDefault();
            if (null == t) {
                return;
            }

            // Save rules first
            Rules.Save();

            var rule = ScriptableObjectUtils.ConfirmCreateScriptableObject(t) as AssetImporterRuleBase;
            if (!rule) {
                return;
            }
            Rules.Add(rule);
        }

        private void ResetDisplayInternal() {
            if (null == Rules) {
                return;
            }
            Rules.ResetDisplay();
        }
    }
}