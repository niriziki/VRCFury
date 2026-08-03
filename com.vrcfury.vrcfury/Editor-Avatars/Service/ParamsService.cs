using VF.Builder;
using VF.Injector;
using VF.Plugin;
using VF.Utils;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Service {
    [VFService]
    internal class ParamsService {
        [VFAutowired] private readonly VRCAvatarDescriptor avatar;
        [VFAutowired] private readonly IAvatarOutput avatarOutput;
        
        private ParamManager _params;
        public ParamManager GetParams() {
            if (_params == null) _params = MakeParams();
            return _params;
        }

        private ParamManager MakeParams() {
            var origParams = avatarOutput.GetAvatarParams();
            VRCExpressionParameters prms;
            if (VrcfObjectFactory.DidCreate(origParams)) {
                // We probably made this in an earlier preprocessor hook, so we can just adopt it
                prms = origParams;
            } else if (origParams != null) {
                prms = origParams.Clone();
                prms.WorkLog($"Loaded expression parameters for mutation from {origParams.GetPathAndName()} ({origParams.parameters.Length} entries)");
            } else {
                prms = VrcfObjectFactory.Create<VRCExpressionParameters>();
                prms.parameters = new VRCExpressionParameters.Parameter[] { };
            }
            avatarOutput.SetAvatarParams(prms);
            prms.RemoveDuplicates();
            return new ParamManager(prms);
        }

        public void ClearCache() {
            _params = null;
        }

        public VRCExpressionParameters GetReadOnlyParams() {
            var p = avatarOutput.GetAvatarParams();
            if (p == null) {
                p = VrcfObjectFactory.Create<VRCExpressionParameters>();
                p.parameters = new VRCExpressionParameters.Parameter[] { };
            }
            return p;
        }
    }
}
