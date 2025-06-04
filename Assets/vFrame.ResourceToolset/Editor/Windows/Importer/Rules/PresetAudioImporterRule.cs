using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Common;

namespace vFrame.ResourceToolset.Editor.Windows.Importer.Rules
{
    public class PresetAudioImporterRule : AssetImporterRuleBase<AudioImporter>
    {
        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Audio Import Setting")]
        [HideLabel]
        private PresetAudioImportSetting _importSetting = new PresetAudioImportSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Audio Import Platform Setting")]
        [Header("iOS")]
        [HideLabel]
        private PresetAudioImportPlatformSetting _iOSPlatformSetting = new PresetAudioImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Audio Import Platform Setting")]
        [Header("Android")]
        [HideLabel]
        private PresetAudioImportPlatformSetting _androidPlatformSetting = new PresetAudioImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Audio Import Platform Setting")]
        [Header("WebGL")]
        [HideLabel]
        private PresetAudioImportPlatformSetting _webGLPlatformSetting = new PresetAudioImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Audio Import Platform Setting")]
        [Header("Standalone")]
        [HideLabel]
        private PresetAudioImportPlatformSetting _standalonePlatformSetting = new PresetAudioImportPlatformSetting();

        [SerializeField]
        [VerticalGroup(GroupName)]
        [TitleGroup(GroupName + "/Audio Import Platform Setting")]
        [Header("Windows Store Apps")]
        [HideLabel]
        private PresetAudioImportPlatformSetting _wsaPlatformSetting = new PresetAudioImportPlatformSetting();

        private bool UpdateImporterSettings(AudioImporter assetImporter) {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var ret = false;
            if (SetImporterPropertyValueIfEnable(_importSetting.ForceToMono, assetImporter, nameof(assetImporter.forceToMono))) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.LoadInBackground, assetImporter, nameof(assetImporter.loadInBackground))) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.Ambisonic, assetImporter, nameof(assetImporter.ambisonic))) {
                ret = true;
            }
            if (SetImporterPropertyValueIfEnable(_importSetting.PreloadAudioData, assetImporter, nameof(assetImporter.preloadAudioData))) {
                ret = true;
            }
            return ret;
        }

        private bool UpdatePlatformSettings(PresetAudioImportPlatformSetting inputSettings, AudioImporterSampleSettings outputSettings) {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var ret = false;
            if (SetImporterFieldValueIfEnable(inputSettings.LoadType, outputSettings, nameof(outputSettings.loadType))) {
                ret = true;
            }
            if (SetImporterFieldValueIfEnable(inputSettings.CompressionFormat, outputSettings, nameof(outputSettings.compressionFormat))) {
                ret = true;
            }
            if (SetImporterFieldValueIfEnable(inputSettings.Quality, outputSettings, nameof(outputSettings.quality))) {
                ret = true;
            }
            if (SetImporterFieldValueIfEnable(inputSettings.SampleRateSetting, outputSettings, nameof(outputSettings.sampleRateSetting))) {
                ret = true;
            }
            if (inputSettings.SampleRateSetting.Enabled && inputSettings.SampleRateSetting.Value == AudioSampleRateSetting.OverrideSampleRate) {
                if (SetImporterFieldValueIfEnable(inputSettings.SampleRateOverride, outputSettings, nameof(outputSettings.sampleRateOverride))) {
                    ret = true;
                }
            }
            return ret;
        }

        protected override bool ProcessImport(AudioImporter assetImporter) {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var ret = false;

            // Update importer settings
            if (UpdateImporterSettings(assetImporter)) {
                ret = true;
            }

            // Update platform settings
            void UpdatePlatformSettingsIfEnabled(PresetAudioImportPlatformSetting inputSettings, string platform) {
                if (inputSettings.Override.Enabled) {
                    if (inputSettings.Override.Value) {
                        var platformSettings = assetImporter.GetOverrideSampleSettings(platform);
                        if (UpdatePlatformSettings(inputSettings, platformSettings)) {
                            assetImporter.SetOverrideSampleSettings(platform, platformSettings);
                            ret = true;
                        }
                    }
                    else {
                        assetImporter.ClearSampleSettingOverride(platform);
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
        private class PresetAudioImportSetting
        {
            [SerializeField]
            private ToggleableFieldBool _forceToMono = new ToggleableFieldBool("Force To Mono");

            [SerializeField]
            private ToggleableFieldBool _loadInBackground = new ToggleableFieldBool("Load In Background");

            [SerializeField]
            private ToggleableFieldBool _ambisonic = new ToggleableFieldBool("Ambisonic");

            [SerializeField]
            private ToggleableFieldBool _preloadAudioData = new ToggleableFieldBool("Preload Audio Data");

            public ToggleableFieldBool ForceToMono => _forceToMono;
            public ToggleableFieldBool LoadInBackground => _loadInBackground;
            public ToggleableFieldBool Ambisonic => _ambisonic;
            public ToggleableFieldBool PreloadAudioData => _preloadAudioData;
        }

        [Serializable]
        private class PresetAudioImportPlatformSetting
        {
            [SerializeField]
            private ToggleableFieldBool _override = new ToggleableFieldBool("Override");

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value")]
            [Indent]
            private TFAudioClipLoadType _loadType = new TFAudioClipLoadType("Load Type");

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value")]
            [Indent]
            private TFAudioCompressionFormat _compressionFormat = new TFAudioCompressionFormat("Compression Format");

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value")]
            [Indent]
            private ToggleableFieldFloat _quality = new ToggleableFieldFloat("Quality");

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value")]
            [Indent]
            private TFAudioSampleRateSetting _sampleRateSetting = new TFAudioSampleRateSetting("Sample Rate Setting");

            [SerializeField]
            [ShowIf("@_override.Enabled && _override.Value && _sampleRateSetting.Value == AudioSampleRateSetting.OverrideSampleRate")]
            [Indent]
            private ToggleableFieldUInt _sampleRateOverride = new ToggleableFieldUInt("Sample Rate Override");

            public ToggleableFieldBool Override => _override;
            public TFAudioClipLoadType LoadType => _loadType;
            public TFAudioCompressionFormat CompressionFormat => _compressionFormat;
            public ToggleableFieldFloat Quality => _quality;
            public TFAudioSampleRateSetting SampleRateSetting => _sampleRateSetting;
            public ToggleableFieldUInt SampleRateOverride => _sampleRateOverride;
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFAudioClipLoadType : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            public AudioClipLoadType Value ;

            #pragma warning restore 649

            public TFAudioClipLoadType(string label) : base(label) {
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFAudioSampleRateSetting : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            public AudioSampleRateSetting Value ;

            #pragma warning restore 649

            public TFAudioSampleRateSetting(string label) : base(label) {
            }
        }

        [Serializable]
        [InlineProperty]
        [HideLabel]
        private class TFAudioCompressionFormat : ToggleableField
        {
            #pragma warning disable 649

            [SerializeField]
            [HideLabel]
            [HorizontalGroup(GroupName)]
            [EnableIf("Enabled")]
            public AudioCompressionFormat Value ;

            #pragma warning restore 649

            public TFAudioCompressionFormat(string label) : base(label) {
            }
        }
    }
}