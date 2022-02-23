using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Common;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Odin;
using vFrame.ResourceToolset.Editor.Utils;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.IdMapper
{
    [Serializable]
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    internal class AIMAssetIdGroups
    {
        #pragma warning disable 0649, 0414

        [ShowInInspector]
        [VerticalGroup("Asset Groups")]
        [LabelText("Mapper Scriptable Object")]
        [ReadOnly]
        private AssetIdsMapperSO _aimSO;

        [ShowInInspector]
        [VerticalGroup("Asset Groups")]
        [Indent]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        private string _exportJsonPath;

        [ShowInInspector]
        [VerticalGroup("Asset Groups")]
        [LabelText("Custom Groups")]
        [ListDrawerSettings(Expanded = true, HideAddButton = true, ShowPaging = false, CustomRemoveIndexFunction = "RemoveElementAtIndex")]
        [HideIf("@AssetGroups.Count <= 0")]
        private List<CollapsableField<AIMAssetGroup>> _assetGroups = new List<CollapsableField<AIMAssetGroup>>();

        [ShowInInspector]
        [VerticalGroup("Asset Groups")]
        [ShowIf("@AssetGroups.Count <= 0")]
        [DisplayAsString]
        [HideLabel]
        private string _emptyTips1 = "Asset groups empty.";

        #pragma warning restore 0649, 0414

        public IReadOnlyCollection<AIMAssetGroup> AssetGroups => _assetGroups.Select(v => v.Value).ToList();

        public event Action<Object> OnSelect;

        private void RemoveElementAtIndex(int index) {
            if (EditorUtility.DisplayDialog("Warning", "Are you sure to remove this asset group?", "Yes", "No")) {
                _assetGroups.RemoveAt(index);
            }
        }

        public AIMAssetIdGroups(AssetIdsMapperSO aimSO) {
            _aimSO = aimSO;
            Initialize();
        }

        public void Append<T>(T group) where T: AIMAssetGroup {
            group.OnSelect += OnGroupItemSelect;

            var ret = new CollapsableField<AIMAssetGroup>(group);
            ret.OnCollapseChanged += OnCollapseChanged;
            ret.Collapsed = false;

            _assetGroups.Add(ret);
        }

        public void Save() {
            _aimSO.Groups.Clear();
            _aimSO.Groups.AddRange(_assetGroups.Select(v => v.Value.ExportData()));
            _aimSO.ExportJsonPath = _exportJsonPath;
            _aimSO.ForceSaveAsset();

            AssetIdsMapperUtils.ShowNotification("Saved.");
        }

        public void Export() {
            if (string.IsNullOrEmpty(_exportJsonPath)) {
                AssetIdsMapperUtils.ShowNotification("Please select a path to export.");
                return;
            }

            AssetIdsMapperUtils.Export(_aimSO, _exportJsonPath);
        }

        private void Initialize() {
            _assetGroups.Clear();
            _assetGroups.AddRange(_aimSO.Groups.Select(CreateGroupFromData));
        }

        private CollapsableField<AIMAssetGroup> CreateGroupFromData(AssetIdsMapperGroup data) {
            var genericType = (from kv in AIMAssetTypeMap.TypeMap where kv.Value == data.AssetType select kv.Key).FirstOrDefault();
            if (genericType == null) {
                throw new Exception($"Can't find generic type for {data.AssetType}");
            }

            var certainType = typeof(AIMAssetGroup<>).MakeGenericType(genericType);
            if (!(Activator.CreateInstance(certainType, data.GroupIndex, data.GroupName) is AIMAssetGroup group)) {
                throw new Exception($"Can't create instance for {certainType}");
            }

            group.ImportData(data);
            group.OnSelect += OnGroupItemSelect;

            var ret = new CollapsableField<AIMAssetGroup>(group);
            ret.OnCollapseChanged += OnCollapseChanged;
            return ret;
        }

        private void OnCollapseChanged(CollapsableField field, bool value) {
            if (value) {
                return;
            }

            foreach (var assetGroup in _assetGroups.Where(assetGroup => assetGroup != field)) {
                assetGroup.Collapsed = true;
            }
        }

        private void OnGroupItemSelect(Object target) {
            OnSelect?.Invoke(target);
        }
    }

    [Serializable]
    internal abstract class AIMAssetGroup
    {
        public abstract AssetIdsMapperGroup ExportData();
        public abstract void ImportData(AssetIdsMapperGroup data);
        public abstract event Action<Object> OnSelect;
    }


    [Serializable]
    [HideReferenceObjectPicker]
    internal class AIMAssetGroup<T> : AIMAssetGroup where T: Object
    {
        #pragma warning disable 0649, 0414

        private const string RootGroupName = "Asset List";

        [Title("$_name")]
        [ShowInInspector]
        [PropertyOrder(11)]
        [VerticalGroup(RootGroupName)]
        [HorizontalGroup(RootGroupName + "/1")]
        [ListItemSelector(SetSelectedMethod = "SetSelectedAsset")]
        [HideIf("@_assets.Count <= 0")]
        [ListDrawerSettings(Expanded = true, HideAddButton = true, NumberOfItemsPerPage = 20)]
        [PropertySpace(SpaceAfter = 10)]
        private List<AIMAssetData<T>> _assets = new List<AIMAssetData<T>>();

        [Title("$_name")]
        [ShowInInspector]
        [PropertyOrder(11)]
        [VerticalGroup(RootGroupName)]
        [HideIf("@_assets.Count > 0")]
        [DisplayAsString]
        [HideLabel]
        private string _emptyTips = "Assets list empty.";

        [ShowInInspector]
        [VerticalGroup(RootGroupName)]
        [HorizontalGroup(RootGroupName + "/2")]
        [LabelWidth(100)]
        [PropertyOrder(12)]
        private bool _advance;

        [ShowInInspector]
        [Button(ButtonSizes.Small)]
        [VerticalGroup(RootGroupName)]
        [HorizontalGroup(RootGroupName + "/2", 80)]
        [PropertyOrder(12)]
        public void AddAsset() {
            var data = new AIMAssetData<T>(NextId);
            Assets.Add(data);
        }

        [ShowInInspector]
        [Button(ButtonSizes.Small)]
        [VerticalGroup(RootGroupName)]
        [HorizontalGroup(RootGroupName + "/2", 80)]
        [PropertyOrder(12)]
        public void AddSelected() {
            var objects = AssetIdsMapperUtils.GetSelectObjectsRecursive<T>(WantsSubAssets);
            if (null == objects || objects.Length <= 0) {
                AssetIdsMapperUtils.ShowNotification($"No {typeof(T).Name} selected.");
                return;
            }

            var duplicated = false;
            foreach (var obj in objects) {
                if (!(obj is T t)) {
                    continue;
                }

                if (ContainsAsset(t)) {
                    duplicated = true;
                    Debug.LogWarning("Asset already exists in group: " + AssetDatabase.GetAssetPath(t));
                    continue;
                }

                var data = new AIMAssetData<T>(NextId) {
                    Asset = t
                };
                Assets.Add(data);
            }

            if (duplicated) {
                AssetIdsMapperUtils.ShowNotification("Some assets already exists in group.");
            }
        }

        [ShowInInspector]
        [ShowIf("@_advance")]
        [PropertyOrder(101)]
        [VerticalGroup(RootGroupName)]
        [LabelText("Group Name")]
        [LabelWidth(150)]
        [Indent]
        private string _name;

        [ShowInInspector]
        [ShowIf("@_advance")]
        [PropertyOrder(102)]
        [VerticalGroup(RootGroupName)]
        [LabelText("Group Next Index")]
        [LabelWidth(150)]
        [Indent]
        private int _index;

        public List<AIMAssetData<T>> Assets => _assets;
        public string GroupName => _name;
        public int GroupIndex => _index;
        public override event Action<Object> OnSelect;

        #pragma warning restore 0649, 0414

        protected string NextId => ToolsetConst.AssetIdPrefix + _index++;

        public AIMAssetGroup(int index, string name) {
            _index = index;
            _name = string.IsNullOrEmpty(name) ? typeof(T).Name : name;
        }

        private bool ContainsAsset(T asset) {
            return _assets?.Any(v => v.Asset == asset) ?? false;
        }

        public bool WantsSubAssets {
            get {
                if (typeof(T) == typeof(GameObject)) {
                    return false;
                }
                return true;
            }
        }

        private void SetSelectedAsset(int index) {
            if (index < 0 || index >= _assets.Count) {
                return;
            }
            OnSelect?.Invoke(_assets[index].Asset);
        }

        public override AssetIdsMapperGroup ExportData() {
            var ret = new AssetIdsMapperGroup {
                AssetType = AIMAssetTypeMap.TypeMap[typeof(T)],
                GroupIndex = GroupIndex,
                GroupName = GroupName,
                Assets = Assets.Select(v => v.ExportData()).ToList(),
            };
            return ret;
        }

        public override void ImportData(AssetIdsMapperGroup data) {
            _index = data.GroupIndex;
            _name = data.GroupName;
            _assets = data.Assets.Select(v => {
                var item = new AIMAssetData<T>(v.AssetId);
                item.ImportData(v);
                return item;
            }).ToList();
        }

        public override string ToString() {
            return _name;
        }
    }

    [Serializable]
    [HideReferenceObjectPicker]
    internal class AIMAssetData<T> where T: Object
    {
        [ShowInInspector]
        [DisplayAsString]
        [InlineButton("CopyId", "Copy")]
        [HorizontalGroup(Width = 250)]
        [LabelWidth(50)]
        private string _id;

        [ShowInInspector]
        [HideLabel]
        [ShowIf("@false")]
        [HorizontalGroup(Width = 10)]
        private string _padding;

        [ShowInInspector]
        [AssetsOnly]
        [HorizontalGroup]
        [LabelWidth(50)]
        private T _asset;

        public string Id => _id;

        public T Asset {
            get => _asset;
            set => _asset = value;
        }

        public AIMAssetData(string id) {
            _id = id;
        }

        private void CopyId() {
            GUIUtility.systemCopyBuffer = _id;
            AssetIdsMapperUtils.ShowNotification("Asset ID Copied");
        }

        public AssetIdsMapperGroupItem ExportData() {
            var ret = new AssetIdsMapperGroupItem {
                AssetId = Id,
                Asset = Asset
            };
            return ret;
        }

        public void ImportData(AssetIdsMapperGroupItem data) {
            _id = data.AssetId;
            _asset = data.Asset as T;
        }
    }
}