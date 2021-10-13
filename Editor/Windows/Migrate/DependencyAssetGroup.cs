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
        [ListItemSelector]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, Expanded = true)]
        [VerticalGroup("1")]
        private List<DependencyAssetListItem> _assets = new List<DependencyAssetListItem>();

        [PropertySpace(SpaceAfter = 10)]
        [VerticalGroup("1")]
        [HorizontalGroup("1/1", Width = 100)]
        [Button(ButtonSizes.Small)]
        [PropertyOrder(1)]
        private void SelectAll() {
            _assets.ForEach(asset => asset.Selected = true);
        }

        [PropertySpace]
        [VerticalGroup("1")]
        [HorizontalGroup("1/1", Width = 100)]
        [Button(ButtonSizes.Small)]
        [PropertyOrder(2)]
        private void UnselectAll() {
            _assets.ForEach(asset => asset.Selected = false);
        }

        [PropertySpace]
        [ShowInInspector]
        [HideLabel]
        [DisplayAsString]
        [VerticalGroup("1")]
        [HorizontalGroup("1/1")]
        [PropertyOrder(3)]
        private string _groupKey;

        public DependencyAssetGroup(string key = null) {
            _groupKey = string.IsNullOrEmpty(key) ? "<Not Grouped>" : key;
        }

        public List<DependencyAssetListItem> Assets => _assets;
    }
}