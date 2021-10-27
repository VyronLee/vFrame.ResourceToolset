namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    internal abstract class AssetFilterBase
    {
        public abstract string[] GetFiles();
        public abstract bool FilterTest(string path);
    }
}