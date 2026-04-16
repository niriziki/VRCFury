using nadena.dev.ndmf;
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

            // Check if any SPS components exist
            var hasSps = avatarObj.GetComponentInChildren<VRCFuryHapticPlug>(true) != null
                      || avatarObj.GetComponentInChildren<VRCFuryHapticSocket>(true) != null;
            if (!hasSps) return;

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
