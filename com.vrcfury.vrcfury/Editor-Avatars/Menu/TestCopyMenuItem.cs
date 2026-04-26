using UnityEditor;

namespace VF.Menu {
    internal static class TestCopyMenuItem {
        // SPS-NDMF: removed menu registration (non-SPS utility)
        // [MenuItem(MenuItems.testCopy, priority = MenuItems.testCopyPriority)]
        private static void RunForceRun() {
            VRCFuryTestCopyMenuItem.RunBuildTestCopy();
        }
        // SPS-NDMF: removed menu registration (non-SPS utility)
        // [MenuItem(MenuItems.testCopy, true)]
        private static bool CheckForceRun() {
            return VRCFuryTestCopyMenuItem.CheckBuildTestCopy();
        }
    }
}
