using com.vrcfury.api.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace com.vrcfury.api {
    /** Public API for creating VRCFury components */
    [PublicAPI]
    public static class FuryComponents {

        public static FurySocket CreateSocket(GameObject obj) {
            return new FurySocket(obj);
        }
    }
}
