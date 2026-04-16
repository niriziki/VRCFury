using nadena.dev.ndmf;
using VF.Service;

namespace VF.Plugin.Passes {
    internal class SpsOptionsPass : Pass<SpsOptionsPass> {
        public override string DisplayName => "SPS Options";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            if (spsCtx.Injector == null) return;

            var service = spsCtx.Injector.GetService<SpsOptionsService>();
            service.MoveMenus();
        }
    }
}
