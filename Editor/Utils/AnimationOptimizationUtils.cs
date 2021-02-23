using System.Collections.Generic;
using System.Globalization;
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

            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            if (curveBindings.Length > 0) {
                var curves = new AnimationClipCurveData[curveBindings.Length];
                for (var i = 0; i < curves.Length; ++i) {
                    curves[i] = new AnimationClipCurveData(curveBindings[i]) {
                        curve = AnimationUtility.GetEditorCurve(clip, curveBindings[i])
                    };
                }

                foreach (var curveData in curves) {
                    var keyFrames = curveData.curve.keys;
                    var modified = false;
                    for (var i = 0; i < keyFrames.Length; i++) {
                        var key = keyFrames[i];
                        var newValue = float.Parse(key.value.ToString(floatFormat));
                        var newInTangent = float.Parse(key.inTangent.ToString(floatFormat));
                        var newOutTangent = float.Parse(key.outTangent.ToString(floatFormat));

                        if (newValue.ToString() == key.value.ToString()
                            || newInTangent.ToString() == key.inTangent.ToString()
                            || newOutTangent.ToString() == key.outTangent.ToString()
                        ) {
                            continue;
                        }
                        keyFrames[i] = key;
                        modified = true;
                    }

                    if (!modified)
                        continue;

                    curveData.curve.keys = keyFrames;
                    clip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curveData.curve);
                    ret = true;
                }
            }

            if (ret) {
                EditorUtility.SetDirty(clip);
            }
            return ret;
        }
    }
}