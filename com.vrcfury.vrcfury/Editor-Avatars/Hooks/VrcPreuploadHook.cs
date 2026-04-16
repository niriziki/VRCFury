using UnityEngine;
using VF.Builder;
using VF.Menu;
using VF.Utils;

namespace VF.Hooks {
    internal class VrcPreuploadHook : VrcfAvatarPreprocessor {
        protected override int order => -10000;

        protected override void Process(VFGameObject obj) {
            // SPS-NDMF: VRCFury auto-build disabled, NDMF handles SPS processing
            return;
        }
    }
}
