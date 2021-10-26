using System;
using Sirenix.OdinInspector;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Configs
{
    [Serializable]
    [InlineEditor]
    internal class ResourceToolsetSettingConfig : ResourceToolsetConfig
    {
        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [TitleGroup("AnimationOptimizationConfig")]
        [HideReferenceObjectPicker]
        private AnimationOptimizationConfig _animationOptimizationConfig;

        [Button(ButtonSizes.Medium)]
        [TitleGroup("AnimationOptimizationConfig")]
        [ShowIf("@!_animationOptimizationConfig")]
        private void CreateAnimationOptimizationConfig() {
            _animationOptimizationConfig = ScriptableObjectUtils.ConfirmCreateConfig<AnimationOptimizationConfig>();
        }

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [TitleGroup("BuiltinAssetConfig")]
        [HideReferenceObjectPicker]
        private BuiltinAssetConfig _builtinAssetConfig;

        [Button(ButtonSizes.Medium)]
        [TitleGroup("BuiltinAssetConfig")]
        [ShowIf("@!_builtinAssetConfig")]
        private void CreateBuiltinAssetConfig() {
            _builtinAssetConfig = ScriptableObjectUtils.ConfirmCreateConfig<BuiltinAssetConfig>();
        }

        public AnimationOptimizationConfig AnimationOptimizationConfig => _animationOptimizationConfig;
        public BuiltinAssetConfig BuiltinAssetConfig => _builtinAssetConfig;
    }
}