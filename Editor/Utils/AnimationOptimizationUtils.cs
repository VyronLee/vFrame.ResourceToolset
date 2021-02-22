﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class AnimationOptimizationUtils
    {
        private static readonly Dictionary<uint, string> FloatFormat = new Dictionary<uint, string>();

        static AnimationOptimizationUtils() {
            for (var i = 1u; i < 6; i++) {
                FloatFormat.Add(i, "f" + i);
            }
        }

        public static bool RemoveScaleCurve(AnimationClip clip) {
            var curves = AnimationUtility.GetCurveBindings(clip);
            var ret = false;
            foreach (var curveBinding in curves) {
                var name = curveBinding.propertyName.ToLower();
                if (!name.Contains("scale"))
                    continue;

                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                ret = true;
            }

            if (ret) {
                EditorUtility.SetDirty(clip);
            }
            return ret;
        }

        public static bool ModifyCurveValuesPrecision(AnimationClip clip, uint precision) {
            if (!FloatFormat.TryGetValue(precision, out var floatFormat)) {
                Debug.LogError("Precision not supported: " + precision);
            }

            var ret = false;
            var curves = AnimationUtility.GetCurveBindings(clip);
            foreach (var curveBinding in curves) {
                var curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                var keys = curve.keys;
                if (null == keys) {
                    continue;
                }

                for (var i = 0; i < keys.Length; i++) {
                    var key = keys[i];
                    key.value = float.Parse(key.value.ToString(floatFormat));
                    key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                    key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                    keys[i] = key;
                }
                curve.keys = keys;

                AnimationUtility.SetEditorCurve(clip, curveBinding, curve);

                ret = true;
            }

            if (ret) {
                EditorUtility.SetDirty(clip);
            }
            return ret;
        }
    }
}
