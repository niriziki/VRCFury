#if SPSNDMF_HAS_AVATAR_OPTIMIZER
using Anatawa12.AvatarOptimizer.API;
using VF.Component;

namespace VF.Integration.AvatarOptimizer {
    /**
     * These components are created during the build and must survive until Start() runs in play mode,
     * so they cannot be removed before Avatar Optimizer's Optimizing phase. Registering them here keeps
     * Avatar Optimizer from treating them as unknown components, which would degrade its optimization.
     */
    [ComponentInformation(typeof(VRCFurySpsGreenScreenFix))]
    internal class VRCFurySpsGreenScreenFixInformation : ComponentInformation<VRCFurySpsGreenScreenFix> {
        protected override void CollectDependency(VRCFurySpsGreenScreenFix component, ComponentDependencyCollector collector) {
            collector.AddDependency(component, component.transform).EvenIfDependantDisabled();
        }
    }

    [ComponentInformation(typeof(VRCFurySocketGizmo))]
    internal class VRCFurySocketGizmoInformation : ComponentInformation<VRCFurySocketGizmo> {
        protected override void CollectDependency(VRCFurySocketGizmo component, ComponentDependencyCollector collector) {
            collector.AddDependency(component, component.transform).EvenIfDependantDisabled();
        }
    }

    [ComponentInformation(typeof(VRCFuryHideGizmoUnlessSelected))]
    internal class VRCFuryHideGizmoUnlessSelectedInformation : ComponentInformation<VRCFuryHideGizmoUnlessSelected> {
        protected override void CollectDependency(VRCFuryHideGizmoUnlessSelected component, ComponentDependencyCollector collector) {
            collector.AddDependency(component, component.transform).EvenIfDependantDisabled();
        }
    }

    [ComponentInformation(typeof(VRCFuryNoUpdateWhenOffscreen))]
    internal class VRCFuryNoUpdateWhenOffscreenInformation : ComponentInformation<VRCFuryNoUpdateWhenOffscreen> {
        protected override void CollectDependency(VRCFuryNoUpdateWhenOffscreen component, ComponentDependencyCollector collector) {
            collector.AddDependency(component, component.transform).EvenIfDependantDisabled();
        }
    }
}
#endif
