using System.Collections.Generic;
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
            var driven = spsCtx?.Injector?.GetService<SpsDeclareExternalAapsService>()?.DrivenAaps;
            if (driven == null || driven.Count == 0) return;

            var descriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            var prms = descriptor == null ? null : descriptor.expressionParameters;
            if (prms == null || prms.parameters == null) return;

            // The expression parameters are the ones MA produced, so the SPS names have to be
            // taken through the renames MA applied to reach them.
            var renames = spsCtx.ParameterRenames;
            var finalNames = new HashSet<string>(driven.Select(name =>
                renames != null && renames.TryGetValue(name, out var renamed) ? renamed : name));

            var reported = new HashSet<string>();
            foreach (var param in prms.parameters) {
                if (param == null || !param.networkSynced) continue;
                if (!finalNames.Contains(param.name)) continue;
                if (!reported.Add(param.name)) continue;
                SpsErrors.ReportSyncedAap(param.name);
            }
        }
    }
}
