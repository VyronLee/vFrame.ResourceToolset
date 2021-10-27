using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Configs
{
    public class BuiltinAssetConfig : ResourceToolsetConfig
    {
        #pragma warning disable CS0649

        [SerializeField]
        [FolderPath]
        private string _builtinReplacementMaterialsDir;

        [SerializeField]
        [FolderPath]
        private string _builtinReplacementTextureDir;

        [SerializeField]
        [AssetsOnly]
        private Material _fbxInternalMaterialReplacement;

        [SerializeField]
        private bool _autoReplaceFBXInternalMaterialOnImport;

        #pragma warning restore CS0649

        public string BuiltinReplacementMaterialsDir => _builtinReplacementMaterialsDir;
        public string BuiltinReplacementTextureDir => _builtinReplacementTextureDir;
        public Material FBXInternalMaterialReplacement => _fbxInternalMaterialReplacement;
        public bool AutoReplaceFBXInternalMaterialOnImport => _autoReplaceFBXInternalMaterialOnImport;
    }
}