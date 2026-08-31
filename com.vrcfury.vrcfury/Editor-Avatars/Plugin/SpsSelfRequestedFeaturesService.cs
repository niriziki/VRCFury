using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VF.Builder;
using VF.Builder.Haptics;
using VF.Exceptions;
using VF.Feature;
using VF.Feature.Base;
using VF.Injector;
using VF.Model.Feature;
using VF.Service;
using VF.Utils;
using VRC.SDK3.Avatars.Components;

namespace VF.Plugin {
    /**
     * The SPS bake services ask for two upstream features through globals.addOtherFeature:
     * ShowInFirstPerson for every bake root, and TpsScaleFix for plugs using the deprecated
     * configureTps option. Both are implemented as FeatureBuilders, which SpsServiceFilter
     * rejects wholesale, so SPS silently loses features it requested for itself. The models
     * still reach globals.allFeaturesInRun, so apply them here instead.
     *
     * Only the shapes SPS actually asks for are handled: ShowInFirstPerson always arrives with
     * an object override and onlyIfChildOfHead, which upstream resolves to a head chop entry
     * and nothing else, and TpsScaleFix always names a single renderer.
     */
    [VFService]
    internal class SpsSelfRequestedFeaturesService {
        [VFAutowired] private readonly GlobalsService globals;
        [VFAutowired] private readonly VRCFArmatureCache armatureCache;
        [VFAutowired] private readonly FakeHeadService fakeHead;
        [VFAutowired] private readonly ScalePropertyCompensationService scaleCompensationService;

        [FeatureBuilderAction(FeatureOrder.TpsScaleFix)]
        public void Apply() {
            ApplyShowInFirstPerson();
            ApplyTpsScaleFix();
        }

        private void ApplyShowInFirstPerson() {
#if VRCSDK_HAS_HEAD_CHOP
            var head = armatureCache.FindBoneOnArmatureOrNull(HumanBodyBones.Head);
            if (head == null) return;

            foreach (var model in globals.allFeaturesInRun.OfType<ShowInFirstPerson>()) {
                if (!model.useObjOverride) continue;
                var obj = model.objOverride.asVf();
                if (obj == null) continue;
                if (model.onlyIfChildOfHead && !obj.IsSameOrChildOf(head)) continue;

                var headChopObj = fakeHead.GetHeadChopObj();
                var headChop = headChopObj.GetComponents<VRCHeadChop>()
                    .FirstOrDefault(c => c.targetBones.Length < 32);
                if (headChop == null) headChop = headChopObj.AddComponent<VRCHeadChop>();
                headChop.targetBones = headChop.targetBones.Append(new VRCHeadChop.HeadChopBone() {
                    transform = obj,
                    scaleFactor = 1,
                    applyCondition = VRCHeadChop.HeadChopBone.ApplyCondition.AlwaysApply
                }).ToArray();
            }
#endif
        }

        private void ApplyTpsScaleFix() {
            var renderers = new HashSet<Renderer>(globals.allFeaturesInRun
                .OfType<TpsScaleFix>()
                .Select(model => model.singleRenderer)
                .Where(renderer => renderer != null));

            foreach (var renderer in renderers) {
                var scaledProps = TpsScaleFixBuilder.GetScaledProps(renderer.sharedMaterials);
                if (scaledProps.Count == 0) continue;

                renderer.sharedMaterials = renderer.sharedMaterials.Select(mat => {
                    if (!TpsConfigurer.IsTps(mat)) return mat;
                    mat = mat.Clone("Needed to mark TPS parameters as animated for TPS Scale Fix");
                    if (TpsConfigurer.IsLocked(mat)) {
                        throw new VRCFBuilderException(
                            "TpsScaleFix requires that all deforming materials using poiyomi must be unlocked. " +
                            $"Please unlock the material on {renderer.owner().GetDebugPath()}");
                    }
                    mat.SetOverrideTag("_TPS_PenetratorLengthAnimated", "1");
                    mat.SetOverrideTag("_TPS_PenetratorScaleAnimated", "1");
                    return mat;
                }).ToArray();

                var rootBone = renderer.owner();
                if (renderer is SkinnedMeshRenderer skin && skin.rootBone != null) {
                    rootBone = skin.rootBone;
                }

                scaleCompensationService.AddScaledProp(rootBone, scaledProps.Select(p => (
                    (UnityEngine.Component)renderer,
                    $"material.{p.Key}",
                    p.Value / rootBone.worldScale.x
                )).ToList());
            }
        }
    }
}
