using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Windows.Importer;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class AssetImportUtils
    {
        private const string MsgNoAssetImportRuleFound = "No asset importer rule found for path: {0}";

        public static void ImportAsset(string path) {
            var rules = FindImportRuleOf(path);
            if (null == rules || rules.Length <= 0) {
                AssetDatabase.ImportAsset(path); // Fallback to builtin import process.
                Debug.LogWarningFormat(MsgNoAssetImportRuleFound, path);
                return;
            }
            rules.ForEach(r => r.ApplyTo(path));
        }

        private static AssetImporterRuleBase[] FindImportRuleOf(string path) {
            var ruleGuids = AssetDatabase.FindAssets($"t:{nameof(AssetImporterRuleBase)}");
            return ruleGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssetImporterRuleBase>)
                .Where(rule => rule.FilterTest(path))
                .ToArray();
        }
    }
}