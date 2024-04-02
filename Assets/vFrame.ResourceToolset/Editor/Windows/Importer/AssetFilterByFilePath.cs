using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilterByFilePath : AssetFilterBase
    {
        #pragma warning disable 649, 414

        [SerializeField]
        [LabelWidth(200)]
        [VerticalGroup("1")]
        [ListDrawerSettings(HideAddButton = true, ShowPaging = false)]
        private List<string> _filePaths;

        [ShowInInspector]
        [HideLabel]
        [DisplayAsString]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2")]
        private string _padding = "";

        [VerticalGroup("1")]
        [HorizontalGroup("1/2", Width = 100)]
        [Button(ButtonSizes.Small)]
        private void AddFile() {
            _filePaths.Add("");
        }

        #pragma warning restore 649

        public override string[] GetFiles() {
            return _filePaths.ToArray();
        }

        public override bool FilterTest(string path) {
            return _filePaths.Contains(path);
        }

        public override string GetSummary() {
            if (_filePaths.Count <= 3)
                return string.Join("\n", _filePaths.ToArray());

            var ret = string.Join("\n", _filePaths.GetRange(0, 3).ToArray());
            ret += "\n...";
            return ret;
        }
    }
}