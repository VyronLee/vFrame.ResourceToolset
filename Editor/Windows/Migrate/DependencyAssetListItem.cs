using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.Migrate
{
    [Serializable]
    [HideReferenceObjectPicker]
    [InlineEditor]
    internal class DependencyAssetListItem
    {
        [HideLabel]
        [ShowInInspector]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2", Width = 20)]
        private bool _selected;

        [HideLabel]
        [ShowInInspector]
        [ReadOnly]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2")]
        private Object _asset;

        [ShowInInspector]
        [DisplayAsString]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2", Width = 50)]
        [LabelText("Ref"), LabelWidth(30)]
        private int _referenceCount = 1;

        [Button("Move")]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2", MinWidth = 500), HorizontalGroup("1/2/Operation")]
        private void OnMoveButtonClick() {
            OnMoveClick?.Invoke(this);
        }

        [Button("Duplicate")]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2"), HorizontalGroup("1/2/Operation")]
        private void OnDuplicateButtonClick() {
            OnDuplicateClick?.Invoke(this);
        }

        [Button("Replace")]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2"), HorizontalGroup("1/2/Operation")]
        private void OnReplaceButtonClick() {
            OnReplaceClick?.Invoke(this);
        }

        [Button("Delete")]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2"), HorizontalGroup("1/2/Operation")]
        private void OnDeleteButtonClick() {
            OnDeleteClick?.Invoke(this);
        }

        [Button("$ShowReferenceButtonState")]
        [VerticalGroup("1")]
        [HorizontalGroup("1/2"), HorizontalGroup("1/2/Operation")]
        private void OnInvertShowReferenceButtonClick() {
            _showReference = !_showReference;
        }
        private bool _showReference;
        private string ShowReferenceButtonState() {
            return _showReference ? "Hide Reference" : "Show Reference";
        }

        [OnInspectorGUI]
        private void DrawReference() {
            if (!_showReference || null == _references) {
                return;
            }

            GUILayout.Label("            Reference:");
            foreach (var reference in _references) {
                var path = AssetDatabase.GetAssetPath(reference);
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                if (GUILayout.Button(">", GUILayout.Width(20))) {
                    EditorGUIUtility.PingObject(reference);
                }
                GUILayout.Label($" - Name: {reference.name} Path: {path}");
                GUILayout.EndHorizontal();
            }
        }

        private HashSet<Object> _references = new HashSet<Object>();

        private string _path;
        private string _guid;
        private long _fileId;

        public DependencyAssetListItem(Object asset) {
            _asset = asset;
            _path = AssetDatabase.GetAssetPath(_asset);

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_asset, out _guid, out _fileId)) {
                Debug.LogError("Get file id of asset failed: " + _path);
            }
        }

        public int ReferenceCount {
            set => _referenceCount = value;
            get => _referenceCount;
        }

        public HashSet<Object> References => _references;

        public bool Selected {
            get => _selected;
            set => _selected = value;
        }

        public Object Asset => _asset;
        public string Path => _path;
        public string Guid => _guid;
        public long FileId => _fileId;

        public AssetOperationEvent OnMoveClick { get; private set; } = new AssetOperationEvent();
        public AssetOperationEvent OnDuplicateClick { get; private set; } = new AssetOperationEvent();
        public AssetOperationEvent OnReplaceClick { get; private set; } = new AssetOperationEvent();
        public AssetOperationEvent OnDeleteClick { get; private set; } = new AssetOperationEvent();

        public override string ToString() {
            return $"Asset: {_asset}, Path: {_path}, Guid: {_guid}, FileID: {_fileId}";
        }
    }

    internal class AssetOperationEvent : UnityEvent<DependencyAssetListItem>
    {

    }
}