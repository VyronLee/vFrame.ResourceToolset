using System;

namespace vFrame.ResourceToolset.Editor.Odin
{
    public class ListItemSelectorAttribute : Attribute
    {
        public string SetSelectedMethod;

        public ListItemSelectorAttribute() {

        }

        public ListItemSelectorAttribute(string setSelectedMethod)
        {
            SetSelectedMethod = setSelectedMethod;
        }
    }
}