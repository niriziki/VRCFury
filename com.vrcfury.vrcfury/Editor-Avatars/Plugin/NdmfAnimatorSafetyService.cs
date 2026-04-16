using System.Collections.Generic;
using UnityEngine;
using VF.Feature.Base;
using VF.Injector;
using VF.Utils;

namespace VF.Service {
    /// <summary>
    /// Prevents SampleAnimation motorcycle pose by temporarily nulling the
    /// humanoid Avatar on all Animators during SPS processing.
    /// Unlike AnimatorHolderService (which destroys and recreates the Animator
    /// component), this preserves the component reference.
    /// </summary>
    [VFService]
    internal class NdmfAnimatorSafetyService {
        [VFAutowired] private readonly VFGameObject avatarObject;

        private readonly Dictionary<Animator, Avatar> savedAvatars = new Dictionary<Animator, Avatar>();

        [FeatureBuilderAction(FeatureOrder.ResetAnimatorBefore)]
        public void NullHumanoidAvatars() {
            foreach (var animator in avatarObject.GetComponentsInSelfAndChildren<Animator>()) {
                if (animator.avatar != null) {
                    savedAvatars[animator] = animator.avatar;
                    animator.avatar = null;
                }
            }
        }

        [FeatureBuilderAction(FeatureOrder.ResetAnimatorAfter)]
        public void RestoreHumanoidAvatars() {
            foreach (var pair in savedAvatars) {
                if (pair.Key != null) {
                    pair.Key.avatar = pair.Value;
                }
            }
        }
    }
}
