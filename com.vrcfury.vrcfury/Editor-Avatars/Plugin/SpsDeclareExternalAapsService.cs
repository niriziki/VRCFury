using System.Collections.Generic;
using System.Linq;
using VF.Feature.Base;
using VF.Injector;
using VF.Service;
using VF.Utils;

namespace VF.Plugin {
    /**
     * Upstream VRCFury builds into the avatar's own FX controller, so an AAP written by
     * "Set an FX Float" (or by an AAP curve inside a user's Animation Clip) always finds
     * its parameter already declared. SPS-NDMF builds into an isolated controller where
     * only SPS-owned parameters exist, so RemoveWrongParamTypes deletes those curves as
     * writes to a non-float parameter. Declare the missing ones as floats; Modular Avatar
     * keeps the avatar's own declaration when merging if it already has one.
     *
     * Must run after FixPartiallyWeightedAaps, which would otherwise pin these parameters
     * to a default in the write-defaults clip. Weight protection does not depend on the
     * declaration, so it still applies.
     */
    [VFService]
    internal class SpsDeclareExternalAapsService {
        [VFAutowired] private readonly ControllersService controllers;
        private ControllerManager fx => controllers.GetFx();

        private readonly List<string> declared = new List<string>();

        /** Parameters SPS drives but does not own, for passes running after the build. */
        public IReadOnlyList<string> Declared => declared;

        [FeatureBuilderAction(FeatureOrder.DeclareExternalAaps)]
        public void Apply() {
            var aaps = new AnimatorIterator.Clips().From(fx)
                .SelectMany(clip => clip.GetFloatBindings())
                .Where(binding => binding.GetPropType() == EditorCurveBindingType.Aap)
                .Select(binding => binding.propertyName)
                .Distinct();
            foreach (var aap in aaps) {
                if (fx.GetParam(aap) != null) continue;
                fx.NewFloat(aap, usePrefix: false);
                declared.Add(aap);
            }
        }
    }
}
