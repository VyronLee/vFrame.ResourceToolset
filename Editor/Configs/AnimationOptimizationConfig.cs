using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;

namespace vFrame.ResourceToolset.Editor.Configs
{
    [CreateAssetMenu(
        fileName = nameof(AnimationOptimizationConfig),
        menuName = ToolsetConst.AnimationOptimizationConfigMenuName,
        order = 0)]
    [FilePath(
        ToolsetConst.AnimationOptimizationConfigFilePath,
        FilePathAttribute.Location.ProjectFolder)]
    public class AnimationOptimizationConfig : ScriptableSingleton<AnimationOptimizationConfig>
    {
        [SerializeField]
        [Range(1, 6)]
        private uint _precision = 3;

        public uint Precision => _precision;
    }
}