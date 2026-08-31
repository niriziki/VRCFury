using System.Linq;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;

namespace VF.Plugin.Passes {
    /// <summary>
    /// Upstream VRCFury unsyncs any expression parameter it drives with an AAP
    /// (DisableSyncForAapsService), but SPS-NDMF only owns its own parameter list, and
    /// Modular Avatar's merge can only ever add sync, never remove it. Warn instead, so
    /// the user can decide. Runs after MA, which is when the avatar's expression
    /// parameters reach their final state.
    /// </summary>
    internal class SpsSyncedAapWarningPass : Pass<SpsSyncedAapWarningPass> {
        public override string DisplayName => "SPS Synced AAP Warning";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            var declared = spsCtx?.Injector?.GetService<SpsDeclareExternalAapsService>()?.Declared;
            if (declared == null || declared.Count == 0) return;

            var descriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            var prms = descriptor == null ? null : descriptor.expressionParameters;
            if (prms == null || prms.parameters == null) return;

            foreach (var param in prms.parameters) {
                if (param == null || !param.networkSynced) continue;
                if (!declared.Contains(param.name)) continue;
                SpsErrors.ReportSyncedAap(param.name);
            }
        }
    }
}
