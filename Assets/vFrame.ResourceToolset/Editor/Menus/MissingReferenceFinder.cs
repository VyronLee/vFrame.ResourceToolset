using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Menus
{
    internal static class MissingReferenceFinder
    {
        private static readonly string[] ManagedAssetExtensions = {
            ".unity",
            ".prefab",
            ".mat",
            ".anim",
            ".controller",
            ".asset"
        };

        [MenuItem(ToolsetConst.AssetsMenuDir + "Missing Reference/Find Missing Reference")]
        private static void FindMissingReference() {
            var missing = new List<string>();
            void Validate(Object obj) {
                if (MissingReferenceValidationUtils.ValidateAsset(obj, out var missingObjects))
                    return;

                var info = AssetDatabase.GetAssetPath(obj);
                info += "\n";
                info = missingObjects.Aggregate(info,
                    (current, missingObject) => current + "\t" + missingObject + "\n");

                missing.Add(info);
            }

            AssetProcessorUtils.TraversalSelectedObjects(ManagedAssetExtensions,
                "Find Missing Reference",
                Validate);

            if (missing.Count <= 0) {
                Debug.Log("Find missing reference finished, nothing wrong detected.");
                return;
            }

            Debug.Log("Find missing reference finished, reference list below are missing: \n"
                      + string.Join("\n", missing.ToArray()));
        }

        [MenuItem(ToolsetConst.AssetsMenuDir + "Missing Reference/Remove Missing Reference")]
        private static void RemoveMissingReference() {
            var missing = new List<string>();
            void Validate(Object obj) {
                if (MissingReferenceValidationUtils.RemoveMissingReference(obj, out var missingObjects))
                    return;

                var info = AssetDatabase.GetAssetPath(obj);
                info += "\n";
                info = missingObjects.Aggregate(info,
                    (current, missingObject) => current + "\t" + missingObject + "\n");

                missing.Add(info);
            }

            AssetProcessorUtils.TraversalSelectedObjects(ManagedAssetExtensions,
                "Remove Missing Reference",
                Validate);

            if (missing.Count <= 0) {
                Debug.Log("Remove missing reference finished, nothing wrong detected.");
                return;
            }

            AssetDatabase.Refresh();

            Debug.Log("Remove missing reference finished, reference list below are missing: \n"
                      + string.Join("\n", missing.ToArray()));
        }
    }
}