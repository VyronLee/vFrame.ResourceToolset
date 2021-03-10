using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class MissingReferenceValidationUtils
    {
#region Validate Missing Reference

        public static bool ValidateAsset(Object obj, out List<string> missing) {
            return TravelAsset(obj, out missing, ValidateAsset);
        }

        private static bool ValidateAsset(SerializedObject serializeObject, ref List<string> missing,
            string propertyParent = "") {
            var serializedProperty = serializeObject.GetIterator();
            if (!serializedProperty.NextVisible(true)) {
                return true;
            }

            var ret = true;
            do {
                if (serializedProperty.isArray) {
                    for (var i = 0; i < serializedProperty.arraySize; i++) {
                        var element = serializedProperty.GetArrayElementAtIndex(i);
                        if (!IsObjectReferenceMissing(element)) {
                            continue;
                        }

                        missing.Add($"{propertyParent}.{serializedProperty.propertyPath}");
                        ret = false;
                    }
                }
                else {
                    if (!IsObjectReferenceMissing(serializedProperty)) {
                        continue;
                    }

                    missing.Add($"{propertyParent}.{serializedProperty.propertyPath}");
                    ret = false;
                }
            }
            while (serializedProperty.NextVisible(true));

            return ret;
        }

#endregion

#region Remove Missing Reference

        public static bool RemoveMissingReference(Object obj, out List<string> missing) {
            return TravelAsset(obj, out missing, RemoveMissingReference);
        }

        private static bool RemoveMissingReference(SerializedObject serializeObject, ref List<string> missing,
            string propertyParent = "") {
            var serializedProperty = serializeObject.GetIterator();
            if (!serializedProperty.NextVisible(true)) {
                return true;
            }

            var ret = true;
            do {
                if (serializedProperty.isArray) {
                    for (var i = serializedProperty.arraySize - 1; i >= 0; i--) {
                        var element = serializedProperty.GetArrayElementAtIndex(i);
                        if (!IsObjectReferenceMissing(element)) {
                            continue;
                        }
                        serializedProperty.DeleteArrayElementAtIndex(i); // Delete array object reference

                        missing.Add($"{propertyParent}.{serializedProperty.propertyPath}");
                        ret = false;
                    }
                }
                else {
                    if (!IsObjectReferenceMissing(serializedProperty)) {
                        continue;
                    }

                    serializedProperty.objectReferenceValue = null; // Delete object reference
                    missing.Add($"{propertyParent}.{serializedProperty.propertyPath}");
                    ret = false;
                }
            }
            while (serializedProperty.NextVisible(true));

            if (!ret) {
                serializeObject.ApplyModifiedPropertiesWithoutUndo();
            }
            return ret;
        }

#endregion

        private delegate bool ReferenceHandler(SerializedObject serializeObject, ref List<string> missing,
            string propertyParent = "");

        private static bool IsObjectReferenceMissing(SerializedProperty serializedProperty) {
            if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference) {
                return false;
            }

            if (serializedProperty.objectReferenceValue != null) {
                return false;
            }

            if (serializedProperty.objectReferenceInstanceIDValue == 0) {
                return false;
            }

            return true;
        }

        private static bool TravelAsset(Object obj, out List<string> missing, ReferenceHandler handler) {
            var ret = true;
            missing = new List<string>();

            if (!obj) {
                throw new ArgumentNullException(nameof(obj));
            }

            switch (obj) {
                case GameObject gameObject:
                    ret &= TravelAsset(gameObject, ref missing, "", handler);
                    break;
                case SceneAsset sceneAsset:
                    ret &= TravelAsset(sceneAsset, ref missing, "", handler);
                    break;
                default:
                    using (var serializedObject = new SerializedObject(obj)) {
                        ret &= handler(serializedObject, ref missing);
                    }
                    break;
            }

            return ret;
        }

        private static bool TravelAsset(GameObject obj, ref List<string> missing, string propertyParent,
            ReferenceHandler handler) {

            var ret = true;

            // Self
            using (var serializedObject = new SerializedObject(obj)) {
                ret &= handler(serializedObject, ref missing);
            }

            // Components in this game object
            var components = obj.GetComponents<Component>();
            foreach (var component in components) {
                using (var serializedObject = new SerializedObject(component)) {
                    ret &= handler(serializedObject, ref missing, $"{propertyParent}");
                }
            }

            // Children game objects
            for (var i = 0; i < obj.transform.childCount; ++i) {
                var child = obj.transform.GetChild(i).gameObject;
                ret &= TravelAsset(child, ref missing, $"{propertyParent}./{child.name}", handler);
            }

            return ret;
        }

        private static bool TravelAsset(SceneAsset sceneAsset, ref List<string> missing, string propertyParent,
            ReferenceHandler handler) {

            var ret = true;

            using (var serializedObject = new SerializedObject(sceneAsset)) {
                ret &= handler(serializedObject, ref missing);
            }

            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            var roots = scene.GetRootGameObjects();
            foreach (var gameObject in roots) {
                ret &= TravelAsset(gameObject, ref missing, $"{propertyParent}./{gameObject.name}", handler);
            }

            if (!ret) {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            EditorSceneManager.CloseScene(scene, true);
            return ret;
        }
    }
}