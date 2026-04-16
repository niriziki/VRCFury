using nadena.dev.ndmf;
using VF.Plugin.Passes;

[assembly: ExportsPlugin(typeof(VF.Plugin.SpsNdmfPlugin))]

namespace VF.Plugin {
    internal class SpsNdmfPlugin : Plugin<SpsNdmfPlugin> {
        public override string QualifiedName => "com.vrcfury.sps-ndmf";
        public override string DisplayName => "SPS-NDMF";

        protected override void Configure() {
            InPhase(BuildPhase.Generating)
                .Run<SpsInitPass>()
                .Then.Run<SpsSendersPass>()
                .Then.Run<SpsBakePlugsPass>()
                .Then.Run<SpsBakeSocketsPass>()
                .Then.Run<SpsOptionsPass>()
                .Then.Run<SpsOutputPass>();
        }
    }
}
