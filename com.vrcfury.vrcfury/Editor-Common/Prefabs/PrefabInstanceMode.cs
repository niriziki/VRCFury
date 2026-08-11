using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VF.Component;
using VF.Inspector;
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

        /**
         * An action list is the one part of a socket or plug that lives in a [SerializeReference] field, which unity
         * cannot store as a prefab override. Everything else about a depth animation -- its range, units, even adding
         * and removing entries -- is an ordinary field and stays editable.
         */
        public static VisualElement LockActionsOnPrefabInstance(SerializedProperty prop, VisualElement actions) {
            var target = prop.serializedObject.targetObject;
            if (!(target is VRCFuryComponent) || !PrefabUtility.IsPartOfPrefabInstance(target)) return actions;

            actions.SetEnabled(false);
            var container = new VisualElement();
            container.Add(VRCFuryEditorUtils.Info(SpsLocalization.Get("prefabMigration.actionsLocked")));
            container.Add(actions);
            return container;
        }

        public static bool ShouldRenderReadOnlyPreview(VRCFuryComponent c) {
            if (PrefabUtility.IsPartOfPrefabInstance(c)) return !AllowInstanceEditing(c);
            return ShouldSkipAutoUpgrade(c) && !IsUpToDate(c);
        }
    }
}
