using UnityEditor;
using VF.Utils;

namespace VF.Menu {
    /**
     * While enabled, every contact in the editor interacts with every other one during play mode,
     * which is what lets SPS react while testing. The tradeoff is that "self" and "others" become
     * indistinguishable there, and the override is not limited to contacts that SPS created.
     */
    internal static class SpsPlayModeContactsMenuItem {
        private const string EditorPref = "com.vrcfury.spsPlayModeContacts";
        private const string MenuPath = "Tools/VRCFury/Settings/Enable SPS contacts in play mode";

        public static bool Get() {
            return EditorPrefs.GetBool(EditorPref, true);
        }

        [MenuItem(MenuPath, priority = 1414)]
        private static void Click() {
            if (Get()) {
                var ok = DialogUtils.DisplayDialog(
                    "Warning",
                    "VRChat contacts do not interact in play mode on their own, so disabling this will" +
                    " stop SPS from reacting while you test in the editor. Are you sure you want to continue?",
                    "Yes, disable SPS contacts in play mode",
                    "Cancel"
                );
                if (!ok) return;
            }
            EditorPrefs.SetBool(EditorPref, !Get());
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate() {
            UnityEditor.Menu.SetChecked(MenuPath, Get());
            return true;
        }
    }
}
