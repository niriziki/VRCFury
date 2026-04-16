using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VF.Builder;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Plugin {
    /// <summary>
    /// NDMF implementation: records output in internal buffer for MA component generation.
    /// Does NOT write to Avatar Descriptor. Reads fall back to Descriptor for types not yet Set.
    /// SpsOutputPass converts recorded data to MA Merge Animator / Menu Installer / Parameters.
    /// </summary>
    internal class NdmfAvatarOutput : IAvatarOutput {
        private readonly VRCAvatarDescriptor avatar;

        // Recorded outputs for MA component generation
        public readonly Dictionary<VRCAvatarDescriptor.AnimLayerType, RuntimeAnimatorController> Controllers
            = new Dictionary<VRCAvatarDescriptor.AnimLayerType, RuntimeAnimatorController>();
        public VRCExpressionsMenu Menu { get; private set; }
        public VRCExpressionParameters Params { get; private set; }

        public NdmfAvatarOutput(VRCAvatarDescriptor avatar) {
            this.avatar = avatar;
        }

        public void SetAvatarController(VRCAvatarDescriptor.AnimLayerType type, RuntimeAnimatorController controller) {
            Controllers[type] = controller;
        }

        public (bool isDefault, RuntimeAnimatorController controller) GetAvatarController(VRCAvatarDescriptor.AnimLayerType type) {
            // Return from buffer if Set was called for this type
            if (Controllers.TryGetValue(type, out var controller)) {
                return (false, controller);
            }
            // Fall back to Descriptor for types not yet Set
            return VRCAvatarUtils.GetAvatarController(avatar, type);
        }

        public IList<AvatarOutputController> GetAllAvatarControllers() {
            // Start with Descriptor state, overlay with Set values
            var result = VRCAvatarUtils.GetAllControllers(avatar)
                .Select(c => new AvatarOutputController {
                    type = c.type,
                    isDefault = c.isDefault,
                    controller = c.controller
                })
                .ToList();

            foreach (var entry in Controllers) {
                var existing = result.FirstOrDefault(c => c.type == entry.Key);
                if (existing != null) {
                    existing.isDefault = false;
                    existing.controller = entry.Value;
                }
            }

            return result;
        }

        public void SetAvatarMenu(VRCExpressionsMenu menu) {
            Menu = menu;
        }

        [CanBeNull]
        public VRCExpressionsMenu GetAvatarMenu() {
            return Menu ?? VRCAvatarUtils.GetAvatarMenu(avatar);
        }

        public void SetAvatarParams(VRCExpressionParameters prms) {
            Params = prms;
        }

        [CanBeNull]
        public VRCExpressionParameters GetAvatarParams() {
            return Params ?? VRCAvatarUtils.GetAvatarParams(avatar);
        }
    }
}
