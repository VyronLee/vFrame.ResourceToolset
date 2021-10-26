using UnityEngine;

namespace vFrame.ResourceToolset.Editor.Configs
{
    public class AnimationOptimizationConfig : ResourceToolsetConfig
    {
        [SerializeField]
        [Range(1, 6)]
        private uint _precision = 3;

        public uint Precision => _precision;
    }
}