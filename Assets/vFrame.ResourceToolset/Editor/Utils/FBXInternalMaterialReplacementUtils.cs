using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class FBXInternalMaterialReplacementUtils
    {
        public static bool RemoveExternalObject(string path) {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (!importer) {
                return false;
            }

            if (!RemoveExternalObject(importer)) {
                return false;
            }
            importer.SaveAndReimport();
            return true;
        }

        public static bool ReplaceMaterial(string path, Material newMaterial) {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (!importer) {
                return false;
            }
            var ret = ReplaceMaterial(importer, newMaterial);
            importer.SaveAndReimport();
            return ret;
        }

        private static bool RemoveExternalObject(ModelImporter importer) {
            using (var serializedObject = new SerializedObject(importer)) {
                var externalObjects = serializedObject.FindProperty("m_ExternalObjects");
                if (null == externalObjects) {
                    return false;
                }
                externalObjects.ClearArray();

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                return true;
            }
        }

        private static bool ReplaceMaterial(ModelImporter importer, Material newMaterial) {
            var ret = false;
            using (var serializedObject = new SerializedObject(importer)) {
                var externalObjects = serializedObject.FindProperty("m_ExternalObjects");
                var materials = serializedObject.FindProperty("m_Materials");

                for (var i = 0; i < materials.arraySize; ++i) {
                    var id = materials.GetArrayElementAtIndex(i);
                    var name = id.FindPropertyRelative("name").stringValue;
                    var type = id.FindPropertyRelative("type").stringValue;
                    var assembly = id.FindPropertyRelative("assembly").stringValue;

                    SerializedProperty materialProp = null;
                    for (var j = 0; j < externalObjects.arraySize; ++j) {
                        var pair = externalObjects.GetArrayElementAtIndex(j);
                        var externalName = pair.FindPropertyRelative("first.name").stringValue;
                        var externalType = pair.FindPropertyRelative("first.type").stringValue;

                        if (externalName != name || externalType != type)
                            continue;

                        materialProp = pair.FindPropertyRelative("second");
                        break;
                    }

                    if (materialProp != null) {
                        if (materialProp.objectReferenceValue == newMaterial)
                            continue;

                        materialProp.objectReferenceValue = newMaterial;
                        ret = true;
                    }
                    else {
                        var newIndex = externalObjects.arraySize++;
                        var pair = externalObjects.GetArrayElementAtIndex(newIndex);
                        pair.FindPropertyRelative("first.name").stringValue = name;
                        pair.FindPropertyRelative("first.type").stringValue = type;
                        pair.FindPropertyRelative("first.assembly").stringValue = assembly;
                        pair.FindPropertyRelative("second").objectReferenceValue = newMaterial;
                        ret = true;
                    }
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            importer.importMaterials = false;
            return ret;
        }
    }
}