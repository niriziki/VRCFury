using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Plugin {
    /// <summary>
    /// Abstraction for avatar output (controllers, menu, params).
    /// Replaces direct VRCAvatarUtils.Set*/Get* calls in services.
    ///
    /// DescriptorAvatarOutput: reads/writes Avatar Descriptor directly (existing VRCFury behavior).
    /// NdmfAvatarOutput: records in internal buffer for MA component generation (no Descriptor writes).
    /// </summary>
    internal interface IAvatarOutput {
        /// <summary>
        /// Whether the build result is applied to the avatar in the scene as it runs.
        /// False for NDMF, where output is handed to MA components instead, so the
        /// avatar's own Animator must not be repointed at SPS-only controllers.
        /// </summary>
        bool AppliesToAvatarInPlace { get; }

        void SetAvatarController(VRCAvatarDescriptor.AnimLayerType type, RuntimeAnimatorController controller);
        (bool isDefault, RuntimeAnimatorController controller) GetAvatarController(VRCAvatarDescriptor.AnimLayerType type);
        IList<AvatarOutputController> GetAllAvatarControllers();

        void SetAvatarMenu(VRCExpressionsMenu menu);
        [CanBeNull] VRCExpressionsMenu GetAvatarMenu();

        void SetAvatarParams(VRCExpressionParameters prms);
        [CanBeNull] VRCExpressionParameters GetAvatarParams();
    }

    internal class AvatarOutputController {
        public VRCAvatarDescriptor.AnimLayerType type;
        public bool isDefault;
        public RuntimeAnimatorController controller;
    }
}
