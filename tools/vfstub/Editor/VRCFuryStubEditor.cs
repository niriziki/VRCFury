using UnityEditor;
using UnityEngine.UIElements;
using VF.Model;

namespace VF.Inspector {
    /**
     * The stub ships no feature builders, so the upstream VRCFury editor cannot render a feature list.
     * Show the stored features as plain fields so they can still be inspected and edited.
     */
    [CustomEditor(typeof(VRCFury))]
    internal class VRCFuryStubEditor : VRCFuryComponentEditor<VRCFury> {
        protected override VisualElement CreateEditor(SerializedObject serializedObject, VRCFury target) {
            var container = new VisualElement();
            container.Add(VRCFuryEditorUtils.Info(
                "VRCFury Stub keeps this component's data but does not build it." +
                " Only SPS Plug / Socket components are processed by SPSNDMF."));
            container.Add(VRCFuryEditorUtils.Prop(serializedObject.FindProperty("content")));
            return container;
        }
    }
}
