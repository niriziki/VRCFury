using UnityEditor;
using UnityEngine.UIElements;
using VF.Component;
using VF.Model;

namespace VF.Inspector {
    /**
     * Components whose upstream editor needs the feature builders (not shipped in the stub) are shown as plain
     * fields, so their data can still be inspected and edited. Everything else keeps its upstream editor.
     */
    internal abstract class VRCFuryStubPlainEditor<T> : VRCFuryComponentEditor<T> where T : VRCFuryComponent {
        protected override VisualElement CreateEditor(SerializedObject serializedObject, T target) {
            var container = new VisualElement();
            container.Add(VRCFuryEditorUtils.Info(
                "VRCFury Stub keeps this component's data but does not build it." +
                " Only SPS Plug / Socket components are processed by SPSNDMF."));
            var prop = serializedObject.GetIterator();
            for (var enter = true; prop.NextVisible(enter); enter = false) {
                if (prop.propertyPath == "m_Script") continue;
                container.Add(VRCFuryEditorUtils.Prop(prop.Copy()));
            }
            return container;
        }
    }

    [CustomEditor(typeof(VRCFury))]
    internal class VRCFuryStubEditor : VRCFuryStubPlainEditor<VRCFury> {
    }

    [CustomEditor(typeof(VRCFuryDebugInfo))]
    internal class VRCFuryDebugInfoStubEditor : VRCFuryStubPlainEditor<VRCFuryDebugInfo> {
    }

    [CustomEditor(typeof(VRCFuryTest))]
    internal class VRCFuryTestStubEditor : VRCFuryStubPlainEditor<VRCFuryTest> {
    }
}
