using UnityEngine;
using VF.Exceptions;
using VF.Utils;
using VRC.SDKBase.Editor.BuildPipeline;

namespace VF.Hooks {
    internal abstract class VrcfAvatarPreprocessor : IVRCSDKPreprocessAvatarCallback {
        // We don't run anything on MinValue because that's when StartCheck runs
        public int callbackOrder => order == int.MinValue ? int.MinValue + 1 : order;
        public bool OnPreprocessAvatar(GameObject obj) {
            // SPS-NDMF: disable all VRCFury VRCSDK preprocessor subclasses (SPS runs in NDMF Transforming phase)
            return true;
            #pragma warning disable CS0162
            var go = (VFGameObject)obj;
            if (!RunPreprocessorsOnlyOncePatch.ShouldRunPreprocessors(go)) {
                Debug.LogWarning("Skipping " + GetType().FullName + " preprocessor because preprocessors already ran on this object");
                return true;
            }
            return VRCFExceptionUtils.ErrorDialogBoundary(() => Process(obj));
            #pragma warning restore CS0162
        }

        protected abstract int order { get; }
        protected abstract void Process(VFGameObject obj);
    }
}
