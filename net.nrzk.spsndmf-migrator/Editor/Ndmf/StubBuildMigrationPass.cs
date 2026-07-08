#if SPSNDMF_VFSTUB
using nadena.dev.ndmf;
using Nrzk.SpsMigrator.Convert;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator.Ndmf {
    internal sealed class StubBuildMigrationPass : Pass<StubBuildMigrationPass> {
        public override string DisplayName => "SPSNDMF Stub Migration";

        protected override void Execute(BuildContext context) {
            if (!PackageBinding.VrcfAvailable || !PackageBinding.SpsNdmfAvailable) return;

            var root = context.AvatarRootObject;
            var plan = new ConversionPlan();
            var ops = new BuildConversionOps();

            PlugSocketConverter.Plan(root, vrcfToSpsNdmf: true, plan);
            if (PackageBinding.GlobalColliderConvertible) GlobalColliderConverter.Plan(root, plan);
            if (PackageBinding.TouchReceiverConvertible) TouchConverter.PlanReceiver(root, plan);
            if (PackageBinding.TouchSenderConvertible) TouchConverter.PlanSender(root, plan);

            PlugSocketConverter.Execute(root, vrcfToSpsNdmf: true, plan, ops);
            if (PackageBinding.GlobalColliderConvertible) GlobalColliderConverter.Execute(root, plan, ops);
            if (PackageBinding.TouchReceiverConvertible) TouchConverter.ExecuteReceiver(root, plan, ops);
            if (PackageBinding.TouchSenderConvertible) TouchConverter.ExecuteSender(root, plan, ops);
        }
    }
}
#endif
