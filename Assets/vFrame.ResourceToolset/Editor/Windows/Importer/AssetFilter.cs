using System;
using Sirenix.OdinInspector;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Common;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilter : ICollapsableFieldSummary
    {
        #pragma warning disable 649

        [SerializeField]
        [LabelWidth(130)]
        private FilterType _filterType;

        [SerializeField]
        [ShowIf("@_filterType == FilterType.ByFolderAndExtension")]
        [HideLabel]
        [InlineProperty]
        [Indent]
        private AssetFilterByFolderAndExtension _filterByFolderAndExtension;

        [SerializeField]
        [ShowIf("@_filterType == FilterType.ByFilePath")]
        [HideLabel]
        [InlineProperty]
        [Indent]
        private AssetFilterByFilePath _filterByFilePath;

        #pragma warning restore 649

        public string[] GetFiles() {
            return GetCurrentFilter().GetFiles();
        }

        public bool FilterTest(string path) {
            return GetCurrentFilter().FilterTest(path);
        }

        public string GetSummary() {
            return GetCurrentFilter().GetSummary();
        }

        private AssetFilterBase GetCurrentFilter() {
            switch (_filterType) {
                case FilterType.ByFolderAndExtension:
                    return _filterByFolderAndExtension;
                case FilterType.ByFilePath:
                    return _filterByFilePath;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal enum FilterType
    {
        ByFolderAndExtension,
        ByFilePath,
    }
}