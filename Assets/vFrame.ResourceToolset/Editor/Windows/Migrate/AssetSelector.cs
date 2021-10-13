using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.Migrate
{
    public abstract class AssetSelector<T> : OdinSelector<T> where T: Object
    {
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.Selection.SupportsMultiSelect = false;

            EditorUtility.DisplayProgressBar("Filtering", "", 0);
            var items = AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .Where(v => v is T)
                .Where(ApplyFilter);
            EditorUtility.ClearProgressBar();

            tree.AddRange(items, BuildItemName);
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
        public static void OpenSelector(Object target, Action<IEnumerable<Object>> onSelectionConfirmed) {
            var focus = EditorWindow.focusedWindow.position;
            var position = new Rect {
                x = focus.width / 2 - 300,
                y = 50,
                width = 600,
            };

            switch (target) {
                case GameObject gameObject: {
                    var selector = new GameObjectSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(gameObject);
                    selector.ShowInPopup(position);
                    break;
                }
                case SceneAsset sceneAsset: {
                    var selector = new SceneAssetSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(sceneAsset);
                    selector.ShowInPopup(position);
                    break;
                }
                case Material material: {
                    var selector = new MaterialSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(material);
                    selector.ShowInPopup(position);
                    break;
                }
                case Texture texture: {
                    var selector = new TextureSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(texture);
                    selector.ShowInPopup(position);
                    break;
                }
                case Sprite sprite: {
                    var selector = new SpriteSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(sprite);
                    selector.ShowInPopup(position);
                    break;
                }
                case AnimationClip animationClip: {
                    var selector = new AnimationClipSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(animationClip);
                    selector.ShowInPopup(position);
                    break;
                }
                case AnimatorController animatorController: {
                    var selector = new AnimatorControllerSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(animatorController);
                    selector.ShowInPopup(position);
                    break;
                }
                case AudioClip audioClip: {
                    var selector = new AudioClipSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(audioClip);
                    selector.ShowInPopup(position);
                    break;
                }
                case MonoScript monoScript: {
                    var selector = new MonoScriptSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(monoScript);
                    selector.ShowInPopup(position);
                    break;
                }
                case ScriptableObject scriptableObject: {
                    var selector = new ScriptableObjectSelector();
                    selector.SelectionConfirmed += onSelectionConfirmed;
                    selector.SetSelection(scriptableObject);
                    selector.ShowInPopup(position);
                    break;
                }
            }
        }
    }

    public class SpriteSelector : AssetSelector<Sprite> {}
    public class TextureSelector : AssetSelector<Texture> {}
    public class AudioClipSelector : AssetSelector<AudioClip> {}
    public class AnimationClipSelector : AssetSelector<AnimationClip> {}
    public class AnimatorControllerSelector : AssetSelector<AnimatorController> {}
    public class GameObjectSelector : AssetSelector<GameObject>
    {
        protected override bool ApplyFilter(Object val) {
            return AssetDatabase.IsMainAsset(val);
        }
    }
    public class SceneAssetSelector : AssetSelector<SceneAsset> {}
    public class MaterialSelector : AssetSelector<Material> {}
    public class MonoScriptSelector : AssetSelector<MonoScript> {}
    public class ScriptableObjectSelector : AssetSelector<ScriptableObject> {}
}