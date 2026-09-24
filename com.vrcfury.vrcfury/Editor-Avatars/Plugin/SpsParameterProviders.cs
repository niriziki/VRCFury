using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;
using VF.Component;

namespace VF.Plugin {
    /**
     * Lets editor tools estimate the synced bits SPS will use. The real names get a build-order
     * prefix (VF{n}_) that is only known during the build, so these names are placeholders:
     * hidden, and shared between components wherever the build creates one parameter per avatar,
     * so that NDMF merges them the same way. The conditions mirror BakeHapticSocketsService and
     * BakeHapticPlugsService; parameters that are not network synced are left out.
     */
    [ParameterProviderFor(typeof(VRCFuryHapticSocket))]
    internal class SpsSocketParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticSocket socket;

        public SpsSocketParameterProvider(VRCFuryHapticSocket socket) {
            this.socket = socket;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            if (!socket.addMenuItem) yield break;
            yield return SpsParameters.SyncedBool(socket, $"SPS/Socket/{socket.GetInstanceID()}");
            yield return SpsParameters.SyncedBool(socket, "SPS/Stealth");
            if (socket.useLights) yield return SpsParameters.SyncedBool(socket, "SPS/Legacy");
        }
    }

    [ParameterProviderFor(typeof(VRCFuryHapticPlug))]
    internal class SpsPlugParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticPlug plug;

        public SpsPlugParameterProvider(VRCFuryHapticPlug plug) {
            this.plug = plug;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            if (plug.enableSps) {
                yield return SpsParameters.SyncedBool(plug, "SPS/DisableDepth");
                yield return SpsParameters.SyncedBool(plug, "SPS/DisableRealtimeShadows");
            }
            if (plug.addDpsTipLight) yield return SpsParameters.SyncedBool(plug, "SPS/TipLight");
        }
    }

    internal static class SpsParameters {
        public static ProvidedParameter SyncedBool(UnityEngine.Component source, string name) {
            return new ProvidedParameter(name, ParameterNamespace.Animator, source, SpsNdmfPlugin.Instance,
                AnimatorControllerParameterType.Bool) {
                IsHidden = true,
                WantSynced = true
            };
        }
    }
}
