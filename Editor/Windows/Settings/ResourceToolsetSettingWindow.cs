using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Configs;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;

namespace vFrame.ResourceToolset.Editor.Windows.Settings
{
    internal class ResourceToolsetSettingWindow : ResourceToolsetWindow
    {
        [MenuItem(ToolsetConst.ToolsMenuDir + "Settings")]
        private static void OpenSettingWindow() {
            var window = GetWindow<ResourceToolsetSettingWindow>();
            window.titleContent = new GUIContent("Toolset Settings");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(860, 720);
            window.Initialize();
            window.Show();
        }


        // ==========================================================
        // Drawer

        private GUIStyle _headerStyle;

        private GUIStyle HeaderStyle {
            get
            {
                if (null != _headerStyle) {
                    return _headerStyle;
                }
                _headerStyle = new GUIStyle(GUI.skin.label) {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                return _headerStyle;
            }
        }

        [OnInspectorGUI]
        [PropertyOrder(1)]
        [PropertySpace(SpaceAfter = 20)]
        private void DrawHeader() {
            GUILayout.Label("vFrame Resource Toolset Settings", HeaderStyle);
        }

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [PropertyOrder(2)]
        private ResourceToolsetSettingConfig _settingConfig;

        [Button(ButtonSizes.Gigantic)]
        [ShowIf("@!_settingConfig")]
        [PropertyOrder(3)]
        private void CreateResourceToolsetSettingConfig() {
            _settingConfig = ScriptableObjectUtils.ConfirmCreateScriptableObject<ResourceToolsetSettingConfig>();
        }

        // ==========================================================
        // Processor

        protected override void Initialize() {
            _settingConfig = ScriptableObjectUtils.GetScriptableObjectSingleton<ResourceToolsetSettingConfig>();
        }
    }
}