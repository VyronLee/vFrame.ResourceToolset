using System;
using System.IO;
using System.Security.Cryptography;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Windows
{
    public abstract class ResourceToolsetWindow : OdinEditorWindow
    {
        protected void ShowNotification(string content) {
            ShowNotification(new GUIContent(content));
        }

        protected static string PathCombine(string path1, params string[] paths) {
            if (null == paths || paths.Length <= 0) {
                return path1;
            }

            var ret = path1;
            foreach (var path in paths) {
                if (!ret.EndsWith("/")) {
                    ret += "/";
                }
                ret += path;
            }
            return ret;
        }

        protected static string CalculateFileMD5(string path) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(path)) {
                    var hash = md5.ComputeHash(stream);
                    var hashValue = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return hashValue;
                }
            }
        }
    }
}