using UnityEditor;

namespace VF.Menu {
    internal static class SpsGizmoSelectedOnlyMenuItem {
        private const string EditorPref = "net.nrzk.spsndmf.gizmoSelectedOnly";
        private const string Path = MenuItems.settings + "Show SPS Gizmos Only When Selected";
        private const int Priority = MenuItems.settingsPriority + 109;

        public static bool Get() {
            return EditorPrefs.GetBool(EditorPref, true);
        }

        public static bool ShouldDraw(GizmoType gizmoType) {
            return !Get() || (gizmoType & GizmoType.InSelectionHierarchy) != 0;
        }

        [MenuItem(Path, priority = Priority)]
        private static void Click() {
            EditorPrefs.SetBool(EditorPref, !Get());
            SceneView.RepaintAll();
        }

        [MenuItem(Path, true)]
        private static bool Validate() {
            UnityEditor.Menu.SetChecked(Path, Get());
            return true;
        }
    }
}
