using nadena.dev.ndmf;
using VF.Plugin.Passes;

[assembly: ExportsPlugin(typeof(VF.Plugin.SpsNdmfPlugin))]

namespace VF.Plugin {
    internal class SpsNdmfPlugin : Plugin<SpsNdmfPlugin> {
        public override string QualifiedName => "net.nrzk.sps-ndmf";
        public override string DisplayName => "SPS-NDMF";

        protected override void Configure() {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(SpsBuildPass.Instance)
                .Then.Run(SpsOutputPass.Instance);
        }
    }
}
