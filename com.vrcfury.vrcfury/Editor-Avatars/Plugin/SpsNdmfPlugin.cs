using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using VF.Plugin.Passes;

[assembly: ExportsPlugin(typeof(VF.Plugin.SpsNdmfPlugin))]

namespace VF.Plugin {
    internal class SpsNdmfPlugin : Plugin<SpsNdmfPlugin> {
        public override string QualifiedName => "net.nrzk.spsndmf";
        public override string DisplayName => "SPS-NDMF";

        protected override void Configure() {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(SpsBuildPass.Instance)
                .Then.Run(SpsOutputPass.Instance);

            // User-authored animations only become visible once MA has merged them.
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .WithRequiredExtension(typeof(AnimatorServicesContext), seq => {
                    seq.Run(SpsExternalAnimationRewritePass.Instance);
                    seq.Run(SpsSyncedAapWarningPass.Instance);
                });
        }
    }
}
