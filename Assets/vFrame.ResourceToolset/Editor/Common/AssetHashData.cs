using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Common
{
    [Serializable]
    public class AssetHashData : ScriptableObject, ISerializationCallbackReceiver, IEnumerable<KeyValuePair<string, string>>
    {
        [SerializeField] private string[] _keys;
        [SerializeField] private string[] _values;

        private Dictionary<string, string> _rawValue = new Dictionary<string, string>();

        public void Clear() {
            _rawValue.Clear();
        }

        public int Count => _rawValue.Count;

        public void Add(string key, string value) {
            _rawValue.Add(key, value);
        }

        public bool ContainsKey(string key) {
            return _rawValue.ContainsKey(key);
        }

        public bool Remove(string key) {
            return _rawValue.Remove(key);
        }

        public bool TryGetValue(string key, out string value) {
            return _rawValue.TryGetValue(key, out value);
        }

        public string this[string key] {
            get => _rawValue[key];
            set => _rawValue[key] = value;
        }

        public void OnBeforeSerialize() {
            _keys = _rawValue.Keys.ToArray();
            _keys.Sort();

            _values = new string[_keys.Length];
            for (var i = 0; i < _keys.Length; i++) {
                _values[i] = _rawValue[_keys[i]];
            }
        }

        public void OnAfterDeserialize() {
            if (null == _keys || null == _values) {
                return;
            }
            for (var i = 0; i < _keys.Length && i < _values.Length; i++) {
                _rawValue.Add(_keys[i], _values[i]);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return _rawValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}