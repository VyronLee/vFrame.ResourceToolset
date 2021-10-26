using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Exceptions;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class BuiltinAssetsReplacementUtils
    {
        private static bool IsBuiltinExtraResource(Object asset) {
            var path = AssetDatabase.GetAssetPath(asset);
            return path.StartsWith("Resources/unity_builtin_extra");
        }

        private static BuiltinAssetConfig GetConfig() {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<BuiltinAssetConfig>();
            if (!config) {
                throw new ResourceToolsetException("BuiltinAssetConfig does not exist.");
            }
            return config;
        }

        public static Material GetReplacementBuiltinMaterial(string name) {
            var dir = GetConfig().BuiltinReplacementMaterialsDir;
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{dir}/{name}.mat");
            if (material)
                return material;

            throw new BuiltinReplacementAssetNotFoundException("No builtin material replacement found: " + name);
        }

        public static Sprite GetReplacementBuiltinSprite(string name) {
            var dir = GetConfig().BuiltinReplacementTextureDir;
            var path = $"{dir}/{name}.psd";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite) {
                return sprite;
            }
            throw new BuiltinReplacementAssetNotFoundException("No builtin sprite replacement found: " + name);
        }

        public static Texture GetReplacementBuiltinTexture(string name) {
            var dir = GetConfig().BuiltinReplacementTextureDir;
            var path = $"{dir}/{name}.psd";
            var texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (texture) {
                return texture;
            }
            throw new BuiltinReplacementAssetNotFoundException("No builtin texture replacement found: " + name);
        }

        public static bool ReplaceBuiltinAssets(Object obj) {
            var ret = false;
            try {
                switch (obj) {
                    case GameObject gameObject:
                        ret |= ReplaceBuiltinAssets(gameObject);
                        break;
                    case SceneAsset scene:
                        ret |= ReplaceBuiltinAssets(scene);
                        break;
                    case Material material:
                        ret |= ReplaceBuiltinAssets(material);
                        break;
                }
            }
            catch (Exception e) {
                Debug.LogError($"Replace builtin assets failed, object path: {AssetDatabase.GetAssetPath(obj)}, message: {e}");
            }

            return ret;
        }

        private static bool ReplaceBuiltinAssets(GameObject go) {
            if (go.scene.name != null) {
                throw new ArgumentException("Argument must be a prefab asset.");
            }

            if (PrefabUtility.IsPartOfModelPrefab(go) || PrefabUtility.IsPartOfImmutablePrefab(go)) {
                return false;
            }

            var ret = false;
            ret |= ReplaceGameObjectBuiltinMaterials(go);
            ret |= ReplaceGameObjectBuiltinUITextureAndSprite(go);

            if (ret) {
                PrefabUtility.SavePrefabAsset(go);
            }

            return ret;
        }

        private static bool ReplaceBuiltinAssets(SceneAsset sceneAsset) {
            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var prevScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);

            var ret = false;
            ret |= ReplaceSceneBuiltinMaterials(scene);
            ret |= ReplaceSceneBuiltinUISprite(scene);

            SceneManager.SetActiveScene(prevScene);

            if (ret) {
                EditorSceneManager.SaveScene(scene);
            }

            EditorSceneManager.CloseScene(scene, true);

            return ret;
        }

        private static bool ReplaceBuiltinAssets(Material material) {
            if (!material.shader)
                return false;

            var ret = false;

            if (IsBuiltinExtraResource(material.shader)) {
                var newShader = Shader.Find(material.shader.name);
                if (newShader == material.shader) {
                    Debug.Log("Replace builtin shader failed, please import builtin shader assets first: "
                              + material.shader.name);
                }
                else {
                    material.shader = newShader;
                    ret = true;
                }
            }

            var propertyCount = ShaderUtil.GetPropertyCount(material.shader);
            for (var i = 0; i < propertyCount; i++) {
                if (ShaderUtil.GetPropertyType(material.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) {
                    continue;
                }

                var propertyName = ShaderUtil.GetPropertyName(material.shader, i);
                var texture = material.GetTexture(propertyName);
                if (!IsBuiltinExtraResource(texture)) {
                    continue;
                }

                texture = GetReplacementBuiltinTexture(texture.name);
                if (!texture) {
                    continue;
                }

                material.SetTexture(propertyName, texture);
                ret = true;
            }

            if (ret) {
                EditorUtility.SetDirty(material);
            }
            return ret;
        }

        private static bool ReplaceGameObjectBuiltinMaterials(GameObject go) {
            if (!go) {
                return false;
            }

            var success = false;

            // Renderer
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (var p in renderers) {
                if (p.sharedMaterials == null || p.sharedMaterials.Length <= 0)
                    continue;

                var mat = p.sharedMaterials;
                for (var j = 0; j < mat.Length; j++) {
                    var material = mat[j];
                    if (!material) {
                        continue;
                    }

                    if (!IsBuiltinExtraResource(material))
                        continue;

                    var newMaterial = GetReplacementBuiltinMaterial(material.name);
                    if (!newMaterial)
                        continue;

                    mat[j] = newMaterial;
                    success = true;
                }

                if (success)
                    p.sharedMaterials = mat;
            }

            return success;
        }

        private static bool ReplaceSceneBuiltinMaterials(Scene scene) {
            var ret = false;
            ret |= ReplaceSceneSkybox(scene);

            var roots = scene.GetRootGameObjects();
            ret |= roots.Aggregate(false,
                (current, root) => current | ReplaceGameObjectBuiltinMaterials(root));
            return ret;
        }

        private static bool ReplaceSceneSkybox(Scene scene) {
            if (SceneManager.GetActiveScene() != scene) {
                return false;
            }

            var skybox = RenderSettings.skybox;
            if (!skybox)
                return false;

            if (!IsBuiltinExtraResource(skybox)) {
                return false;
            }

            var newMaterial = GetReplacementBuiltinMaterial(skybox.name);
            if (!newMaterial)
                return false;

            RenderSettings.skybox = newMaterial;
            return true;
        }

        public static bool ReplaceSceneBuiltinUISprite(Scene scene) {
            var roots = scene.GetRootGameObjects();
            var ret = roots.Aggregate(false,
                (current, root) => current | ReplaceGameObjectBuiltinUITextureAndSprite(root));
            return ret;
        }

        public static bool ReplaceGameObjectBuiltinUITextureAndSprite(GameObject go) {
            if (!go) {
                return false;
            }

            // Image
            var images = go.GetComponentsInChildren<Image>(true);
            var success = false;
            foreach (var p in images) {
                if (!p.sprite) {
                    continue;
                }

                if (!IsBuiltinExtraResource(p.sprite)) {
                    continue;
                }

                var sprite = GetReplacementBuiltinSprite("UI/Skin/" + p.sprite.name);
                if (!sprite) {
                    continue;
                }

                p.sprite = sprite;

                success = true;
            }

            // RawImage
            var rawImages = go.GetComponentsInChildren<RawImage>(true);
            foreach (var p in rawImages) {
                if (!p.texture) {
                    continue;
                }

                if (!IsBuiltinExtraResource(p.texture)) {
                    continue;
                }

                var texture = GetReplacementBuiltinTexture("UI/Skin/" + p.texture.name);
                if (!texture) {
                    continue;
                }

                p.texture = texture;

                success = true;
            }

            return success;
        }
    }
}