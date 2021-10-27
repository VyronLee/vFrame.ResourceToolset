using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Utils
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class ScriptableObjectUtils
    {
        public static T ConfirmCreateScriptableObject<T>() where T: ScriptableObject {
            var typeName = typeof(T).Name;
            var path = EditorUtility.SaveFilePanelInProject("Save", typeName, "asset", $"Create {typeName}");
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            return CreateScriptableObjectAtPath<T>(path);
        }

        public static ScriptableObject ConfirmCreateScriptableObject(Type t) {
            var typeName = t.Name;
            var path = EditorUtility.SaveFilePanelInProject("Save", typeName, "asset", $"Create {typeName}");
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            return CreateScriptableObjectAtPath(t, path);
        }

        public static T CreateScriptableObjectAtPath<T>(string path) where T : ScriptableObject {
            return CreateScriptableObjectAtPath(typeof(T), path) as T;
        }

        public static ScriptableObject CreateScriptableObjectAtPath(Type t, string path) {
            var inst = ScriptableObject.CreateInstance(t);
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