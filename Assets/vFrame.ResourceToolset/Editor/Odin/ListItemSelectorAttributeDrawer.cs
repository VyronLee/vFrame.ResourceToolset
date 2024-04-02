using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Odin
{
    [DrawerPriority(0.01)]
    public class ListItemSelectorAttributeDrawer : OdinAttributeDrawer<ListItemSelectorAttribute>
    {
        private static readonly Color SelectedColor = new Color(0.301f, 0.563f, 1f, 0.497f);
        private InspectorProperty _baseMemberProperty;
        private PropertyContext<InspectorProperty> _globalSelectedProperty;
        private bool _isListElement;
        private Action<object, int> _selectedIndexSetter;
        private InspectorProperty _selectedProperty;

        protected override void Initialize() {
            _isListElement = Property.Parent != null &&
                            Property.Parent.ChildResolver is IOrderedCollectionResolver;
            var isList = !_isListElement;
            var listProperty = isList ? Property : Property.Parent;
            _baseMemberProperty = listProperty.FindParent(x => x.Info.PropertyType == PropertyType.Value, true);
            _globalSelectedProperty =
                _baseMemberProperty.Context.GetGlobal("selectedIndex" + _baseMemberProperty.GetHashCode(),
                    (InspectorProperty)null);

            if (isList) {
                var parentType = _baseMemberProperty.ParentValues[0].GetType();
                if (!string.IsNullOrEmpty(Attribute.SetSelectedMethod)) {
                    var methodInfo = parentType.GetMethod(Attribute.SetSelectedMethod, Flags.AllMembers);
                    if (methodInfo != null) {
                        _selectedIndexSetter = EmitUtilities.CreateWeakInstanceMethodCaller<int>(methodInfo);
                    }
                }
            }
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            var t = Event.current.type;

            if (_isListElement) {
                if (t == EventType.Layout) {
                    CallNextDrawer(label);
                }
                else {
                    var rect = GUIHelper.GetCurrentLayoutRect();
                    var isSelected = _globalSelectedProperty.Value == Property;

                    if (t == EventType.Repaint && isSelected)
                        EditorGUI.DrawRect(rect, SelectedColor);
                    else if (t == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        _globalSelectedProperty.Value = Property;

                    CallNextDrawer(label);
                }
            }
            else {
                CallNextDrawer(label);

                if (Event.current.type != EventType.Layout) {
                    var sel = _globalSelectedProperty.Value;

                    // Select
                    if (sel != null && sel != _selectedProperty) {
                        _selectedProperty = sel;
                        Select(_selectedProperty.Index);
                    }
                    // Deselect when destroyed
                    else if (_selectedProperty != null &&
                             _selectedProperty.Index < Property.Children.Count && _selectedProperty !=
                             Property.Children[_selectedProperty.Index]) {
                        var index = -1;
                        Select(index);
                        _selectedProperty = null;
                        _globalSelectedProperty.Value = null;
                    }
                }
            }
        }

        private void Select(int index) {
            GUIHelper.RequestRepaint();
            Property.Tree.DelayAction(() => {
                for (var i = 0; i < _baseMemberProperty.ParentValues.Count; i++)
                    _selectedIndexSetter?.Invoke(_baseMemberProperty.ParentValues[i], index);
            });
        }
    }
}