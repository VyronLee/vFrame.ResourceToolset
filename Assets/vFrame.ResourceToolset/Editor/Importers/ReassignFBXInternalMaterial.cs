using System.Linq;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Importers
{
    public class ReassignFBXInternalMaterial : AssetPostprocessor
    {
        /// <summary>
        /// Reassign SkinnedMeshRenderer material.
        /// </summary>
        /// <param name="previousMaterial"></param>
        /// <param name="renderer"></param>
        /// <returns></returns>
        protected Material OnAssignMaterialModel(Material previousMaterial, Renderer renderer) {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<BuiltinAssetConfig>();
            if (!config || !config.AutoReplaceFBXInternalMaterialOnImport) {
                return previousMaterial;
            }

            var material = config.FBXInternalMaterialReplacement;
            if (material) {
                return material;
            }
            Debug.LogError("FBX internal material must be specified.");
            return previousMaterial;
        }

        /// <summary>
        /// Reassign MeshRenderer material.
        /// </summary>
        /// <param name="go"></param>
        private void OnPostprocessModel(GameObject go) {
            var config = ScriptableObjectUtils.GetScriptableObjectSingleton<BuiltinAssetConfig>();
            if (!config || !config.AutoReplaceFBXInternalMaterialOnImport) {
                return;
            }

            var material = config.FBXInternalMaterialReplacement;
            if (!material) {
                Debug.LogError("FBX internal material must be specified.");
                return;
            }

            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var meshRenderer in meshRenderers) {
                meshRenderer.sharedMaterials =
                    Enumerable.Repeat(material, meshRenderer.sharedMaterials.Length).ToArray();
            }
        }
    }
}