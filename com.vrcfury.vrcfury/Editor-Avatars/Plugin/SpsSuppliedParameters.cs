using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;
using VF.Component;

namespace VF.Plugin {
    /**
     * The synced parameters the build will add for a socket or plug, for editor tools estimating bit
     * usage. The real names get a build-order prefix (VF{n}_) that is only known during the build, so
     * these names are placeholders: hidden, prefixed so they cannot meet a user's parameter, and shared
     * between components wherever the build creates one parameter per avatar, so that NDMF merges them
     * the same way. The conditions mirror BakeHapticSocketsService and BakeHapticPlugsService;
     * parameters that are not network synced are left out.
     *
     * Also shipped in VRCFury Stub, whose components the migrator converts at build time.
     */
    internal static class SpsSuppliedParameters {
        private const string Prefix = "__SPSNDMF/";

        public static IEnumerable<ProvidedParameter> For(VRCFuryHapticSocket socket, PluginBase plugin) {
            if (!socket.addMenuItem) yield break;
            yield return SyncedBool(socket, plugin, $"Socket/{socket.GetInstanceID()}");
            yield return SyncedBool(socket, plugin, "Stealth");
            if (socket.useLights) yield return SyncedBool(socket, plugin, "Legacy");
        }

        public static IEnumerable<ProvidedParameter> For(VRCFuryHapticPlug plug, PluginBase plugin) {
            if (EnableSps(plug)) {
                yield return SyncedBool(plug, plugin, "DisableDepth");
                yield return SyncedBool(plug, plugin, "DisableRealtimeShadows");
            }
            if (plug.addDpsTipLight) yield return SyncedBool(plug, plugin, "TipLight");
        }

        // The build upgrades components before reading them; loading does not.
        private static bool EnableSps(VRCFuryHapticPlug plug) {
#pragma warning disable 0612
            if (plug.Version >= 0 && plug.Version < 3) return plug.configureSps;
            return plug.enableSps;
        }

        private static ProvidedParameter SyncedBool(UnityEngine.Component source, PluginBase plugin, string name) {
            return new ProvidedParameter(Prefix + name, ParameterNamespace.Animator, source, plugin,
                AnimatorControllerParameterType.Bool) {
                IsHidden = true,
                WantSynced = true
            };
        }
    }
}
