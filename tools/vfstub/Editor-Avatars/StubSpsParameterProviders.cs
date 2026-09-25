using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using VF.Component;

namespace VF.Plugin {
    /**
     * Stub components only reach the SPS build when the migrator converts them for SPSNDMF, so they
     * report the parameters SPSNDMF will add, attributed to SPSNDMF, and nothing when either is missing.
     */
    internal static class StubSpsParameterSource {
        private static readonly Lazy<PluginBase> plugin = new Lazy<PluginBase>(() => {
            var plugins = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetCustomAttributes(typeof(ExportsPlugin), false))
                .Select(e => ((ExportsPlugin)e).PluginType
                    .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?
                    .GetValue(null) as PluginBase)
                .Where(p => p != null)
                .ToList();
            if (plugins.All(p => p.QualifiedName != "net.nrzk.spsndmf-migrator")) return null;
            return plugins.FirstOrDefault(p => p.QualifiedName == "net.nrzk.spsndmf");
        });

        public static PluginBase Plugin => plugin.Value;
    }

    [ParameterProviderFor(typeof(VRCFuryHapticSocket))]
    internal class StubSpsSocketParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticSocket socket;

        public StubSpsSocketParameterProvider(VRCFuryHapticSocket socket) {
            this.socket = socket;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            var plugin = StubSpsParameterSource.Plugin;
            return plugin == null ? Enumerable.Empty<ProvidedParameter>() : SpsSuppliedParameters.For(socket, plugin);
        }
    }

    [ParameterProviderFor(typeof(VRCFuryHapticPlug))]
    internal class StubSpsPlugParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticPlug plug;

        public StubSpsPlugParameterProvider(VRCFuryHapticPlug plug) {
            this.plug = plug;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            var plugin = StubSpsParameterSource.Plugin;
            return plugin == null ? Enumerable.Empty<ProvidedParameter>() : SpsSuppliedParameters.For(plug, plugin);
        }
    }
}
