using vFrame.ResourceToolset.Editor.Common;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    internal abstract class AssetFilterBase : ICollapsableFieldSummary
    {
        public abstract string[] GetFiles();
        public abstract bool FilterTest(string path);
        public abstract string GetSummary();
    }
}