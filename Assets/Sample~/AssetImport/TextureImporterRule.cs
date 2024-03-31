using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Windows.Importer;
using Random = UnityEngine.Random;

namespace Test.AssetImport
{
    public class TextureImporterRule : AssetImporterRuleBase<TextureImporter>
    {
        [SerializeField]
        [VerticalGroup(GroupName)]
        [LabelWidth(LabelWidth)]
        private bool _isReadable;

        protected override bool ProcessImport(TextureImporter assetImporter) {
            return Random.Range(1, 100) < 50;
        }
    }
}