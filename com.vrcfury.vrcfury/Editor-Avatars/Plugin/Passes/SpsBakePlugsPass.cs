using nadena.dev.ndmf;
using VF.Service;

namespace VF.Plugin.Passes {
    internal class SpsBakePlugsPass : Pass<SpsBakePlugsPass> {
        public override string DisplayName => "SPS Bake Plugs";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            if (spsCtx.Injector == null) return;

            var service = spsCtx.Injector.GetService<BakeHapticPlugsService>();
            service.ApplyEarlyBake();
            service.Apply();
        }
    }
}
