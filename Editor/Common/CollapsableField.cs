using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Common
{
    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    public class CollapsableField<T>
    {
        private bool _collapsed = true;

        [ShowInInspector]
        [HorizontalGroup("CollapseGroup", Width = 30)]
        [Button("$GetCollapseButtonLabel", ButtonSizes.Small)]
        [PropertyOrder(1)]
        private void InvertCollapse() {
            _collapsed = !_collapsed;
        }

        [ShowInInspector]
        [HorizontalGroup("CollapseGroup")]
        [PropertyOrder(2)]
        [ShowIf("@!_collapsed")]
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

        public CollapsableField(T value) {
            _value = value;
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
    }

    public interface ICollapsableFieldSummary
    {
        string GetSummary();
    }
}