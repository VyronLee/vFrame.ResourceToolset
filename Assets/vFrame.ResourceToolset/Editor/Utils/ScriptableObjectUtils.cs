using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class ScriptableObjectUtils
    {
        public static T ConfirmCreateConfig<T>() where T: ScriptableObject {
            var typeName = typeof(T).Name;
            var path = EditorUtility.SaveFilePanelInProject("Save", typeName, "asset", $"Create {typeName}");
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            var inst = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(inst, path);
            return inst;
        }

        public static T GetScriptableObjectSingleton<T>() where T : ScriptableObject {
            var configs = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (null == configs || configs.Length <= 0) {
                return null;
            }

            var configGuid = configs.FirstOrDefault();
            if (string.IsNullOrEmpty(configGuid)) {
                return null;
            }

            var configPath = AssetDatabase.GUIDToAssetPath(configGuid);
            return AssetDatabase.LoadAssetAtPath<T>(configPath);
        }

    }
}