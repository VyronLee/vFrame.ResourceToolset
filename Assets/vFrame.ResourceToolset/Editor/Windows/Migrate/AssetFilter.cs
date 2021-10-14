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
        private enum FilterMode
        {
            Exclude,
            Include,
        }

        [ShowInInspector]
        [EnumToggleButtons]
        [HorizontalGroup("Mode", Width = 300)]
        [LabelWidth(100)]
        private FilterMode _filterMode = FilterMode.Exclude;

        [ShowInInspector]
        [HideLabel]
        [ShowIf("_filterMode", FilterMode.Exclude)]
        [InlineProperty]
        private ExcludeMode _excludeMode = new ExcludeMode();

        [ShowInInspector]
        [HideLabel]
        [ShowIf("_filterMode", FilterMode.Include)]
        [InlineProperty]
        private IncludeMode _includeMode = new IncludeMode();

        public bool FilterTest(Object obj) {
            switch (_filterMode) {
                case FilterMode.Include:
                    if (_includeMode.IsInclude(obj)) {
                        return true;
                    }
                    break;
                case FilterMode.Exclude:
                    if (!_excludeMode.IsExclude(obj)) {
                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        [Serializable]
        [HideReferenceObjectPicker]
        private class ExcludeMode
        {
            #pragma warning disable 649

            [ShowInInspector]
            [LabelText("Exclude Prefab SubAssets")]
            [HorizontalGroup("1")]
            [ToggleLeft]
            private bool _excludePrefabSubAssets = true;

            [ShowInInspector]
            [LabelText("Exclude Prefab MainAsset")]
            [HorizontalGroup("2")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _excludePrefabMainAsset;

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

            [ShowInInspector]
            [LabelText("Exclude Others")]
            [HorizontalGroup("7")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _excludeOthers;

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

            public bool IsExclude(Object obj) {
                switch (obj) {
                    case SceneAsset sceneAsset:
                        return _excludeSceneAsset;
                    case GameObject gameObject:
                        if (IsPrefabMainAsset(gameObject)) {
                            return _excludePrefabMainAsset;
                        }
                        if (IsPrefabSubAsset(gameObject)) {
                            return _excludePrefabSubAssets;
                        }
                        return false;
                    case Material material:
                        return _excludeMaterial;
                    case Texture texture:
                        return _excludeTexture;
                    case Sprite sprite:
                        return _excludeSprite;
                    case AnimationClip animationClip:
                        return _excludeAnimationClip;
                    case AnimatorController animatorController:
                        return _excludeAnimatorController;
                    case AudioClip audioClip:
                        return _excludeAudioClip;
                    case MonoScript monoScript:
                        return _excludeMonoScript;
                    case ScriptableObject scriptableObject:
                        return _excludeScriptableObject;
                    default:
                        if (IsPrefabSubAsset(obj)) {
                            return _excludePrefabSubAssets;
                        }
                        return _excludeOthers;
                }
            }

        }

        [Serializable]
        [HideReferenceObjectPicker]
        private class IncludeMode
        {
#pragma warning disable 649

            [ShowInInspector]
            [LabelText("Include Prefab SubAssets")]
            [HorizontalGroup("1")]
            [ToggleLeft]
            private bool _includePrefabSubAssets;

            [ShowInInspector]
            [LabelText("Include Prefab MainAsset")]
            [HorizontalGroup("2")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includePrefabMainAsset = true;

            [ShowInInspector]
            [LabelText("Include Scene Asset")]
            [HorizontalGroup("2")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeSceneAsset = true;

            [ShowInInspector]
            [LabelText("Include Material")]
            [HorizontalGroup("3")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeMaterial = true;

            [ShowInInspector]
            [LabelText("Include Texture")]
            [HorizontalGroup("3")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeTexture = true;

            [ShowInInspector]
            [LabelText("Include Sprite")]
            [HorizontalGroup("4")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeSprite = true;

            [ShowInInspector]
            [LabelText("Include Animation Clip")]
            [HorizontalGroup("4")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeAnimationClip = true;

            [ShowInInspector]
            [LabelText("Include Animator Controller")]
            [HorizontalGroup("5")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeAnimatorController = true;

            [ShowInInspector]
            [LabelText("Include Audio Clip")]
            [HorizontalGroup("5")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeAudioClip = true;

            [ShowInInspector]
            [LabelText("Include MonoScript")]
            [HorizontalGroup("6")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeMonoScript = true;

            [ShowInInspector]
            [LabelText("Include Scriptable Object")]
            [HorizontalGroup("6")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeScriptableObject = true;

            [ShowInInspector]
            [LabelText("Include Others")]
            [HorizontalGroup("7")]
            [ToggleLeft]
            [HideIf("Collapsed")]
            private bool _includeOthers;

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
                    if (fieldInfo.Name.StartsWith("_include")) {
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
                    if (fieldInfo.Name.StartsWith("_include")) {
                        fieldInfo.SetValue(this, false);
                    }
                }

                Collapsed = false;
            }

            public bool IsInclude(Object obj) {
                switch (obj) {
                    case SceneAsset sceneAsset:
                        return _includeSceneAsset;
                    case GameObject gameObject:
                        if (IsPrefabMainAsset(gameObject)) {
                            return _includePrefabMainAsset;
                        }
                        if (IsPrefabSubAsset(gameObject)) {
                            return _includePrefabSubAssets;
                        }
                        return false;
                    case Material material:
                        return _includeMaterial;
                    case Texture texture:
                        return _includeTexture;
                    case Sprite sprite:
                        return _includeSprite;
                    case AnimationClip animationClip:
                        return _includeAnimationClip;
                    case AnimatorController animatorController:
                        return _includeAnimatorController;
                    case AudioClip audioClip:
                        return _includeAudioClip;
                    case MonoScript monoScript:
                        return _includeMonoScript;
                    case ScriptableObject scriptableObject:
                        return _includeScriptableObject;
                    default:
                        if (IsPrefabSubAsset(obj)) {
                            return _includePrefabSubAssets;
                        }
                        return _includeOthers;
                }
            }
        }

        private static bool IsPrefabSubAsset(Object obj) {
            var partOfAnyPrefab = PrefabUtility.IsPartOfAnyPrefab(obj);
            if (!partOfAnyPrefab)
                return false;

            if (!(obj is GameObject gameObject))
                return true;

            var outermostRoot = gameObject.transform.root.gameObject;
            if (outermostRoot && outermostRoot != gameObject) {
                return true;
            }
            return false;
        }

        private static bool IsPrefabMainAsset(Object obj) {
            var partOfAnyPrefab = PrefabUtility.IsPartOfAnyPrefab(obj);
            if (!partOfAnyPrefab)
                return false;

            if (!(obj is GameObject gameObject))
                return false;

            var outermostRoot = gameObject.transform.root.gameObject;
            if (outermostRoot && outermostRoot == gameObject) {
                return true;
            }
            return false;
        }
    }
}