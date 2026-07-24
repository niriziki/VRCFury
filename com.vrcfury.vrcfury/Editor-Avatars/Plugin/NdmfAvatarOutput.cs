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
    /// Does NOT write to Avatar Descriptor. Does NOT read from Descriptor.
    /// Services always start from empty controllers/menu/params, ensuring only
    /// SPS-generated data is output to MA components via SpsOutputPass.
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

        public bool AppliesToAvatarInPlace => false;

        public void SetAvatarController(VRCAvatarDescriptor.AnimLayerType type, RuntimeAnimatorController controller) {
            Controllers[type] = controller;
        }

        public (bool isDefault, RuntimeAnimatorController controller) GetAvatarController(VRCAvatarDescriptor.AnimLayerType type) {
            if (Controllers.TryGetValue(type, out var controller)) {
                return (false, controller);
            }
            // Do not fall back to Descriptor — return empty so MakeController creates
            // a fresh controller with only SPS-generated layers.
            return (true, null);
        }

        public IList<AvatarOutputController> GetAllAvatarControllers() {
            // Only return controllers that were actually Set by SPS services.
            // This prevents NoBadControllerParamsService.GetAllUsedControllers() from
            // triggering MakeController for controller types SPS never touched.
            return Controllers.Select(kvp => new AvatarOutputController {
                type = kvp.Key,
                isDefault = false,
                controller = kvp.Value
            }).ToList();
        }

        public void SetAvatarMenu(VRCExpressionsMenu menu) {
            Menu = menu;
        }

        [CanBeNull]
        public VRCExpressionsMenu GetAvatarMenu() {
            // Do not fall back to Descriptor — SPS menu items only.
            return Menu;
        }

        public void SetAvatarParams(VRCExpressionParameters prms) {
            Params = prms;
        }

        [CanBeNull]
        public VRCExpressionParameters GetAvatarParams() {
            // Do not fall back to Descriptor — SPS params only.
            return Params;
        }
    }
}
