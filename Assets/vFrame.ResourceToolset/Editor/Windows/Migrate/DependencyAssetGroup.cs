using System.Collections.Generic;
using Sirenix.OdinInspector;
using vFrame.ResourceToolset.Editor.Odin;

namespace vFrame.ResourceToolset.Editor.Windows.Migrate
{
    [HideReferenceObjectPicker]
    [InlineEditor]
    internal class DependencyAssetGroup
    {
        [ShowInInspector]
        [HideLabel]
        [DisplayAsString]
        private string _groupKey;

        [ShowInInspector]
        [ListItemSelector]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, Expanded = true)]
        private List<DependencyAssetListItem> _assets = new List<DependencyAssetListItem>();

        public DependencyAssetGroup(string key = null) {
            _groupKey = string.IsNullOrEmpty(key) ? "<Not Grouped>" : key;
        }

        public List<DependencyAssetListItem> Assets => _assets;
    }
}