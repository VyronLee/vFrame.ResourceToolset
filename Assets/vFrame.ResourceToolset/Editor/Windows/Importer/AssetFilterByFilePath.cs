using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilterByFilePath : AssetFilterBase
    {
        #pragma warning disable 649

        [SerializeField]
        [LabelWidth(200)]
        private string _filePath;

        #pragma warning restore 649

        public override string[] GetFiles() {
            return new[] { _filePath };
        }

        public override bool FilterTest(string path) {
            return _filePath.Equals(path);
        }
    }
}