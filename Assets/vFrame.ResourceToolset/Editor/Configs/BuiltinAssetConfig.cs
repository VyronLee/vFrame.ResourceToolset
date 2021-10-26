using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Exceptions;

namespace vFrame.ResourceToolset.Editor.Configs
{
    internal class BuiltinAssetConfig : ResourceToolsetConfig
    {
        #pragma warning disable CS0649

        [SerializeField]
        [AssetsOnly]
        private Object _builtinReplacementMaterialsDir;

        [SerializeField]
        [AssetsOnly]
        private Object _builtinReplacementTextureDir;

        [SerializeField]
        [AssetsOnly]
        private Material _fbxInternalMaterialReplacement;

        [SerializeField]
        private bool _autoReplaceFBXInternalMaterialOnImport;

        #pragma warning restore CS0649

        public string BuiltinReplacementMaterialsDir {
            get {
                if (!_builtinReplacementMaterialsDir) {
                    throw new ResourceToolsetException("Please assign replacement materials directory first.");
                }
                return AssetDatabase.GetAssetPath(_builtinReplacementMaterialsDir);
            }
            set => _builtinReplacementMaterialsDir = AssetDatabase.LoadAssetAtPath<Object>(value);
        }

        public string BuiltinReplacementTextureDir {
            get {
                if (!_builtinReplacementTextureDir) {
                    throw new ResourceToolsetException("Please assign replacement texture directory first.");
                }
                return AssetDatabase.GetAssetPath(_builtinReplacementTextureDir);
            }
            set => _builtinReplacementTextureDir = AssetDatabase.LoadAssetAtPath<Object>(value);
        }

        public Material FBXInternalMaterialReplacement => _fbxInternalMaterialReplacement;

        public bool AutoReplaceFBXInternalMaterialOnImport => _autoReplaceFBXInternalMaterialOnImport;
    }
}