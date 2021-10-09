using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.Migrate
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class AssetFilter
    {
        #pragma warning disable 649

        [ShowInInspector]
        [LabelText("Exclude Prefab SubAssets")]
        [HorizontalGroup("1")]
        [ToggleLeft]
        private bool _excludePrefabSubAssets = true;

        [ShowInInspector]
        [LabelText("Exclude Prefab")]
        [HorizontalGroup("2")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludePrefab;

        [ShowInInspector]
        [LabelText("Exclude Scene Asset")]
        [HorizontalGroup("2")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeSceneAsset;

        [ShowInInspector]
        [LabelText("Exclude Material")]
        [HorizontalGroup("3")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeMaterial;

        [ShowInInspector]
        [LabelText("Exclude Texture")]
        [HorizontalGroup("3")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeTexture;

        [ShowInInspector]
        [LabelText("Exclude Sprite")]
        [HorizontalGroup("4")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeSprite;

        [ShowInInspector]
        [LabelText("Exclude Animation Clip")]
        [HorizontalGroup("4")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeAnimationClip;

        [ShowInInspector]
        [LabelText("Exclude Animator Controller")]
        [HorizontalGroup("5")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeAnimatorController;

        [ShowInInspector]
        [LabelText("Exclude Audio Clip")]
        [HorizontalGroup("5")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeAudioClip;

        [ShowInInspector]
        [LabelText("Exclude MonoScript")]
        [HorizontalGroup("6")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeMonoScript;

        [ShowInInspector]
        [LabelText("Exclude Scriptable Object")]
        [HorizontalGroup("6")]
        [ToggleLeft]
        [HideIf("Collapsed")]
        private bool _excludeScriptableObject;

        private bool Collapsed { get; set; } = true;
        private string CollapseState => Collapsed ? "▼" : "▲";

        #pragma warning restore 649

        [PropertySpace]
        [Button("$CollapseState")]
        [HorizontalGroup("Button", Width = 30)]
        private void InvertCollapse() {
            Collapsed = !Collapsed;
        }

        [PropertySpace]
        [Button("Select All")]
        [HorizontalGroup("Button", Width = 100)]
        private void SelectAll() {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var fieldInfo in fields) {
                if (fieldInfo.Name.StartsWith("_exclude")) {
                    fieldInfo.SetValue(this, true);
                }
            }
            Collapsed = false;
        }

        [PropertySpace]
        [Button("Unselect All")]
        [HorizontalGroup("Button", Width = 100)]
        private void UnSelectAll() {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var fieldInfo in fields) {
                if (fieldInfo.Name.StartsWith("_exclude")) {
                    fieldInfo.SetValue(this, false);
                }
            }
            Collapsed = false;
        }

        public bool FilterTest(Object obj) {
            if (_excludePrefabSubAssets) {
                var partOfAnyPrefab = PrefabUtility.IsPartOfAnyPrefab(obj);
                if (partOfAnyPrefab){
                    if (obj is GameObject gameObject) {
                        //var outermostRoot = PrefabUtility.FindPrefabRoot(gameObject);
                        var outermostRoot = gameObject.transform.root.gameObject;
                        if (outermostRoot && outermostRoot != gameObject) {
                            return true;
                        }
                    }
                    else {
                        return true;
                    }
                }
            }

            if (_excludePrefab && obj is GameObject) {
                return true;
            }

            if (_excludeSceneAsset && obj is SceneAsset) {
                return true;
            }

            if (_excludeMaterial && obj is Material) {
                return true;
            }

            if (_excludeTexture && obj is Texture) {
                return true;
            }

            if (_excludeSprite && obj is Sprite) {
                return true;
            }

            if (_excludeAnimationClip && obj is AnimationClip) {
                return true;
            }

            if (_excludeAnimatorController && obj is AnimatorController) {
                return true;
            }

            if (_excludeMonoScript && obj is MonoScript) {
                return true;
            }

            if (_excludeScriptableObject && obj is ScriptableObject) {
                return true;
            }

            return false;
        }
    }
}