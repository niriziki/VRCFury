using nadena.dev.ndmf;
using VF.Service;

namespace VF.Plugin.Passes {
    internal class SpsBakeSocketsPass : Pass<SpsBakeSocketsPass> {
        public override string DisplayName => "SPS Bake Sockets";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            if (spsCtx.Injector == null) return;

            var service = spsCtx.Injector.GetService<BakeHapticSocketsService>();
            service.Apply();
        }
    }
}
