using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VF.Component;
using VF.Inspector;
using VF.Upgradeable;
using VF.Utils;
using Object = UnityEngine.Object;

namespace VF.Prefabs {
    /**
     * Migrates every VRCFury component in the project to the current schema in one pass.
     *
     * A prefab instance carries a list of overrides written against the schema its source prefab had at the time.
     * Migrating the source alone leaves those overrides pointing at fields the migration emptied or stopped reading,
     * so the instance silently loses settings. Instead, each instance's post-migration state is computed from its own
     * effective (post-override) values, and its override list is rebuilt from scratch against the migrated source.
     */
    internal static class PrefabMigration {
        private const string modeMenu = "Tools/VRCFury/SPS/Allow Editing on Prefab Instances";
        private const string runMenu = "Tools/VRCFury/SPS/Migrate Project Data";

        [MenuItem(modeMenu, priority = 1330)]
        private static void ToggleMode() {
            if (!PrefabInstanceMode.Unlocked) {
                var ok = DialogUtils.DisplayDialog(
                    SpsLocalization.Get("prefabMigration.title"),
                    SpsLocalization.Get("prefabMigration.mode.confirm"),
                    SpsLocalization.Get("prefabMigration.mode.enable"),
                    SpsLocalization.Get("prefabMigration.cancel")
                );
                if (!ok) return;
            }
            PrefabInstanceMode.Unlocked = !PrefabInstanceMode.Unlocked;
        }

        [MenuItem(modeMenu, true)]
        private static bool ValidateMode() {
            UnityEditor.Menu.SetChecked(modeMenu, PrefabInstanceMode.Unlocked);
            return true;
        }

        [MenuItem(runMenu, true)]
        private static bool ValidateRun() {
            return PrefabInstanceMode.Unlocked;
        }

        [MenuItem(runMenu, priority = 1331)]
        public static void Run() {
            var ok = DialogUtils.DisplayDialog(
                SpsLocalization.Get("prefabMigration.title"),
                SpsLocalization.Get("prefabMigration.run.confirm"),
                SpsLocalization.Get("prefabMigration.run.ok"),
                SpsLocalization.Get("prefabMigration.cancel")
            );
            if (!ok) return;

            DialogUtils.DisplayDialog(
                SpsLocalization.Get("prefabMigration.title"),
                WithProjectScenesOpen(Migrate),
                SpsLocalization.Get("prefabMigration.ok")
            );
        }

        /**
         * Without this the migration is only reachable from a menu, so a locked component gives no hint that anything
         * can be done about it.
         */
        public static VisualElement CreateInspectorNotice(VRCFuryComponent c) {
            var container = new VisualElement();
            if (!PrefabInstanceMode.Unlocked) return container;
            if (PrefabInstanceMode.IsUpToDate(c)) return container;

            container.Add(VRCFuryEditorUtils.Info(SpsLocalization.Get("prefabMigration.notice")));
            container.Add(new Button(Run) { text = SpsLocalization.Get("prefabMigration.notice.button") });
            return container;
        }

        /**
         * Every instance in the project has to be visible at once, since an override list can only be rebuilt while
         * its component is loaded. Scenes shipped inside read-only packages are skipped: unity refuses to open them
         * with a modal dialog, and nothing in them could be written anyway.
         */
        private static string WithProjectScenesOpen(Func<string> fn) {
            var openScenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToList();
            if (openScenes.Any(s => string.IsNullOrEmpty(s.path))) {
                return SpsLocalization.Get("prefabMigration.result.unsavedScene");
            }

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            var alreadyOpen = new HashSet<string>(openScenes.Select(s => s.path));
            var opened = new List<Scene>();
            try {
                foreach (var path in AssetDatabase.FindAssets("t:SceneAsset")
                             .Select(AssetDatabase.GUIDToAssetPath)
                             .Where(p => p.StartsWith("Assets/"))
                             .Distinct()) {
                    if (alreadyOpen.Contains(path)) continue;
                    opened.Add(EditorSceneManager.OpenScene(path, OpenSceneMode.Additive));
                }

                var result = fn();

                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
                return result;
            } finally {
                foreach (var scene in opened) EditorSceneManager.CloseScene(scene, true);
                EditorUtility.UnloadUnusedAssetsImmediate();
            }
        }

        private class Plan {
            public VRCFuryComponent target;
            public VRCFuryComponent upgraded;
            public GameObject scratch;
            public bool isInstance;
        }

