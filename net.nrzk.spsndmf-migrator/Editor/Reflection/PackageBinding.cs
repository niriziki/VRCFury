using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nrzk.SpsMigrator.Reflection {
    internal static class PackageBinding {
        private static readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();

        public static Type FindType(string fullName) {
            if (_cache.TryGetValue(fullName, out var cached)) return cached;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Type t;
                try {
                    t = asm.GetType(fullName, throwOnError: false);
                } catch {
                    continue;
                }
                if (t != null) {
                    _cache[fullName] = t;
                    return t;
                }
            }
            _cache[fullName] = null;
            return null;
        }

        public static Type VrcfPlugType => FindType("VF.Component.VRCFuryHapticPlug");
        public static Type VrcfSocketType => FindType("VF.Component.VRCFuryHapticSocket");
        public static Type VrcfGlobalColliderType => FindType("VF.Component.VRCFuryGlobalCollider");
        public static Type VrcfTouchReceiverType => FindType("VF.Component.VRCFuryHapticTouchReceiver");
        public static Type VrcfTouchSenderType => FindType("VF.Component.VRCFuryHapticTouchSender");

        public static Type SpsNdmfPlugType => FindType("SpsNdmf.Component.VRCFuryHapticPlug");
        public static Type SpsNdmfSocketType => FindType("SpsNdmf.Component.VRCFuryHapticSocket");

        public static bool VrcfAvailable => VrcfPlugType != null && VrcfSocketType != null;
        public static bool SpsNdmfAvailable => SpsNdmfPlugType != null && SpsNdmfSocketType != null;

        public static bool GlobalColliderConvertible => VrcfGlobalColliderType != null;
        public static bool TouchReceiverConvertible => VrcfTouchReceiverType != null;
        public static bool TouchSenderConvertible => VrcfTouchSenderType != null;
    }
}
