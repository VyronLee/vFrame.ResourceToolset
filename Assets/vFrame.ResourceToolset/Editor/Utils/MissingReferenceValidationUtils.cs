using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class MissingReferenceValidationUtils
    {
#region Validate Missing Reference

        public static bool ValidateAsset(Object obj, out List<string> missing) {
            if (obj)
                return TraversalAsset(obj, out missing, ValidateAsset);

            missing = new List<string>();
            return true;
        }

        private static bool ValidateAsset(SerializedObject serializeObject, ref List<string> missing,
            string propertyParent = "") {
            var serializedProperty = serializeObject.GetIterator();
            if (!serializedProperty.Next(true)) {
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
            while (serializedProperty.Next(true));

            return ret;
        }

        public static bool ValidateActiveScene(out List<string> missing) {
            var result = true;
            missing = new List<string>();
            var scene = EditorSceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            foreach (var gameObject in roots) {
                var ret = TraversalAsset(gameObject, ref missing, "", ValidateAsset);
                result &= ret;
            }
            return result;
        }

#endregion

#region Remove Missing Reference

        public static bool RemoveMissingReference(Object obj, out List<string> missing) {
            var ret = TraversalAsset(obj, out missing, RemoveMissingReference);
            if (!ret) {
                AssetProcessorUtils.ForceSaveAsset(obj);
            }
            return ret;
        }

        private static bool RemoveMissingReference(SerializedObject serializeObject, ref List<string> missing,
            string propertyParent = "") {
            var serializedProperty = serializeObject.GetIterator();
            if (!serializedProperty.Next(true)) {
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
            while (serializedProperty.Next(true));

            if (!ret) {
                if (!serializeObject.hasModifiedProperties) {
                    Debug.LogError("No properties modified.");
                }

                if (!serializeObject.ApplyModifiedPropertiesWithoutUndo()) {
                    Debug.LogError("Apply modified properties failed.");
                }
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

        private static bool TraversalAsset(Object obj, out List<string> missing, ReferenceHandler handler) {
            var result = true;
            missing = new List<string>();

            if (!obj) {
                throw new ArgumentNullException(nameof(obj));
            }

            switch (obj) {
                case GameObject gameObject: {
                    var ret = TraversalAsset(gameObject, ref missing, "", handler);
                    result &= ret;
                }
                    break;
                case SceneAsset sceneAsset: {
                    var ret = TraversalAsset(sceneAsset, ref missing, "", handler);
                    result &= ret;
                }
                    break;
                default:
                    using (var serializedObject = new SerializedObject(obj)) {
                        var ret = handler(serializedObject, ref missing);
                        result &= ret;
                    }
                    break;
            }

            return result;
        }

        private static bool TraversalAsset(GameObject obj, ref List<string> missing, string propertyParent,
            ReferenceHandler handler) {

            if (!obj) {
                return true;
            }

            var result = true;

            // Self
            using (var serializedObject = new SerializedObject(obj)) {
                var ret = handler(serializedObject, ref missing);
                result &= ret;
            }

            // Components in this game object
            var components = obj.GetComponents<Component>();
            foreach (var component in components) {
                if (!component) {
                    missing.Add(propertyParent);
                    result = false;
                    continue;
                }
                using (var serializedObject = new SerializedObject(component)) {
                    var ret = handler(serializedObject, ref missing, $"{propertyParent}");
                    result &= ret;
                }
            }

            // Children game objects
            for (var i = 0; i < obj.transform.childCount; ++i) {
                var child = obj.transform.GetChild(i).gameObject;
                var ret = TraversalAsset(child, ref missing, $"{propertyParent}./{child.name}", handler);
                result &= ret;
            }

            return result;
        }

        private static bool TraversalAsset(SceneAsset sceneAsset, ref List<string> missing, string propertyParent,
            ReferenceHandler handler) {

            if (!sceneAsset) {
                return true;
            }

            var result = true;

            using (var serializedObject = new SerializedObject(sceneAsset)) {
                var ret = handler(serializedObject, ref missing);
                result &= ret;
            }

            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            var roots = scene.GetRootGameObjects();
            foreach (var gameObject in roots) {
                var ret = TraversalAsset(gameObject, ref missing, $"{propertyParent}./{gameObject.name}", handler);
                result &= ret;
            }

            EditorSceneManager.CloseScene(scene, true);
            return result;
        }
    }
}