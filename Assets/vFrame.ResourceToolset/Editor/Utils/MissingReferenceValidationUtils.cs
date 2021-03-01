using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class MissingReferenceValidationUtils
    {
        public static bool ValidateAsset(Object obj, out List<string> missing) {
            var ret = true;
            missing = new List<string>();

            switch (obj) {
                case GameObject gameObject:
                    ret &= ValidateAsset(gameObject, ref missing);
                    break;
                case SceneAsset sceneAsset:
                    ret &= ValidateAsset(sceneAsset, ref missing);
                    break;
                default:
                    using (var serializedObject = new SerializedObject(obj)) {
                        ret &= ValidateAsset(serializedObject, ref missing);
                    }

                    break;
            }

            return ret;
        }

        private static bool ValidateAsset(GameObject obj, ref List<string> missing, string propertyParent = "") {
            var ret = true;

            // Self
            using (var serializedObject = new SerializedObject(obj)) {
                ret &= ValidateAsset(serializedObject, ref missing);
            }

            // Components in this game object
            var components = obj.GetComponents<Component>();
            foreach (var component in components) {
                using (var serializedObject = new SerializedObject(component)) {
                    ret &= ValidateAsset(serializedObject, ref missing, $"{propertyParent}");
                }
            }

            // Children game objects
            for (var i = 0; i < obj.transform.childCount; ++i) {
                var child = obj.transform.GetChild(i).gameObject;
                ret &= ValidateAsset(child, ref missing, $"{propertyParent}./{child.name}");
            }

            return ret;
        }

        private static bool ValidateAsset(SceneAsset sceneAsset, ref List<string> missing, string propertyParent = "") {
            var ret = true;

            using (var serializedObject = new SerializedObject(sceneAsset)) {
                ret &= ValidateAsset(serializedObject, ref missing);
            }

            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            var roots = scene.GetRootGameObjects();
            foreach (var gameObject in roots) {
                ret &= ValidateAsset(gameObject, ref missing, $"{propertyParent}./{gameObject.name}");
            }

            EditorSceneManager.CloseScene(scene, true);
            return ret;
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
    }
}