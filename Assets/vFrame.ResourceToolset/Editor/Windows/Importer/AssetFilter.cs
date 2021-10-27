using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilter
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
            switch (_filterType) {
                case FilterType.ByFolderAndExtension:
                    return _filterByFolderAndExtension.GetFiles();
                case FilterType.ByFilePath:
                    return _filterByFilePath.GetFiles();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool FilterTest(string path) {
            switch (_filterType) {
                case FilterType.ByFolderAndExtension:
                    return _filterByFolderAndExtension.FilterTest(path);
                case FilterType.ByFilePath:
                    return _filterByFilePath.FilterTest(path);
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