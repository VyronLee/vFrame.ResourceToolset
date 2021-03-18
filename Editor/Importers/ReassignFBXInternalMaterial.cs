using System.Linq;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Configs;

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
            if (!BuiltinAssetConfig.Instance.AutoReplaceFBXInternalMaterialOnImport) {
                return previousMaterial;
            }
            var material = BuiltinAssetConfig.Instance.FBXInternalMaterialReplacement;
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
            if(!BuiltinAssetConfig.Instance.AutoReplaceFBXInternalMaterialOnImport) {
                return;
            }

            var material = BuiltinAssetConfig.Instance.FBXInternalMaterialReplacement;
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