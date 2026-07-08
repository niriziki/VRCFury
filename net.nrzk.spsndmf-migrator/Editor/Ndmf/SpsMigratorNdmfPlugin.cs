#if SPSNDMF_VFSTUB
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(Nrzk.SpsMigrator.Ndmf.SpsMigratorNdmfPlugin))]

namespace Nrzk.SpsMigrator.Ndmf {
    internal sealed class SpsMigratorNdmfPlugin : Plugin<SpsMigratorNdmfPlugin> {
        public override string QualifiedName => "net.nrzk.spsndmf-migrator";
        public override string DisplayName => "SPSNDMF Stub Migrator";

        protected override void Configure() {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("net.nrzk.spsndmf")
                .Run(StubBuildMigrationPass.Instance);
        }
    }
}
#endif
