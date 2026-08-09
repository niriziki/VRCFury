using System.IO;
using nadena.dev.ndmf;
using UnityEditor;
using VF.Builder;
using VF.Component;
using VF.Utils;
using VRC.SDK3.Avatars.Components;

namespace VF.Plugin.Passes {
    /// <summary>
    /// Main SPS build pass. Runs VRCFury's build pipeline (ApplyFuryConfigs)
    /// with NdmfAvatarOutput so that Controller/Menu/Params are recorded
    /// instead of written to the Avatar Descriptor.
    /// GameObjects and Contacts are created directly in the hierarchy.
    /// </summary>
    internal class SpsBuildPass : Pass<SpsBuildPass> {
        public override string DisplayName => "SPS Build";

        protected override void Execute(BuildContext context) {
            var avatarObj = context.AvatarRootObject;
            if (avatarObj == null) return;

            var avatar = avatarObj.GetComponent<VRCAvatarDescriptor>();
            if (avatar == null) return;

            if (!SpsErrors.CheckDuplicateComponents(avatarObj)) return;

            // Check if any SPS components exist
            var hasSps = avatarObj.GetComponentInChildren<VRCFuryHapticPlug>(true) != null
                      || avatarObj.GetComponentInChildren<VRCFuryHapticSocket>(true) != null;
            var spsMenusAll = avatarObj.GetComponentsInChildren<VRCFurySpsMenus>(true);
            if (!hasSps) {
                // Without any SPS content, the menu component must not leave an
                // empty submenu behind when MA resolves the menu tree.
                foreach (var spsMenus in spsMenusAll) {
                    UnityEngine.Object.DestroyImmediate(spsMenus);
                }
                return;
            }
            if (spsMenusAll.Length > 1) {
                throw new System.Exception(
                    "Multiple SPS Menus components were found on this avatar. Only one is allowed.");
            }
            if (spsMenusAll.Length == 1) {
                // Remove legacy SpsOptions containers so their menuPath cannot
                // move the menu away from the default "SPS" folder that
                // SpsOutputPass extracts (the SPS Menus component wins).
                foreach (var vf in avatarObj.GetComponentsInChildren<VF.Model.VRCFury>(true)) {
                    if (vf.content is VF.Model.Feature.SpsOptions) {
                        UnityEngine.Object.DestroyImmediate(vf);
                    }
                }
                // Carry saveSockets/legacyModeEnabledOnAvatarLoad through the
                // unmodified upstream pipeline by injecting a temporary SpsOptions
                // feature into the build clone. The menu itself is generated the
                // same way with or without this injection (menuPath defaults to
                // "SPS" either way); it only exists to deliver these two settings.
                var tmp = avatarObj.AddComponent<VF.Model.VRCFury>();
                tmp.content = new VF.Model.Feature.SpsOptions {
                    saveSockets = spsMenusAll[0].saveSockets,
                    legacyModeEnabledOnAvatarLoad = spsMenusAll[0].legacyModeEnabledOnAvatarLoad,
                };
            }

            var containerPath = AssetDatabase.GetAssetPath(context.AssetContainer);
            if (string.IsNullOrEmpty(containerPath))
                throw new System.Exception("SPS-NDMF requires NDMF AssetSaver (got null AssetContainer).");
            TmpFilePackage.TmpDirPath = Path.Combine(Path.GetDirectoryName(containerPath), "spsndmf-temp");

            var avatarVf = avatarObj.asVf();
            var output = new NdmfAvatarOutput(avatar);
            var injector = VRCFuryInjectorBuilder.CreateForNdmf(avatar, avatarVf, output);

            // Store context for SpsOutputPass
            var spsCtx = context.GetState<SpsContext>();
            spsCtx.Injector = injector;
            spsCtx.AvatarOutput = output;

            // Run VRCFury's full build pipeline
            VRCFuryBuilder.RunForNdmf(avatarVf, injector);
        }
    }
}
