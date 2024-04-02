using System;
using Sirenix.OdinInspector;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Configs
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class ResourceToolsetSettingConfig : ResourceToolsetConfig
    {
        #pragma warning disable CS0649

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [TitleGroup("AnimationOptimizationConfig")]
        [HideReferenceObjectPicker]
        private AnimationOptimizationConfig _animationOptimizationConfig;

        [Button(ButtonSizes.Medium)]
        [TitleGroup("AnimationOptimizationConfig")]
        [ShowIf("@!_animationOptimizationConfig")]
        [PropertySpace(SpaceAfter = 20)]
        private void CreateAnimationOptimizationConfig() {
            _animationOptimizationConfig = ScriptableObjectUtils.ConfirmCreateScriptableObject<AnimationOptimizationConfig>();
        }

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [TitleGroup("BuiltinAssetConfig")]
        [HideReferenceObjectPicker]
        private BuiltinAssetConfig _builtinAssetConfig;

        [Button(ButtonSizes.Medium)]
        [TitleGroup("BuiltinAssetConfig")]
        [ShowIf("@!_builtinAssetConfig")]
        [PropertySpace(SpaceAfter = 20)]
        private void CreateBuiltinAssetConfig() {
            _builtinAssetConfig = ScriptableObjectUtils.ConfirmCreateScriptableObject<BuiltinAssetConfig>();
        }

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [TitleGroup("AssetImportConfig")]
        [HideReferenceObjectPicker]
        private AssetImportConfig _assetImportConfig;

        [Button(ButtonSizes.Medium)]
        [TitleGroup("AssetImportConfig")]
        [ShowIf("@!_assetImportConfig")]
        [PropertySpace(SpaceAfter = 20)]
        private void CreateAssetImportConfig() {
            _assetImportConfig = ScriptableObjectUtils.ConfirmCreateScriptableObject<AssetImportConfig>();
        }

        #pragma warning restore CS0649

        public AnimationOptimizationConfig AnimationOptimizationConfig => _animationOptimizationConfig;
        public BuiltinAssetConfig BuiltinAssetConfig => _builtinAssetConfig;

        public void Save() {
            if (_animationOptimizationConfig) {
                _animationOptimizationConfig.ForceSaveAsset();
            }
            if (_builtinAssetConfig) {
                _builtinAssetConfig.ForceSaveAsset();
            }
            if (_assetImportConfig) {
                _assetImportConfig.ForceSaveAsset();
            }
            this.ForceSaveAsset();
        }
    }
}