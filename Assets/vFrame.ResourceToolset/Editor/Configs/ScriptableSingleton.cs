using System;
using UnityEditorInternal;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Configs
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        protected ScriptableSingleton() {
            if (_instance != null)
                Debug.LogError("ScriptableSingleton already exists. Did you query the singleton in a constructor?");
            else
                _instance = (object) this as T;
        }

        public static T Instance {
            get {
                if (_instance == null)
                    CreateAndLoad();
                return _instance;
            }
        }

        private static void CreateAndLoad() {
            var filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
                InternalEditorUtility.LoadSerializedFileAndForget(filePath);
            if (_instance != null)
                return;
            Debug.LogWarning("ScriptableObject asset load failed: " + filePath);
            CreateInstance<T>().hideFlags = HideFlags.HideAndDontSave;
        }

        private static string GetFilePath() {
            foreach (var customAttribute in typeof(T).GetCustomAttributes(true))
                if (customAttribute is FilePathAttribute attribute)
                    return attribute.Filepath;

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FilePathAttribute : Attribute
    {
        public enum Location
        {
            PreferencesFolder,
            ProjectFolder
        }

        public FilePathAttribute(string relativePath, Location location) {
            if (string.IsNullOrEmpty(relativePath)) {
                Debug.LogError("Invalid relative path! (its null or empty)");
            }
            else {
                if (relativePath[0] == '/')
                    relativePath = relativePath.Substring(1);
                if (location == Location.PreferencesFolder)
                    Filepath = InternalEditorUtility.unityPreferencesFolder + "/" + relativePath;
                else
                    Filepath = relativePath;
            }
        }

        public string Filepath { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AutoCreateDefaultValue : Attribute
    {

    }
}