using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class GuidRegenerator
    {
        [MenuItem(ToolsetConst.AssetsMenuDir + "Regenerate Asset GUIDs")]
        private static void RegenerateGuids() {
            if (!EditorUtility.DisplayDialog("GUIDs regeneration",
                "You are going to start the process of GUID regeneration. This may have unexpected results. \n\nMAKE A PROJECT BACKUP BEFORE PROCEEDING!",
                "Regenerate GUIDs", "Cancel"))
            {
                return;
            }

            try {
                AssetDatabase.StartAssetEditing();

                var allAssetsDirectory = Application.dataPath;
                var selectedFiles = GetSelectedObjectPaths();
                GuidRegenerationUtils.RegenerateGuidsOfFiles(selectedFiles, allAssetsDirectory);
            }
            finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        private static IEnumerable<string> GetSelectedObjectPaths() {
            var objects = Selection.objects;
            var paths = new HashSet<string>();
            foreach (var obj in objects) {
                var path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path)) {
                    var files = GuidRegenerationUtils.GetAllFiles(path, GuidRegenerationUtils.DefaultFileExtensions);
                    paths.AddRange(files);
                }
                else {
                    paths.Add(path);
                }
            }
            return paths;
        }
    }
}