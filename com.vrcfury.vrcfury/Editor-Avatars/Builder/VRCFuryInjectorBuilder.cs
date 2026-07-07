using System.Collections.Generic;
using UnityEditor;
using VF.Actions;
using VF.Injector;
using VF.Service;
using VF.Utils;
using VRC.SDK3.Avatars.Components;

namespace VF.Builder {
    /**
     * Gets the injector context for the currently building avatar.
     * This needs to be cached for a frame, because multiple preprocessor
     * hooks may reuse this context during a single avatar build.
     */
    internal static class VRCFuryInjectorBuilder {
        private static Dictionary<VRCAvatarDescriptor, VRCFuryInjector> cached
            = new Dictionary<VRCAvatarDescriptor, VRCFuryInjector>();
        
        [VFInit]
        private static void Init() {
            Scheduler.Schedule(() => {
                cached.Clear();
            }, 0);
        }
        
        public static VRCFuryInjector GetInjector(VRCAvatarDescriptor avatar) {
            return cached.GetOrCreate(avatar, () => MakeInjector(avatar));
        }

        private static VRCFuryInjector MakeInjector(VRCAvatarDescriptor avatar) {
            var injector = new VRCFuryInjector();
            injector.ImportScan(typeof(VFServiceAttribute));
            injector.ImportScan(typeof(ActionBuilder));
            injector.Set(avatar);
            injector.Set("avatarObject", avatar.owner());
            injector.Set(typeof(Plugin.IAvatarOutput), new Plugin.DescriptorAvatarOutput(avatar));

            var globals = new GlobalsService {
                avatarObject = avatar.owner(),
            };
            injector.Set(globals);
            return injector;
        }

        /// <summary>
        /// Create an injector for NDMF mode with NdmfAvatarOutput instead of DescriptorAvatarOutput.
        /// </summary>
        public static VRCFuryInjector CreateForNdmf(VRCAvatarDescriptor avatar, VFGameObject avatarObject, Plugin.IAvatarOutput avatarOutput) {
            var injector = new VRCFuryInjector();
            injector.ImportScan(typeof(VFServiceAttribute));
            injector.ImportScan(typeof(ActionBuilder));
            injector.Set(avatar);
            injector.Set("avatarObject", avatarObject);
            injector.Set(typeof(Plugin.IAvatarOutput), avatarOutput);

            var globals = new GlobalsService {
                avatarObject = avatarObject,
            };
            injector.Set(globals);
            return injector;
        }
    }
}
