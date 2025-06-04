using System;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Windows.Importer;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class AssetImportUtils
    {
        private const string MsgNoAssetImportRuleFound = "No asset importer rule found for path: {0}, falling back to default importer.";

        private static AssetImporterRules _assetImporterRules;

        public static AssetImporterRuleBase[] ImportAsset(string path, bool save = true, bool fallbackOnNoRuleMatch = true) {
            var rules = FindImportRuleOf(path);
            if ((null == rules || rules.Length <= 0) && fallbackOnNoRuleMatch) {
                AssetDatabase.ImportAsset(path); // Fallback to builtin import process.
                Debug.LogWarningFormat(MsgNoAssetImportRuleFound, path);
                return Array.Empty<AssetImporterRuleBase>();
            }
            rules.ForEach(r => r.ApplyTo(path, false));
            if (save) {
                rules.ForEach(r => r.Save());
            }
            return rules;
        }

        private static AssetImporterRuleBase[] FindImportRuleOf(string path) {
            if (!_assetImporterRules) {
                var rules = ScriptableObjectUtils.GetScriptableObjectSingleton<AssetImporterRules>();
                if (null == rules) {
                    return Array.Empty<AssetImporterRuleBase>();
                }
                _assetImporterRules = rules;
            }
            return _assetImporterRules.Rules.Where(rule => rule.FilterTest(path)).ToArray();
        }

        public static TextureImporterFormat RemapTextureFormatType(bool hasAlpha, TextureImporterFormat format) {
            if (hasAlpha) {
                switch (format) {
                    case TextureImporterFormat.RGB16:
                        return TextureImporterFormat.RGBA16;
                    case TextureImporterFormat.RGB24:
                        return TextureImporterFormat.RGBA32;
                    case TextureImporterFormat.PVRTC_RGB2:
                        return TextureImporterFormat.PVRTC_RGBA2;
                    case TextureImporterFormat.PVRTC_RGB4:
                        return TextureImporterFormat.PVRTC_RGBA4;
                    case TextureImporterFormat.ETC_RGB4:
                        return TextureImporterFormat.ETC2_RGBA8;
                    case TextureImporterFormat.ETC2_RGB4:
                        return TextureImporterFormat.ETC2_RGBA8;
                    case TextureImporterFormat.ETC_RGB4Crunched:
                        return TextureImporterFormat.ETC2_RGBA8Crunched;
#if !UNITY_2019_1_OR_NEWER
                    case TextureImporterFormat.ASTC_RGB_4x4:
                        return TextureImporterFormat.ASTC_RGBA_4x4;
                    case TextureImporterFormat.ASTC_RGB_5x5:
                        return TextureImporterFormat.ASTC_RGBA_5x5;
                    case TextureImporterFormat.ASTC_RGB_6x6:
                        return TextureImporterFormat.ASTC_RGBA_6x6;
                    case TextureImporterFormat.ASTC_RGB_8x8:
                        return TextureImporterFormat.ASTC_RGBA_8x8;
                    case TextureImporterFormat.ASTC_RGB_10x10:
                        return TextureImporterFormat.ASTC_RGBA_10x10;
                    case TextureImporterFormat.ASTC_RGB_12x12:
                        return TextureImporterFormat.ASTC_RGBA_12x12;

#endif
                    default:
                        return format;
                }
            }
            else {
                switch (format) {
                    case TextureImporterFormat.ARGB16:
                    case TextureImporterFormat.RGBA32:
                    case TextureImporterFormat.ARGB32:
                    case TextureImporterFormat.RGBA16:
                    case TextureImporterFormat.RGBAFloat:
                        return TextureImporterFormat.RGB16;
                    case TextureImporterFormat.PVRTC_RGBA2:
                        return TextureImporterFormat.PVRTC_RGB2;
                    case TextureImporterFormat.PVRTC_RGBA4:
                        return TextureImporterFormat.PVRTC_RGB4;
                    case TextureImporterFormat.ETC2_RGBA8:
                        return TextureImporterFormat.ETC2_RGB4;
#if !UNITY_2019_1_OR_NEWER
                    case TextureImporterFormat.ASTC_RGBA_4x4:
                        return TextureImporterFormat.ASTC_RGB_4x4;
                    case TextureImporterFormat.ASTC_RGBA_5x5:
                        return TextureImporterFormat.ASTC_RGB_5x5;
                    case TextureImporterFormat.ASTC_RGBA_6x6:
                        return TextureImporterFormat.ASTC_RGB_6x6;
                    case TextureImporterFormat.ASTC_RGBA_8x8:
                        return TextureImporterFormat.ASTC_RGB_8x8;
                    case TextureImporterFormat.ASTC_RGBA_10x10:
                        return TextureImporterFormat.ASTC_RGB_10x10;
                    case TextureImporterFormat.ASTC_RGBA_12x12:
                        return TextureImporterFormat.ASTC_RGB_12x12;
#endif
                    default:
                        return format;
                }
            }
        }

    }
}