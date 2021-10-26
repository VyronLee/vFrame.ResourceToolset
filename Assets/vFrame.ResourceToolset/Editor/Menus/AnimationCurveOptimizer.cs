using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class AnimationCurveOptimizer
    {
        private static readonly string[] ManagedAssetExtensions = {".anim"};

        [MenuItem(ToolsetConst.AssetsMenuDir + "Animation/Modify Animation Curve Precision")]
        private static void ModifyAnimationCurvePrecision() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<AnimationOptimizationConfig>();
            if (!config) {
                return;
            }

            var precision = config.Precision;

            bool Optimize(Object obj) {
                return obj is AnimationClip clip
                       && AnimationOptimizationUtils.ModifyCurveValuesPrecision(clip, precision);
            }

            AssetProcessorUtils.TravelAndProcessSelectedObjects(ManagedAssetExtensions,
                "Modify Animation Curve Precision",
                Optimize);
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Animation/Remove Animation Scale Curve")]
        private static void RemoveAnimationScaleCurve() {
            bool Optimize(Object obj) {
                return obj is AnimationClip clip
                       && AnimationOptimizationUtils.RemoveScaleCurve(clip);
            }

            AssetProcessorUtils.TravelAndProcessSelectedObjects(ManagedAssetExtensions,
                "Remove Animation Scale Curve",
                Optimize);
        }
    }
}