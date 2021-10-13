using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using vFrame.ResourceToolset.Editor.Const;
using vFrame.ResourceToolset.Editor.Utils;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Windows.Migrate
{
    public class AssetMigratorWindow : ResourceToolsetWindow
    {
        [MenuItem(ToolsetConst.ToolsMenuDir + "Migrator")]
        private static void OpenAssetMigratorWindow() {
            var window = GetWindow<AssetMigratorWindow>();
            window.titleContent = new GUIContent("Asset Migrator");
            window.Show();
        }

        // ==========================================================
        // Drawer

        private const string Group1 = "Targets";
        private const string Group2 = "Dependencies";

        private const string Group1_SubGroup1 = "Targets/Filters";
        private const string Group1_SubGroup2 = "Targets/Process Targets";
        private const string Group2_SubGroup1 = "Dependencies/Filters";
        private const string Group2_SubGroup2 = "Dependencies/Mode";
        private const string Group2_SubGroup3 = "Dependencies/Dependent Assets";
        private const string Group2_SubGroup3_Sub1 = "Dependencies/Dependent Assets/Operations";
        private const string Group2_SubGroup4 = "Dependencies/Operations";
        private const string Group2_SubGroup5 = "Dependencies/GroupBy";
        private const string Group2_SubGroup5_Sub1 = "Dependencies/GroupBy/1";

        private const string DeleteOperationPrompt = "Please make a backup before processing DELETE, continue?";

        private enum GroupType
        {
            None,
            AssetName,
            FilePath,
            FileMD5,
        }

#pragma warning disable CS0414, CS0469

        //================ Group1 =================

        [ShowInInspector]
        [TitleGroup(Group1, Alignment = TitleAlignments.Centered)]
        [TitleGroup(Group1_SubGroup1)]
        [HideLabel]
        private AssetFilter _realObjectFilter = new AssetFilter();

        [PropertySpace(SpaceBefore = 5)]
        [ShowInInspector]
        [TitleGroup(Group1)]
        [TitleGroup(Group1_SubGroup2)]
        [AssetsOnly]
        private List<Object> _processAssets = new List<Object>();

        [ShowInInspector]
        [TitleGroup(Group1)]
        [ToggleLeft]
        private bool _showRealObjects = false;

        [ShowInInspector]
        [TitleGroup(Group1)]
        [ShowIf("_showRealObjects")]
        [TableList]
        [ReadOnly]
        private HashSet<Object> _realObjects = new HashSet<Object>();

        //================ Group2 =================

        [ShowInInspector]
        [TitleGroup(Group2, Alignment = TitleAlignments.Centered)]
        [TitleGroup(Group2_SubGroup1)]
        [HideLabel]
        private AssetFilter _dependenciesFilter = new AssetFilter();

        [ShowInInspector]
        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup2)]
        [ToggleLeft]
        private bool _analyzeDependenciesRecursive = true;

        [ShowInInspector]
        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup5)]
        [HorizontalGroup(Group2_SubGroup5_Sub1, Width = 250)]
        [LabelWidth(80)]
        private GroupType _groupType = GroupType.None;

        [ShowInInspector]
        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup5)]
        [HorizontalGroup(Group2_SubGroup5_Sub1, Width = 100)]
        [Button(ButtonSizes.Small)]
        [LabelText("Refresh")]
        private void RefreshByGroupType() {
            var items = _dependencies.SelectMany(v => v.Assets);
            _dependencies = GroupDependenciesInternal(items.ToList());
        }

        [PropertySpace]
        [ShowInInspector]
        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup3)]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true)]
        private List<DependencyAssetGroup> _dependencies = new List<DependencyAssetGroup>();

        [ShowInInspector]
        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup3)]
        [HideIf("_searched")]
        [LabelText("Please Search Dependencies First.")]
        [DisplayAsString]
        private string _dependenciesTips = "";

        private bool _searched = true;

