using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.Migrate
{
    internal abstract class AssetSelector<T> : OdinSelector<T> where T: Object
    {
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.Selection.SupportsMultiSelect = false;
        }

        internal void RebuildSelectionTree() {
            var items = AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .Where(v => v is T)
                .Where(ApplyFilter);
            SelectionTree.AddRange(items, BuildItemName);
        }

        private string BuildItemName(Object v) {
            var mainAsset = AssetDatabase.IsMainAsset(v);
            var subAsset = AssetDatabase.IsSubAsset(v);
            var name = $"{AssetDatabase.GetAssetPath(v)} [{v.name}]";
            if (mainAsset) {
                name += " [Main]";
            }
            if (subAsset) {
                name += " [Sub]";
            }
            return name;
        }

        [ShowInInspector]
        [PreviewField(100, ObjectFieldAlignment.Center)]
        [ShowIf("HasSelection")]
        [HideLabel]
        private T Preview => GetCurrentSelection().FirstOrDefault();
        private bool HasSelection() => GetCurrentSelection().FirstOrDefault();

        protected virtual bool ApplyFilter(Object val) {
            return true;
        }
    }

    internal static class AssetSelector {
        public static IEnumerator OpenSelector(Object target, Action<IEnumerable<Object>> onSelectionConfirmed, EditorWindow focus) {
            var windowRect = focus.position;
            var position = new Rect {
                x = windowRect.width / 2 - 300,
                y = 50,
                width = 600,
            };

            focus.ShowNotification(new GUIContent("Building Selector, Please Wait.."));

            IEnumerator DelayCallback(IEnumerable<Object> enumerable) {
                yield return null;
                onSelectionConfirmed?.Invoke(enumerable);
            }

            void OnConfirmWrap(IEnumerable<Object> ret) {
                EditorCoroutineUtility.StartCoroutine(DelayCallback(ret), focus);
            }

            switch (target) {
                case GameObject gameObject: {
                    var selector = new GameObjectSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(gameObject);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case SceneAsset sceneAsset: {
                    var selector = new SceneAssetSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(sceneAsset);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case Material material: {
                    var selector = new MaterialSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(material);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case Texture texture: {
                    var selector = new TextureSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(texture);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case Sprite sprite: {
                    var selector = new SpriteSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(sprite);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case AnimationClip animationClip: {
                    var selector = new AnimationClipSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(animationClip);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case AnimatorController animatorController: {
                    var selector = new AnimatorControllerSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(animatorController);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case AudioClip audioClip: {
                    var selector = new AudioClipSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(audioClip);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case MonoScript monoScript: {
                    var selector = new MonoScriptSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(monoScript);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
                case ScriptableObject scriptableObject: {
                    var selector = new ScriptableObjectSelector();
                    selector.SelectionConfirmed += OnConfirmWrap;
                    selector.SetSelection(scriptableObject);
                    selector.ShowInPopup(position);
                    yield return null;
                    selector.RebuildSelectionTree();
                    break;
                }
            }

            focus.RemoveNotification();
        }
    }

    internal class SpriteSelector : AssetSelector<Sprite> {}
    internal class TextureSelector : AssetSelector<Texture> {}
    internal class AudioClipSelector : AssetSelector<AudioClip> {}
    internal class AnimationClipSelector : AssetSelector<AnimationClip> {}
    internal class AnimatorControllerSelector : AssetSelector<AnimatorController> {}
    internal class GameObjectSelector : AssetSelector<GameObject>
    {
        protected override bool ApplyFilter(Object val) {
            return AssetDatabase.IsMainAsset(val);
        }
    }
    internal class SceneAssetSelector : AssetSelector<SceneAsset> {}
    internal class MaterialSelector : AssetSelector<Material> {}
    internal class MonoScriptSelector : AssetSelector<MonoScript> {}
    internal class ScriptableObjectSelector : AssetSelector<ScriptableObject> {}
}