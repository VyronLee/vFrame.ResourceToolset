using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilterByFileRegex : AssetFilterBase
    {
        #pragma warning disable 649, 414

        [SerializeField]
        [FolderPath]
        [LabelWidth(120)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/1", width: 300)]
        private string _folder;

        [SerializeField]
        [VerticalGroup("1")]
        [HorizontalGroup("1/1", PaddingLeft = 50)]
        [ToggleLeft]
        private bool _includeSubFolders = true;

        [SerializeField]
        [LabelWidth(120)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/3")]
        private string _includeRegex = string.Empty;

        [SerializeField]
        [LabelWidth(120)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/4")]
        private string _excludeRegex = string.Empty;

        #pragma warning restore 649

        public override string[] GetFiles() {
            if (string.IsNullOrEmpty(_folder) || string.IsNullOrEmpty(_includeRegex)) {
                return Array.Empty<string>();
            }

            if (!Directory.Exists(_folder)) {
                return Array.Empty<string>();
            }

            var ret = new HashSet<string>();
            var option = _includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(_folder, "*.*", option);
            foreach (var file in files) {
                var path = file.Replace("\\", "/");
                if (!Regex.IsMatch(path, _includeRegex)) {
                    continue;
                }
                if (!string.IsNullOrEmpty(_excludeRegex) && Regex.IsMatch(path, _excludeRegex)) {
                    continue;
                }
                ret.Add(path);
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

            if (!Regex.IsMatch(path, _includeRegex)) {
                return false;
            }

            if (!string.IsNullOrEmpty(_excludeRegex) && Regex.IsMatch(path, _excludeRegex)) {
                return false;
            }
            return true;
        }

        public override string GetSummary() {
            return _folder;
        }
    }
}