#pragma warning restore CS0414, CS0469

        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup4)]
        [Button(ButtonSizes.Medium)]
        private void RefreshDependencies() {
            if (_processAssets.Count <= 0) {
                ShowNotification("Please Select Objects To Process.");
                return;
            }

            _searched = true;

            // Must delay call for 1 frame, otherwise error of Odin will occur.
            /*
             * IndexOutOfRangeException: Index was outside the bounds of the array.
             * Sirenix.OdinInspector.Editor.PropertyChildren.Get (System.Int32 index) (at <65e93e2b5170492382789d6ed1597fdb>:0)
             * Sirenix.OdinInspector.Editor.PropertyChildren.get_Item (System.Int32 index) (at <65e93e2b5170492382789d6ed1597fdb>:0)
             * Sirenix.OdinInspector.Editor.Drawers.TableListAttributeDrawer.DrawCell (Sirenix.OdinInspector.Editor.Drawers.TableListAttributeDrawer+Column col, System.Int32 rowIndex) (at <65e93e2b5170492382789d6ed1597fdb>:0)
             * ...
             */
            var delayCall = (EditorApplication.CallbackFunction)null;
            delayCall = () => {
                FilterRealProcessTargets();
                SearchAndGroupDependenciesInternal();
                EditorApplication.update -= delayCall;
            };
            EditorApplication.update += delayCall;
        }

        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup4)]
        [Button(ButtonSizes.Medium)]
        private void MoveSelectedDependencies() {
            var selected = GetSelectedDependencies();
            OnAssetsMoveClick(selected);
        }

        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup4)]
        [Button(ButtonSizes.Medium)]
        private void ReplaceSelectedDependencies() {
            var selected = GetSelectedDependencies();
            OnAssetsReplaceClick(selected);
        }

        [TitleGroup(Group2)]
        [TitleGroup(Group2_SubGroup4)]
        [Button(ButtonSizes.Medium)]
        private void DeleteSelectedDependencies() {
            var selected = GetSelectedDependencies();
            OnAssetsDeleteClick(selected);
        }

        private IEnumerable<DependencyAssetListItem> GetSelectedDependencies() {
            return
                from assetGroup in _dependencies
                from asset in assetGroup.Assets
                where asset.Selected
                select asset;
        }

        // ==========================================================
        // Processor

        private void FilterRealProcessTargets() {
            EditorUtility.DisplayProgressBar("Filtering Real Objects", "", 0f);

            var folders = _processAssets.Where(v =>
                v && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(v)));
            var files = _processAssets.Where(v =>
                v && !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(v)));

            EditorUtility.DisplayProgressBar("Filtering Real Objects", "", 0.2f);

            _realObjects.Clear();
            _realObjects.AddRange(LoadMainAndSubAssets(files));
            EditorUtility.DisplayProgressBar("Filtering Real Objects", "", 0.4f);

            _realObjects.AddRange(LoadAllAssetsInFolder(folders));
            EditorUtility.DisplayProgressBar("Filtering Real Objects", "", 0.8f);

            _realObjects = _realObjects.Where(v => !_realObjectFilter.FilterTest(v)).ToHashSet();
            EditorUtility.ClearProgressBar();
        }

        private static IEnumerable<Object> LoadAllAssetsInFolder(IEnumerable<Object> folders) {
            var fds = folders.Select(AssetDatabase.GetAssetPath).ToArray();
            if (fds.Length <= 0) {
                return Enumerable.Empty<Object>();
            }

            var guids = AssetDatabase.FindAssets("t:Object", fds);
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var objects = paths.SelectMany(AssetMigrationUtils.LoadAllAssetsAtPath);
            return objects;
        }

        private static IEnumerable<Object> LoadMainAndSubAssets(IEnumerable<Object> objects) {
            var assets = objects
                .Select(AssetDatabase.GetAssetPath)
                .SelectMany(AssetMigrationUtils.LoadAllAssetsAtPath);
            return assets;
        }

        private IEnumerable<DependencyAssetListItem> SearchDependenciesInternal() {
            var ret = new Dictionary<Object, DependencyAssetListItem>();
            try {
                var index = 0f;
                foreach (var obj in _realObjects) {
                    EditorUtility.DisplayProgressBar("Collecting Dependencies", obj.ToString(),
                        ++index / _realObjects.Count);
                    var dependencies = new HashSet<Object>();
                    AssetMigrationUtils.CollectDependencies(obj, ref dependencies, _analyzeDependenciesRecursive);
                    foreach (var dependency in dependencies) {
                        if (ret.TryGetValue(dependency, out var target)) {
                            target.ReferenceCount += 1;
                            target.References.Add(obj);
                        }
                        else {
                            var asset = new DependencyAssetListItem(dependency);
                            asset.OnMoveClick.AddListener(OnAssetMoveClick);
                            asset.OnDuplicateClick.AddListener(OnAssetDuplicateClick);
                            asset.OnReplaceClick.AddListener(OnAssetReplaceClick);
                            asset.OnDeleteClick.AddListener(OnAssetDeleteClick);
                            asset.References.Add(obj);
                            ret[dependency] = asset;
                        }
                    }
                }

                var dependenciesRet = ret.Select(d => d.Value)
                    .Where(v => !_dependenciesFilter.FilterTest(v.Asset))
                    .ToList();
                dependenciesRet.Sort((a, b) => string.Compare(
                    a.GetType().GetCompilableNiceFullName(),
                    b.GetType().GetCompilableNiceFullName(),
                    StringComparison.Ordinal));
                return dependenciesRet;
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private List<DependencyAssetGroup> GroupDependenciesInternal(List<DependencyAssetListItem> assets) {
            string GetGroupNameOfType(DependencyAssetListItem dependency) {
                switch (_groupType) {
                    case GroupType.None:
                        return "";
                    case GroupType.AssetName:
                        return dependency.Asset.name;
                    case GroupType.FilePath:
                        return dependency.Path;
                    case GroupType.FileMD5:
                        return CalculateFileMD5(dependency.Path);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var groups = new Dictionary<string, DependencyAssetGroup>();
            var index = 0f;
            foreach (var dependency in assets) {
                EditorUtility.DisplayProgressBar("Grouping", dependency.Asset.name, ++index / assets.Count);
                var groupName = GetGroupNameOfType(dependency);
                if (!groups.TryGetValue(groupName, out var group)) {
                    groups[groupName] = group = new DependencyAssetGroup(groupName);
                }
                group.Assets.Add(dependency);
            }
            EditorUtility.ClearProgressBar();

            var ret = groups.Select(kv => kv.Value).ToList();
            ret.Sort((a,b) => -a.Assets.Count.CompareTo(b.Assets.Count));
            return ret;
        }

        private void SearchAndGroupDependenciesInternal() {
            var dependencies = SearchDependenciesInternal();
            var group = GroupDependenciesInternal(dependencies.ToList());
            _dependencies = group;
        }

        private void OnAssetMoveClick(DependencyAssetListItem target) {
            var sourcePath = target.Path;
            var fileName = Path.GetFileName(sourcePath);
            var fileDir = Path.GetDirectoryName(target.Path);
            var extension = Path.GetExtension(target.Path)?.Replace(".", "") ?? "";

            var destPath = EditorUtility.SaveFilePanelInProject("Move To",
                fileName,
                extension,
                "Select file path to move to..",
                fileDir);

            if (string.IsNullOrEmpty(destPath) || destPath == sourcePath) {
                return;
            }

            var ret = AssetDatabase.MoveAsset(sourcePath, destPath);
            if (string.IsNullOrEmpty(ret)) {
                ShowNotification("Move succeed!");
                return;
            }

            Debug.LogError("Move file failed: " + ret);
            ShowNotification("Move failed.");
        }

        private void OnAssetsMoveClick(IEnumerable<DependencyAssetListItem> assets) {
            var destDir = EditorUtility.OpenFolderPanel("Move To", Application.dataPath, "");
            if (string.IsNullOrEmpty(destDir)) {
                return;
            }

            destDir = destDir.MakeRelativePath(Application.dataPath);

            var failed = new List<string>();
            var moved = new List<string>();
            foreach (var asset in assets) {
                var sourcePath = asset.Path;
                var sourceName = Path.GetFileName(sourcePath);
                if (moved.Contains(sourcePath)) {
                    continue;
                }

                var destPath = PathCombine(destDir, sourceName);
                var ret = AssetDatabase.MoveAsset(sourcePath, destPath);
                if (string.IsNullOrEmpty(ret)) {
                    moved.Add(sourcePath);
                    continue;
                }
                failed.Add(asset.Asset.name);
                Debug.LogError("Move file failed: " + ret);
            }

            if (failed.Count > 0) {
                ShowNotification("Some Asset Move Failed.");
            }
            else {
                ShowNotification("Move successful.");
            }
        }

        private void OnAssetDuplicateClick(DependencyAssetListItem target) {
            var sourcePath = target.Path;
            var fileName = Path.GetFileName(target.Path);
            var fileDir = Path.GetDirectoryName(target.Path);
            var extension = Path.GetExtension(target.Path)?.Replace(".", "") ?? "";

            var destPath = EditorUtility.SaveFilePanelInProject("Save As",
                fileName,
                extension,
                "Select file path to save as..",
                fileDir);

            if (string.IsNullOrEmpty(destPath) || destPath == sourcePath) {
                return;
            }

            var ret = AssetDatabase.CopyAsset(sourcePath, destPath);
            if (!ret) {
                ShowNotification("Duplicate Failed");
                return;
            }

            var newAsset = AssetMigrationUtils.LoadAssetAtPathWithFileId(destPath, target.FileId);
            if (!newAsset) {
                Debug.LogErrorFormat("Load asset at path failed, path: {0}, fileId: {1}", destPath, target.FileId);
                ShowNotification("Duplicate Failed");
                return;
            }

            if (AssetMigrationUtils.ReplaceAsset(_realObjects.ToArray(), target.Asset, newAsset)) {
                AssetDatabase.Refresh();
                if (EditorUtility.DisplayDialog("Tips", "Duplicate Asset Succeed.", "Refresh Dependencies", "Cancel")) {
                    SearchAndGroupDependenciesInternal();
                }
                return;
            }

            ShowNotification("Nothing changed.");
        }

        private void OnAssetReplaceClick(DependencyAssetListItem target) {
            void OnSelectionConfirmed<T>(IEnumerable<T> selection) where T: Object {
                var first = selection.FirstOrDefault();
                if (!first) {
                    return;
                }

                if (first == target.Asset) {
                    return;
                }

                if (AssetMigrationUtils.ReplaceAsset(_realObjects.ToArray(), target.Asset, first)) {
                    AssetDatabase.Refresh();
                    if (EditorUtility.DisplayDialog("Tips", "Replace Asset Succeed.", "Refresh Dependencies", "Cancel")) {
                        SearchAndGroupDependenciesInternal();
                    }
                    return;
                }

                ShowNotification("Nothing changed.");
            }

            AssetSelector.OpenSelector(target.Asset, OnSelectionConfirmed);
        }

        private void OnAssetsReplaceClick(IEnumerable<DependencyAssetListItem> assets) {
            var listItems = assets.ToList();
            if (!listItems.Any()) {
                return;
            }

            var first = listItems.FirstOrDefault();
            if (first == null) {
                return;
            }
            var firstType = first.Asset.GetType();
            if (listItems.Any(v => v.Asset.GetType() != firstType)) {
                ShowNotification("All assets to be replaced must be the same type!");
                return;
            }

            void OnSelectionConfirmed<T>(IEnumerable<T> selection) where T: Object {
                var firstSelection = selection.FirstOrDefault();
                if (!firstSelection) {
                    return;
                }

                var succeed = new List<string>();
                foreach (var asset in listItems.Where(v => v.Asset != firstSelection)) {
                    if (!AssetMigrationUtils.ReplaceAsset(_realObjects.ToArray(), asset.Asset, firstSelection)) {
                        continue;
                    }
                    succeed.Add(asset.Path);
                }

                if (succeed.Count > 0) {
                    if (EditorUtility.DisplayDialog("Tips", "Replace Asset Succeed.", "Refresh Dependencies", "Cancel")) {
                        SearchAndGroupDependenciesInternal();
                    }
                    ShowNotification("All assets successfully replaced!");
                }
                else {
                    ShowNotification("Nothing changed.");
                }
                AssetDatabase.Refresh();
            }

            AssetSelector.OpenSelector(first.Asset, OnSelectionConfirmed);
        }

        private void OnAssetDeleteClick(DependencyAssetListItem target) {
            if (!EditorUtility.DisplayDialog("Warning", DeleteOperationPrompt, "OK", "Cancel")) {
               return;
            }

            if (AssetDatabase.DeleteAsset(target.Path)) {
                ShowNotification("Asset successfully deleted.");
                return;
            }
            ShowNotification("Asset delete failed.");
        }

        private void OnAssetsDeleteClick(IEnumerable<DependencyAssetListItem> targets) {
            if (!EditorUtility.DisplayDialog("Warning", DeleteOperationPrompt, "OK", "Cancel")) {
               return;
            }

            var result = true;
            foreach (var target in targets) {
                var ret = AssetDatabase.DeleteAsset(target.Path);
                result &= ret;
            }

            if (result) {
                ShowNotification("Assets successfully deleted.");
                return;
            }
            ShowNotification("Some assets delete failed.");
        }
    }
}