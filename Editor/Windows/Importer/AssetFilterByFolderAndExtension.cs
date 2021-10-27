using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Exceptions;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilterByFolderAndExtension : AssetFilterBase
    {
        #pragma warning disable 649

        [SerializeField]
        [FolderPath]
        [LabelWidth(120)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/1")]
        private string _folder;

        [SerializeField]
        [Tooltip("Use '|' to split multiple extension.")]
        [LabelWidth(120)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2")]
        private string _extensions;

        [SerializeField]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2", PaddingLeft = 50)]
        [ToggleLeft]
        private bool _includeSubFolders = true;

        [SerializeField]
        [LabelWidth(120)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/3")]
        private string _excludeRegex = string.Empty;

        #pragma warning restore 649

        public override string[] GetFiles() {
            if (string.IsNullOrEmpty(_folder) || string.IsNullOrEmpty(_extensions)) {
                return Array.Empty<string>();
            }

            if (!Directory.Exists(_folder)) {
                return Array.Empty<string>();
            }

            var extArray = _extensions.Split('|');
            foreach (var ext in extArray) {
                if (!ext.StartsWith(".")) {
                    throw new ResourceToolsetException("Invalid file extension format, must start with '.' : " + ext);
                }
            }
            var ret = new HashSet<string>();
            foreach (var ext in extArray) {
                var option = _includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(_folder, $"*{ext}", option);
                foreach (var file in files) {
                    var path = file.Replace("\\", "/");
                    if (!string.IsNullOrEmpty(_excludeRegex) && Regex.IsMatch(path, _excludeRegex)) {
                        continue;
                    }
                    ret.Add(path);
                }
            }
            return ret.ToArray();
        }

        public override bool FilterTest(string path) {
            if (string.IsNullOrEmpty(path)) {
                return false;
            }

            path = path.Replace("\\", "/");
            if (!path.StartsWith(_folder)) {
                return false;
            }

            var ext = Path.GetExtension(path);
            var extArray = _extensions.Split('|');
            if (!extArray.Contains(ext)) {
                return false;
            }

            if (!string.IsNullOrEmpty(_excludeRegex) && Regex.IsMatch(path, _excludeRegex)) {
                return false;
            }
            return true;
        }
    }
}