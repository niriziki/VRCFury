using UnityEditor;
using UnityEngine;
using VF.Component;
using VF.Model;
using VF.Upgradeable;

namespace VF.Prefabs {
    /**
     * A prefab instance's override list is written against one specific schema version, and unity offers no way to
     * migrate it. Upstream resolves this by refusing to let anything be overridden at all. The unlocked mode trades
     * that lock for an explicit whole-project migration which rewrites assets and override lists together
     * (see PrefabMigration).
     */
    [FilePath("ProjectSettings/SpsNdmfPrefabInstanceMode.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class PrefabInstanceMode : ScriptableSingleton<PrefabInstanceMode> {
        [SerializeField] private bool unlocked;

        public static bool Unlocked {
            get => instance.unlocked;
            set {
                instance.unlocked = value;
                instance.Save(true);
            }
        }

        /**
         * Migrating one prefab asset on its own orphans the overrides that its instances carry, so in unlocked mode
         * migration may only run through PrefabMigration, which covers every instance in the same transaction.
         */
        public static bool ShouldSkipAutoUpgrade(VRCFuryComponent c) {
            return Unlocked && PrefabUtility.IsPartOfAnyPrefab(c);
        }

        public static bool IsUpToDate(VRCFuryComponent c) {
            var upToDate = true;
            UnitySerializationUtils.Iterate(c, visit => {
                if (!visit.outgoingLink
                    && visit.value is IUpgradeable u
                    && u.Version >= 0
                    && u.Version < u.GetLatestVersion()) {
                    upToDate = false;
                }
                return UnitySerializationUtils.IterateResult.Continue;
            });
            return upToDate;
        }

        /**
         * The VRCFury component keeps its entire body in a [SerializeReference] field, which unity cannot express as
         * a prefab override, so it stays locked in both modes.
         */
        public static bool AllowInstanceEditing(VRCFuryComponent c) {
            return Unlocked && !(c is VRCFury) && IsUpToDate(c);
        }

        public static bool ShouldRenderReadOnlyPreview(VRCFuryComponent c) {
            if (PrefabUtility.IsPartOfPrefabInstance(c)) return !AllowInstanceEditing(c);
            return ShouldSkipAutoUpgrade(c) && !IsUpToDate(c);
        }
    }
}
