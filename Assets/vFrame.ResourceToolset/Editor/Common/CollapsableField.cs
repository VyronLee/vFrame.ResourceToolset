using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Common
{
    [Serializable]
    public abstract class CollapsableField
    {
        public abstract event Action<CollapsableField, bool> OnCollapseChanged;
    }

    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    public class CollapsableField<T> : CollapsableField
    {
        private bool _collapsed;

        [ShowInInspector]
        [HorizontalGroup("CollapseGroup", Width = 30)]
        [Button("$GetCollapseButtonLabel", ButtonSizes.Small)]
        [PropertyOrder(1)]
        private void InvertCollapse() {
            Collapsed = !Collapsed;
        }

        [ShowInInspector]
        [HorizontalGroup("CollapseGroup")]
        [PropertyOrder(2)]
        [ShowIf("@!_collapsed")]
        [InlineProperty]
        [HideLabel]
        private T _value;

        [ShowInInspector]
        [HorizontalGroup("CollapseGroup")]
        [PropertyOrder(2)]
        [ShowIf("@_collapsed")]
        [OnInspectorGUI]
        private void DisplaySummary() {
            GUILayout.BeginVertical();
            GUILayout.Label(GetValueSummary());
            GUILayout.EndVertical();
        }

        public CollapsableField(T value, bool collapsed = true) {
            _value = value;
            _collapsed = collapsed;
        }

        private string GetCollapseButtonLabel() {
            return _collapsed ? "▼" : "▲";
        }

        private string GetValueSummary() {
            if (null == _value) {
                return string.Empty;
            }

            if (_value is ICollapsableFieldSummary summary) {
                return summary.GetSummary();
            }
            return _value.ToString();
        }

        public T Value => _value;

        public bool Collapsed {
            get =>_collapsed;
            set {
                var prev = _collapsed;
                _collapsed = value;

                if (prev == _collapsed) {
                    return;
                }
                OnCollapseChanged?.Invoke(this, _collapsed);
            }
        }

        public override event Action<CollapsableField, bool> OnCollapseChanged;
    }

    public interface ICollapsableFieldSummary
    {
        string GetSummary();
    }
}