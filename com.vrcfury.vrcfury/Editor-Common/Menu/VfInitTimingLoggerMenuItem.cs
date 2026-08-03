using UnityEditor;

namespace VF.Menu {
    internal static class VfInitTimingLoggerMenuItem {
        private const string EditorPref = "com.vrcfury.logVfInitTimings";

        // SPS-NDMF: removed menu registration (non-SPS utility)
        // [MenuItem(MenuItems.logVfInitTimings, priority = MenuItems.logVfInitTimingsPriority)]
        private static void Click() {
            EditorPrefs.SetBool(EditorPref, !Get());
        }

        // SPS-NDMF: removed menu registration (non-SPS utility)
        // [MenuItem(MenuItems.logVfInitTimings, true)]
        private static bool Validate() {
            UnityEditor.Menu.SetChecked(MenuItems.logVfInitTimings, Get());
            return true;
        }

        private static bool Get() {
            return EditorPrefs.GetBool(EditorPref, false);
        }
    }
}
