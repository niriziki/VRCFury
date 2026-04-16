using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VF.Builder;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Plugin {
    /// <summary>
    /// Default implementation: reads/writes Avatar Descriptor directly via VRCAvatarUtils.
    /// Used when running under VRCFury's own build pipeline.
    /// </summary>
    internal class DescriptorAvatarOutput : IAvatarOutput {
        private readonly VRCAvatarDescriptor avatar;

        public DescriptorAvatarOutput(VRCAvatarDescriptor avatar) {
            this.avatar = avatar;
        }

        public void SetAvatarController(VRCAvatarDescriptor.AnimLayerType type, RuntimeAnimatorController controller) {
            VRCAvatarUtils.SetAvatarController(avatar, type, controller);
        }

        public (bool isDefault, RuntimeAnimatorController controller) GetAvatarController(VRCAvatarDescriptor.AnimLayerType type) {
            return VRCAvatarUtils.GetAvatarController(avatar, type);
        }

        public IList<AvatarOutputController> GetAllAvatarControllers() {
            return VRCAvatarUtils.GetAllControllers(avatar)
                .Select(c => new AvatarOutputController {
                    type = c.type,
                    isDefault = c.isDefault,
                    controller = c.controller
                })
                .ToList();
        }

        public void SetAvatarMenu(VRCExpressionsMenu menu) {
            VRCAvatarUtils.SetAvatarMenu(avatar, menu);
        }

        [CanBeNull]
        public VRCExpressionsMenu GetAvatarMenu() {
            return VRCAvatarUtils.GetAvatarMenu(avatar);
        }

        public void SetAvatarParams(VRCExpressionParameters prms) {
            VRCAvatarUtils.SetAvatarParams(avatar, prms);
        }

        [CanBeNull]
        public VRCExpressionParameters GetAvatarParams() {
            return VRCAvatarUtils.GetAvatarParams(avatar);
        }
    }
}
