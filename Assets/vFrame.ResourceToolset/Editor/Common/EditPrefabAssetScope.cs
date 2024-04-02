using System;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Common
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class EditPrefabAssetScope : IDisposable
    {
        private readonly string _assetPath;

        public GameObject PrefabRoot { get; }

        public EditPrefabAssetScope(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) {
                throw new ArgumentException("Asset path cannot be empty.");
            }

            PrefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
            if (!PrefabRoot) {
                throw new Exception("Load prefab at path failed: " + assetPath);
            }

            _assetPath = assetPath;
        }

        public void Dispose() {
            PrefabUtility.SaveAsPrefabAsset(PrefabRoot, _assetPath);
            PrefabUtility.UnloadPrefabContents(PrefabRoot);
        }
    }
}