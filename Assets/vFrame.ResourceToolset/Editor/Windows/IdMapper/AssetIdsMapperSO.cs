using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.IdMapper
{
    [Serializable]
    public class AssetIdsMapperSO : ScriptableObject
    {
        [SerializeField]
        public List<AssetIdsMapperGroup> Groups = new List<AssetIdsMapperGroup>();

        [SerializeField]
        public string ExportJsonPath;
    }

    [Serializable]
    public class AssetIdsMapperGroup
    {
        [SerializeField]
        public AIMAssetType AssetType;

        [SerializeField]
        public string GroupName;

        [SerializeField]
        public int GroupIndex;

        [SerializeField]
        public List<AssetIdsMapperGroupItem> Assets = new List<AssetIdsMapperGroupItem>();
    }

    [Serializable]
    public class AssetIdsMapperGroupItem
    {
        [SerializeField]
        public string AssetId;

        [SerializeField]
        public Object Asset;
    }

    [Serializable]
    public class AssetIdsMapperSerializable : Dictionary<string, string>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<string> _keys = new List<string>();

        [SerializeField]
        private List<string> _values = new List<string>();

        public void OnBeforeSerialize() {
            _keys = Keys.ToList();
            _keys.Sort();
            _values = _keys.Select(v => this[v]).ToList();
        }

        public void OnAfterDeserialize() {
            if (null == _keys || null == _values) {
                return;
            }
            for (var i = 0; i < _keys.Count && i < _values.Count; i++) {
                Add(_keys[i], _values[i]);
            }
        }
    }

    public enum AIMAssetType
    {
        Default = 0,
        Texture = 1,
        Sprite = 2,
        Material = 3,
        Prefab = 4,
        AnimationClip = 5,
        AudioClip = 6,
        Shader = 7,
    }

    public static class AIMAssetTypeMap
    {
        public static readonly Dictionary<Type, AIMAssetType> TypeMap = new Dictionary<Type, AIMAssetType>
        {
            {typeof(Texture), AIMAssetType.Texture},
            {typeof(Sprite), AIMAssetType.Sprite},
            {typeof(Material), AIMAssetType.Material},
            {typeof(GameObject), AIMAssetType.Prefab},
            {typeof(AnimationClip), AIMAssetType.AnimationClip},
            {typeof(AudioClip), AIMAssetType.AudioClip},
            {typeof(Shader), AIMAssetType.Shader},
            {typeof(Object), AIMAssetType.Default}, // Place at last
        };
    }
}