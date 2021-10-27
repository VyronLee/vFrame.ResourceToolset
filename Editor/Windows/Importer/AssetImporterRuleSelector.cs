using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace vFrame.ResourceToolset.Editor.Windows.Importer
{
    internal class AssetImporterRuleSelector : OdinSelector<Type>
    {
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.Selection.SupportsMultiSelect = false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = new List<Type>();
            foreach (var assembly in assemblies) {
                var ret = assembly.GetTypes()
                    .Where(t => typeof(AssetImporterRuleBase).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract);
                types.AddRange(ret);
            }
            tree.AddRange(types, v => v.Name);
        }

        private bool HasSelection() => null != GetCurrentSelection().FirstOrDefault();

        [ShowInInspector]
        [ShowIf("HasSelection")]
        [HideLabel]
        [DisplayAsString]
        private string Preview => GetCurrentSelection().FirstOrDefault()?.FullName;
    }
}