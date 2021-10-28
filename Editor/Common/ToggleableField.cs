using System;
using System.Collections;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Common
{
    #pragma warning disable 649

    [Serializable]
    [HideLabel]
    internal abstract class ToggleableField
    {
        protected const string GroupName = "Horizontal";

        [SerializeField]
        [HorizontalGroup(GroupName, Width = 10)]
        [LabelText("@_label")]
        [ToggleLeft]
        private bool _enable;

        public bool Enabled => _enable;

        private string _label;
        protected ToggleableField(string label) {
            _label = label;
        }
    }

    [Serializable]
    [HideLabel]
    internal class ToggleableFieldInt : ToggleableField
    {
        [SerializeField]
        [HideLabel]
        [HorizontalGroup(GroupName)]
        [EnableIf("Enabled")]
        public int _value;

        public int Value {
            get => _value;
            set => _value = value;
        }

        public ToggleableFieldInt(string label, int value = 0) : base(label) {
            _value = value;
        }

        public override string ToString() {
            return _value.ToString();
        }
    }

    [Serializable]
    [HideLabel]
    internal class ToggleableFieldIntDropDown : ToggleableField
    {
        [SerializeField]
        [HideLabel]
        [HorizontalGroup(GroupName)]
        [ValueDropdown("GetDropdownValues")]
        [EnableIf("Enabled")]
        public int _value;

        public int Value {
            get => _value;
            set => _value = value;
        }

        private int[] _values;

        public ToggleableFieldIntDropDown(string label, int[] values) : base(label) {
            _values = values;
        }

        private IEnumerable GetDropdownValues() {
            var ret = new ValueDropdownList<int>();
            foreach (var value in _values) {
                ret.Add(value.ToString(), value);
            }
            return ret;
        }

        public override string ToString() {
            return _value.ToString();
        }
    }

    [Serializable]
    [HideLabel]
    internal class ToggleableFieldString : ToggleableField
    {
        [SerializeField]
        [HideLabel]
        [HorizontalGroup(GroupName)]
        [EnableIf("Enabled")]
        public string _value;

        public string Value => _value;

        public ToggleableFieldString(string label) : base(label) {
        }

        public override string ToString() {
            return _value;
        }
    }

    [Serializable]
    [HideLabel]
    internal class ToggleableFieldBool : ToggleableField
    {
        [SerializeField]
        [HideLabel]
        [HorizontalGroup(GroupName)]
        [EnableIf("Enabled")]
        public bool _value;

        public bool Value => _value;

        public ToggleableFieldBool(string label) : base(label) {
        }

        public override string ToString() {
            return _value.ToString();
        }
    }

    [Serializable]
    [HideLabel]
    internal class ToggleableFieldFloat : ToggleableField
    {
        [SerializeField]
        [HideLabel]
        [HorizontalGroup(GroupName)]
        [EnableIf("Enabled")]
        public float _value;

        public float Value => _value;

        public ToggleableFieldFloat(string label) : base(label) {
        }

        public override string ToString() {
            return _value.ToString(CultureInfo.CurrentCulture);
        }
    }

    [Serializable]
    [HideLabel]
    internal class ToggleableFieldDouble : ToggleableField
    {
        [SerializeField]
        [HideLabel]
        [HorizontalGroup(GroupName)]
        [EnableIf("Enabled")]
        public double _value;

        public double Value => _value;

        public ToggleableFieldDouble(string label) : base(label) {
        }

        public override string ToString() {
            return _value.ToString(CultureInfo.CurrentCulture);
        }
    }

    #pragma warning restore 649
}