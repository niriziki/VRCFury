using nadena.dev.ndmf;
using VF.Service;

namespace VF.Plugin.Passes {
    internal class SpsSendersPass : Pass<SpsSendersPass> {
        public override string DisplayName => "SPS Senders For All";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            if (spsCtx.Injector == null) return;

            var service = spsCtx.Injector.GetService<SpsSendersForAllService>();
            service.Apply();
        }
    }
}
