using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Common;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Windows.Importer.Rules
{
    public class PresetTextureImporterRule : AssetImporterRuleBase<TextureImporter>
    {
        [ShowInInspector]
        [VerticalGroup(GroupName)]
        [LabelText("Advance")]
        [OnValueChanged("OnAdvanceModeChanged")]
        [Tooltip("Turn on advance mode to see more settings.")]
        [ToggleLeft]
        private bool _advanceMode = false;

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Setting")]
        [HideLabel]
        private PresetTextureImportSetting _importSetting = new PresetTextureImportSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Platform Setting")]
        [LabelText("Auto detect alpha channel to choose RGB or RGBA")]
        [ToggleLeft]
        private bool _autoDetectAlphaChannel = true;

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Platform Setting")]
        [Header("iOS")]
        [HideLabel]
        private PresetTextureImportPlatformSetting _iOSPlatformSetting = new PresetTextureImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Platform Setting")]
        [Header("Android")]
        [HideLabel]
        private PresetTextureImportPlatformSetting _androidPlatformSetting = new PresetTextureImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Platform Setting")]
        [Header("WebGL")]
        [HideLabel]
        private PresetTextureImportPlatformSetting _webGLPlatformSetting = new PresetTextureImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Platform Setting")]
        [Header("Standalone")]
        [HideLabel]
        private PresetTextureImportPlatformSetting _standalonePlatformSetting = new PresetTextureImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Texture Import Platform Setting")]
        [Header("Windows Store Apps")]
        [HideLabel]
        private PresetTextureImportPlatformSetting _wsaPlatformSetting = new PresetTextureImportPlatformSetting();

        private void OnAdvanceModeChanged() {
            _importSetting.Advance = _advanceMode;
            _iOSPlatformSetting.Advance = _advanceMode;
            _androidPlatformSetting.Advance = _advanceMode;
            _webGLPlatformSetting.Advance = _advanceMode;
            _standalonePlatformSetting.Advance = _advanceMode;
            _wsaPlatformSetting.Advance = _advanceMode;
        }


        private bool SetPackingTagValueIfEnable(TFPackingTag toggleField, TextureImporter importer) {
            if (!toggleField.Enabled) {
                return false;
            }

            var newValue = toggleField.Value;
            switch (toggleField.GenerationType) {
                case PackingTagGenerationType.GeneratedByAssetName:
                    var fileName = Path.GetFileNameWithoutExtension(importer.assetPath);
                    var dir = Path.GetDirectoryName(importer.assetPath);
                    newValue = string.IsNullOrEmpty(dir) ? fileName : Path.Combine(dir, fileName);
                    newValue = newValue.Replace("\\", "/").Replace("Assets/", "").Replace("/", "_");
                    break;
                case PackingTagGenerationType.GeneratedByAssetFolder:
                    newValue = Path.GetDirectoryName(importer.assetPath);
                    if (!string.IsNullOrEmpty(newValue)) {
                        newValue = newValue.Replace("\\", "/").Replace("Assets/", "").Replace("/", "_");
                    }
                    break;
            }

            var prevValue = ReflectionUtils.GetPropertyValue(importer, "spritePackingTag") as string;
            if (prevValue == newValue) {
                return false;
            }
            ReflectionUtils.SetPropertyValue(importer, "spritePackingTag", newValue);
            return true;
        }

        private bool UpdateImporterSettings(TextureImporter assetImporter) {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var ret = false;
            if (SetImporterPropertyValueIfEnable(_importSetting.TextureImporterType, assetImporter, "textureType")) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.sRGB, assetImporter, "sRGBTexture")) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.Mipmap, assetImporter, "mipmapEnabled")) {
                ret = true;
            }
            if (_importSetting.Mipmap.Enabled
                && _importSetting.Mipmap.Value
                && SetImporterPropertyValueIfEnable(_importSetting.AnisoLevel, assetImporter, "anisoLevel")) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.Readable, assetImporter, "isReadable")) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.AlphaIsTransparent, assetImporter, "alphaIsTransparency")) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.FilterMode, assetImporter, "filterMode")) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.WrapMode, assetImporter, "wrapMode")) {
                ret = true;
            }
            if (SetPackingTagValueIfEnable(_importSetting.PackingTag, assetImporter)) {
                ret = true;
            }
            return ret;
        }

        private bool UpdatePlatformSettings(PresetTextureImportPlatformSetting inputSettings,
            TextureImporterPlatformSettings outputSettings,
            TextureImporter importer) {

            // ReSharper disable once ReplaceWithSingleAssignment.False
            var ret = false;
            if (SetImporterPropertyValueIfEnable(inputSettings.MaxSize, outputSettings, "maxTextureSize")) {
                ret = true;
            }
            if (inputSettings.TextureFormat.Enabled) {
                var format = inputSettings.TextureFormat.Value;
                if (_autoDetectAlphaChannel) {
                    format = AssetImportUtils.RemapTextureFormatType(importer.DoesSourceTextureHaveAlpha(), format);
                }

                var prevValue = (TextureImporterFormat) ReflectionUtils.GetPropertyValue(outputSettings, "format");
                if (prevValue != format) {
                    ReflectionUtils.SetPropertyValue(outputSettings, "format", format);
                    ret = true;
                }
            }
            return ret;
        }

        protected override bool ProcessImport(TextureImporter assetImporter) {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var ret = false;

            // Update importer settings
            if (UpdateImporterSettings(assetImporter)) {
                ret = true;
            }

            // Update platform settings
            void UpdatePlatformSettingsIfEnabled(PresetTextureImportPlatformSetting inputSettings, string platform) {
                if (inputSettings.Override.Enabled) {
                    if (inputSettings.Override.Value) {
                        var platformSettings = assetImporter.GetPlatformTextureSettings(platform);
                        platformSettings.overridden = true;
                        if (UpdatePlatformSettings(inputSettings, platformSettings, assetImporter)) {
                            assetImporter.SetPlatformTextureSettings(platformSettings);
                            ret = true;
                        }
                    }
                    else {
                        assetImporter.ClearPlatformTextureSettings(platform);
                    }
                }
            }
            UpdatePlatformSettingsIfEnabled(_iOSPlatformSetting, NamedBuildTarget.iOS.TargetName);
            UpdatePlatformSettingsIfEnabled(_androidPlatformSetting, NamedBuildTarget.Android.TargetName);
            UpdatePlatformSettingsIfEnabled(_webGLPlatformSetting, NamedBuildTarget.WebGL.TargetName);
            UpdatePlatformSettingsIfEnabled(_standalonePlatformSetting, NamedBuildTarget.Standalone.TargetName);
            UpdatePlatformSettingsIfEnabled(_wsaPlatformSetting, NamedBuildTarget.WindowsStoreApps.TargetName);

            return ret;
        }

        [Serializable]
        private class PresetTextureImportSetting
        {
            [SerializeField]
            private TFTextureImportType _textureImportType = new TFTextureImportType("Texture Import Type");

            [SerializeField]
            private TFPackingTag _packingTag = new TFPackingTag("Packing Tag");

            [SerializeField]
            [ShowIf("@_advance")]
            private ToggleableFieldBool _sRGB = new ToggleableFieldBool("sRGB");

            [SerializeField]
            private ToggleableFieldBool _mipmap = new ToggleableFieldBool("Mipmap Enable");

            [SerializeField]
            [ShowIf("@_mipmap.Enabled && _mipmap.Value && _advance")]
            [Indent]
            private ToggleableFieldInt _anisoLevel = new ToggleableFieldInt("Aniso Level");

            [SerializeField]
            private ToggleableFieldBool _readable = new ToggleableFieldBool("Read/Write Enable");

            [SerializeField]
            [ShowIf("@_advance")]
            private ToggleableFieldBool _alphaIsTransparent = new ToggleableFieldBool("Alpha Is Transparent");

            [SerializeField]
            [ShowIf("@_advance")]
            private TFFilterMode _filterMode = new TFFilterMode("Filter Mode");

            [SerializeField]
            [ShowIf("@_advance")]
            private TFTextureWrapMode _wrapMode = new TFTextureWrapMode("Wrap Mode");

            public TFTextureImportType TextureImporterType => _textureImportType;
            public TFPackingTag PackingTag => _packingTag;
            public ToggleableFieldBool sRGB => _sRGB;
            public ToggleableFieldBool Mipmap => _mipmap;
            public ToggleableFieldInt AnisoLevel => _anisoLevel;
            public ToggleableFieldBool Readable => _readable;
            public ToggleableFieldBool AlphaIsTransparent => _alphaIsTransparent;
            public TFFilterMode FilterMode => _filterMode;
            public TFTextureWrapMode WrapMode => _wrapMode;

            private bool _advance;

            public bool Advance {
                get => _advance;
                set => _advance = value;
            }
        }

        [Serializable]
        private class PresetTextureImportPlatformSetting
        {
            private static readonly int[] MaxSizeValues = {
                32, 64, 128, 256, 512, 1024, 2048,
            };

            [SerializeField]
            private ToggleableFieldBool _override = new ToggleableFieldBool("Override");

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value")]
            [Indent]
            private ToggleableFieldIntDropDown _maxSize = new ToggleableFieldIntDropDown("Max Size", MaxSizeValues);

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value")]
            [Indent]
            private TFTextureImporterFormat _textureFormat = new TFTextureImporterFormat("Texture Format");

            public ToggleableFieldBool Override => _override;
            public ToggleableFieldIntDropDown MaxSize => _maxSize;
            public TFTextureImporterFormat TextureFormat => _textureFormat;

            private bool _advance;

            public bool Advance {
                get => _advance;
                set => _advance = value;
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFTextureImportType : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            private TextureImporterType _value ;

            #pragma warning restore 649

            public TextureImporterType Value => _value;

            public TFTextureImportType(string label) : base(label) {
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFTextureImporterFormat : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            private TextureImporterFormat _value ;

            #pragma warning restore 649

            public TextureImporterFormat Value => _value;

            public TFTextureImporterFormat(string label) : base(label) {
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFFilterMode : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            private FilterMode _value ;

            #pragma warning restore 649

            public FilterMode Value => _value;

            public TFFilterMode(string label) : base(label) {
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFTextureWrapMode : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            private TextureWrapMode _value ;

            #pragma warning restore 649

            public TextureWrapMode Value => _value;

            public TFTextureWrapMode(string label) : base(label) {
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFPackingTag : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName, width: 200)]
            [EnableIf("Enabled")]
            private PackingTagGenerationType _packingTagGenerationType;

            [SerializeField]
            [LabelText("Fixed Value")]
            [LabelWidth(80)]
            [HorizontalGroup(GroupName)]
            [ShowIf("@Enabled && _packingTagGenerationType == PackingTagGenerationType.Fixed")]
            private string _value ;

            #pragma warning restore 649

            public string Value => _value;
            public PackingTagGenerationType GenerationType => _packingTagGenerationType;

            public TFPackingTag(string label) : base(label)
            {
            }
        }

        private enum PackingTagGenerationType
        {
            Fixed,
            GeneratedByAssetName,
            GeneratedByAssetFolder,
        }
    }
}