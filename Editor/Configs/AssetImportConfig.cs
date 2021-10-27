using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Configs
{
    public class AssetImportConfig : ResourceToolsetConfig
    {
        #pragma warning disable CS0649

        [SerializeField]
        [FolderPath]
        private string _assetHashCacheDirectory;

        #pragma warning restore CS0649

        public string AssetHashCacheDirectory => _assetHashCacheDirectory;
    }
}