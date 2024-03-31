using UnityEditor;
using vFrame.ResourceToolset.Editor.Windows.Importer;
using Random = UnityEngine.Random;

namespace Test.AssetImport
{
    public class PrefabImporterRule : AssetImporterRuleBase<TextureImporter>
    {
        protected override bool ProcessImport(TextureImporter assetImporter) {
            return Random.Range(1, 100) < 50;
        }
    }
}