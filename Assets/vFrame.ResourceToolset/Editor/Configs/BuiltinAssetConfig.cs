using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;

namespace vFrame.ResourceToolset.Editor.Configs
{
    [CreateAssetMenu(
        fileName = nameof(BuiltinAssetConfig),
        menuName = ToolsetConst.BuiltinAssetConfigMenuName,
        order = 0)]
    [FilePath(
        ToolsetConst.BuiltinAssetConfigFilePath,
        FilePathAttribute.Location.ProjectFolder)]
    internal class BuiltinAssetConfig : RTSScriptableSingleton<BuiltinAssetConfig>
    {
        [SerializeField]
        [Sirenix.OdinInspector.FilePath]
        private string _builtinReplacementMaterialsDir;

        [SerializeField]
        [Sirenix.OdinInspector.FilePath]
        private string _builtinReplacementUISkinDir;

        public string BuiltinReplacementMaterialsDir {
            get => _builtinReplacementMaterialsDir;
            set => _builtinReplacementMaterialsDir = value;
        }

        public string BuiltinReplacementUISkinDir {
            get => _builtinReplacementUISkinDir;
            set => _builtinReplacementUISkinDir = value;
        }
    }
}