        private static string Migrate() {
            var targets = BulkUpgradeUtils.FindAll<VRCFuryComponent>()
                .Where(c => c != null && !c.IsBroken() && !PrefabInstanceMode.IsUpToDate(c))
                .ToList();
            if (targets.Count == 0) return SpsLocalization.Get("prefabMigration.result.upToDate");

            var plans = new List<Plan>();
            var blocked = new List<string>();
            var skipped = new List<string>();
            try {
                foreach (var target in targets) {
                    /*
                     * Nothing can be written to a read-only package, but its instances still can be, and they are
                     * what the user actually works with. Leaving the source behind means an instance ends up holding
                     * the whole migration as overrides, which is worse than inheriting it but better than a project
                     * that can never be migrated at all.
                     */
                    if (PrefabUtility.IsPartOfPrefabAsset(target) && PrefabUtility.IsPartOfImmutablePrefab(target)) {
                        skipped.Add(Describe(target));
                        continue;
                    }
                    GameObject scratch = null;
                    VRCFuryComponent upgraded = null;
                    try {
                        upgraded = VRCFuryComponentEditor.CreateUpgradedClone(target, out scratch);
                    } catch (Exception e) {
                        blocked.Add($"{Describe(target)}: " + SpsLocalization.Get("prefabMigration.blocked.failed", e.Message));
                        continue;
                    }
                    plans.Add(new Plan {
                        target = target,
                        upgraded = upgraded,
                        scratch = scratch,
                        isInstance = PrefabUtility.IsPartOfPrefabInstance(target)
                    });
                }

                var planByTarget = new Dictionary<VRCFuryComponent, Plan>();
                foreach (var plan in plans) planByTarget[plan.target] = plan;

                foreach (var plan in plans.Where(p => p.isInstance)) {
                    if (plan.upgraded == null || plan.scratch.GetComponents<VRCFuryComponent>().Length != 1) {
                        blocked.Add($"{Describe(plan.target)}: "
                                    + SpsLocalization.Get("prefabMigration.blocked.componentCount"));
                        continue;
                    }
                    var source = GetSource(plan.target);
                    if (source == null) continue;
                    var sourceState = planByTarget.TryGetValue(source, out var sourcePlan)
                        ? (Object)sourcePlan.upgraded
                        : source;
                    if (sourceState == null) continue;
                    if (ManagedReferencesDiffer(plan.upgraded, sourceState)) {
                        blocked.Add($"{Describe(plan.target)}: "
                                    + SpsLocalization.Get("prefabMigration.blocked.managedReference"));
                    }
                }

                if (blocked.Count > 0) {
                    Debug.LogError(SpsLocalization.Get("prefabMigration.log.aborted")
                                   + "\n" + string.Join("\n", blocked));
                    return SpsLocalization.Get("prefabMigration.result.aborted", blocked.Count);
                }

                foreach (var plan in plans.OrderBy(p => SourceDepth(p.target))) {
                    if (plan.isInstance) {
                        UnitySerializationUtils.CloneSerializable(plan.upgraded, plan.target);
                    } else {
                        IUpgradeableUtility.UpgradeRecursive(plan.target);
                    }
                    if (plan.target != null) plan.target.Dirty();
                }

                var done = SpsLocalization.Get("prefabMigration.result.done", plans.Count);
                if (skipped.Count > 0) {
                    Debug.LogWarning(SpsLocalization.Get("prefabMigration.log.skipped")
                                     + "\n" + string.Join("\n", skipped));
                    done += "\n\n" + SpsLocalization.Get("prefabMigration.result.skipped", skipped.Count);
                }
                return done;
            } finally {
                foreach (var plan in plans) {
                    if (plan.scratch != null) Object.DestroyImmediate(plan.scratch);
                }
            }
        }

        /**
         * A prefab override is recorded against the instance's immediate source, so every source has to be migrated
         * before anything that inherits from it.
         */
        private static int SourceDepth(Object o) {
            var depth = 0;
            for (var i = GetSource(o); i != null; i = GetSource(i)) depth++;
            return depth;
        }

        private static T GetSource<T>(T o) where T : Object {
            try {
                return PrefabUtility.GetCorrespondingObjectFromSource(o);
            } catch (Exception) {
                return null;
            }
        }

        /**
         * Unity has no way to express a [SerializeReference] value as a prefab override, so if the instance and its
         * source do not agree on one after migrating, the instance's version cannot be stored anywhere.
         */
        private static bool ManagedReferencesDiffer(Object a, Object b) {
            var ia = new SerializedObject(a).GetIterator();
            var ib = new SerializedObject(b).GetIterator();
            var managedRefDepth = -1;
            while (true) {
                var moreA = ia.NextVisible(true);
                var moreB = ib.NextVisible(true);
                if (moreA != moreB) return true;
                if (!moreA) return false;
                if (ia.propertyPath != ib.propertyPath) return true;

                if (managedRefDepth >= 0 && ia.depth <= managedRefDepth) managedRefDepth = -1;
                if (ia.propertyType == SerializedPropertyType.ManagedReference) {
                    if (ia.managedReferenceFullTypename != ib.managedReferenceFullTypename) return true;
                    if (managedRefDepth < 0) managedRefDepth = ia.depth;
                }
                if (managedRefDepth >= 0 && !SerializedProperty.DataEquals(ia, ib)) return true;
            }
        }

        private static string Describe(VRCFuryComponent c) {
            var path = AssetDatabase.GetAssetPath(c);
            if (string.IsNullOrEmpty(path)) path = c.gameObject.scene.path;
            return $"{c.GetType().Name} on {c.owner().GetPath()} ({path})";
        }
    }
}
