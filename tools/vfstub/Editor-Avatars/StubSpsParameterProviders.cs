using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using UnityEngine;
using VF.Component;

namespace VF.Plugin {
    /**
     * Stub components only reach the SPS build when the migrator converts them for SPSNDMF, so they
     * report the parameters SPSNDMF will add, attributed to SPSNDMF, and nothing when either is missing.
     */
    internal static class StubSpsParameterSource {
        private static readonly Lazy<PluginBase> plugin = new Lazy<PluginBase>(() => {
            // Only these two assemblies are looked at: reading Instance creates the plugin, and editor
            // queries must not construct unrelated plugins or fail because of them.
            var plugins = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => {
                    var name = a.GetName().Name;
                    return name == "SPSNDMF-Editor-Avatars" || name == "Nrzk.SpsMigrator.Editor";
                })
                .SelectMany(a => a.GetCustomAttributes(typeof(ExportsPlugin), false))
                .Select(e => GetInstance(((ExportsPlugin)e).PluginType))
                .Where(p => p != null)
                .ToList();
            if (plugins.All(p => p.QualifiedName != "net.nrzk.spsndmf-migrator")) return null;
            return plugins.FirstOrDefault(p => p.QualifiedName == "net.nrzk.spsndmf");
        });

        private static PluginBase GetInstance(Type pluginType) {
            try {
                return pluginType
                    .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?
                    .GetValue(null) as PluginBase;
            } catch (Exception e) {
                Debug.LogException(e);
                return null;
            }
        }

        /**
         * The migrator leaves a stub component alone when its SPSNDMF counterpart is already on the same
         * object, and the build then ignores it.
         */
        public static PluginBase PluginFor(UnityEngine.Component stub) {
            var type = stub.GetType();
            if (stub.GetComponents<UnityEngine.Component>().Any(c => c != null && c.GetType() != type && c.GetType().Name == type.Name)) {
                return null;
            }
            return plugin.Value;
        }
    }

    [ParameterProviderFor(typeof(VRCFuryHapticSocket))]
    internal class StubSpsSocketParameterProvider : IParameterProvider {
        private readonly VRCFuryHapticSocket socket;

        public StubSpsSocketParameterProvider(VRCFuryHapticSocket socket) {
            this.socket = socket;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) {
            var plugin = StubSpsParameterSource.PluginFor(socket);
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
            var plugin = StubSpsParameterSource.PluginFor(plug);
            return plugin == null ? Enumerable.Empty<ProvidedParameter>() : SpsSuppliedParameters.For(plug, plugin);
        }
    }
}
