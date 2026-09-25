using System.Collections.Generic;
using nadena.dev.ndmf;
using VF.Component;

namespace VF.Plugin {
    [ParameterProviderFor(typeof(VRCFuryHapticSocket))]
    internal class SpsSocketParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticSocket socket;

        public SpsSocketParameterProvider(VRCFuryHapticSocket socket) {
            this.socket = socket;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            return SpsSuppliedParameters.For(socket, SpsNdmfPlugin.Instance);
        }
    }

    [ParameterProviderFor(typeof(VRCFuryHapticPlug))]
    internal class SpsPlugParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticPlug plug;

        public SpsPlugParameterProvider(VRCFuryHapticPlug plug) {
            this.plug = plug;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            return SpsSuppliedParameters.For(plug, SpsNdmfPlugin.Instance);
        }
    }
}
