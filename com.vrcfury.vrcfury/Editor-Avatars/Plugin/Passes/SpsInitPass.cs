using nadena.dev.ndmf;
using VF.Component;
using VF.Builder;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace VF.Plugin.Passes {
    internal class SpsInitPass : Pass<SpsInitPass> {
        public override string DisplayName => "SPS Init";

        protected override void Execute(BuildContext context) {
            var avatarObj = context.AvatarRootObject;
            if (avatarObj == null) return;

            // Check if any SPS components exist
            var hasPlug = avatarObj.GetComponentInChildren<VRCFuryHapticPlug>(true) != null;
            var hasSocket = avatarObj.GetComponentInChildren<VRCFuryHapticSocket>(true) != null;
            if (!hasPlug && !hasSocket) return;

            var avatar = avatarObj.GetComponent<VRCAvatarDescriptor>();
            if (avatar == null) return;

            var avatarVf = avatarObj.AsVf();
            var output = new NdmfAvatarOutput(avatar);
            var injector = VRCFuryInjectorBuilder.CreateForNdmf(avatar, avatarVf, output);

            var spsCtx = context.GetState<SpsContext>();
            spsCtx.Injector = injector;
            spsCtx.AvatarOutput = output;
        }
    }
}
