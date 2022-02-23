using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Windows.IdMapper
{
    public class AssetIdsMapperWindow : OdinEditorWindow
    {
        [MenuItem(ToolsetConst.ToolsMenuDir + "Asset ID Mapper")]
        private static void OpenSettingWindow() {
            var window = GetWindow<AssetIdsMapperWindow>();
            window.titleContent = new GUIContent("Asset ID Mapper");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(860, 720);
            window.Initialize();
            window.Show();
        }

        #pragma warning disable 0649, 0414

        [ShowInInspector]
        [PropertyOrder(1)]
        [TableList(HideToolbar = true, AlwaysExpanded = true, ShowPaging = false, IsReadOnly = true)]
        [TitleGroup("Asset ID Groups")]
        [HideLabel]
        [ShowIf("@_idGroups.Count > 0")]
        private List<AIMAssetIdGroups> _idGroups = new List<AIMAssetIdGroups>();

        [ShowInInspector]
        [TitleGroup("Asset ID Groups")]
        [ShowIf("@_idGroups.Count <= 0")]
        [DisplayAsString]
        [HideLabel]
        private string _emptyTips1 = "Asset id groups empty, please create it.";

        #pragma warning restore 0649, 0414

        private AIMAssetIdGroups IdGroups => _idGroups[0];

        protected override void Initialize() {
            base.Initialize();

            var obj = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetIdsMapperSO>();
            if (!obj) {
                return;
            }

            var idGroups = new AIMAssetIdGroups(obj);
            idGroups.OnSelect += OnItemSelected;

            _idGroups.Clear();
            _idGroups.Add(idGroups);
        }

        private void OnItemSelected(Object target) {
            _preview = target;
        }

#region Operations

        [TitleGroup("Operation")]
        [ShowInInspector]
        [Button(ButtonSizes.Large)]
        [HideIf("@_idGroups.Count > 0")]
        [PropertyOrder(2)]
        private void CreateAssetIdGroup() {
            var obj = ScriptableObjectUtils.ConfirmCreateScriptableObject<AssetIdsMapperSO>();
            if (!obj) {
                return;
            }

            var idGroups = new AIMAssetIdGroups(obj);
            idGroups.OnSelect += OnItemSelected;

            _idGroups.Clear();
            _idGroups.Add(idGroups);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/1", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddTextureAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<Texture>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/1", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddSpriteAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<Sprite>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/2", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddMaterialAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<Material>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/2", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddPrefabAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<GameObject>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/3", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddAudioClipAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<AudioClip>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/3", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddAnimationClipAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<AnimationClip>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/4", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddShaderAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<Shader>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [HorizontalGroup("Operation/Horizontal/Button/4", 0.5f)]
        [ShowInInspector]
        [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Expanded = false)]
        [LabelWidth(100)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        private void AddDefaultAssetGroup(int groupIndex, string groupName) {
            var group = new AIMAssetGroup<Object>(groupIndex, groupName);
            IdGroups.Append(group);
        }

        [PropertySpace(SpaceBefore = 10)]
        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal")]
        [VerticalGroup("Operation/Horizontal/Button")]
        [ShowInInspector]
        [Button(ButtonSizes.Large)]
        [ShowIf("@_idGroups.Count > 0")]
        [PropertyOrder(3)]
        [LabelText("Save & Export")]
        private void SaveAndExport() {
            IdGroups.Save();
            IdGroups.Export();
        }

        [TitleGroup("Operation")]
        [HorizontalGroup("Operation/Horizontal", Width = 150)]
        [VerticalGroup("Operation/Horizontal/Preview")]
        [ShowInInspector]
        [PropertyOrder(4)]
        [HideLabel]
        [ShowIf("@_idGroups.Count > 0")]
        [PreviewField(140)]
        [ReadOnly]
        private Object _preview;



#endregion

    }
}