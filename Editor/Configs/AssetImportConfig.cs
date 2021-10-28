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

        [SerializeField]
        private string _ruleHashCacheFile;

        #pragma warning restore CS0649

        public string AssetHashCacheDirectory => _assetHashCacheDirectory;
        public string RuleHashCacheFile => _ruleHashCacheFile;
    }
}