using System.Linq;
using UnityEditor;
using VF.Exceptions;
using VF.Model;
using VF.Model.Feature;
using VF.Utils;
using VRC.SDK3.Avatars.Components;

namespace VF.Menu {
    internal static class SpsOptionsMenuItem {
        [MenuItem("GameObject/VRCFury/Create SPS Options", priority = 42)]
        [MenuItem(MenuItems.createSpsOptions, priority = MenuItems.createSpsOptionsPriority)]
        public static void Run() {
            VRCFExceptionUtils.ErrorDialogBoundary(Create);
        }

        private const string DialogTitle = "VRCFury SPS";

        private static void Create() {
            var sel = Selection.activeTransform;
            var avatar = sel != null ? sel.GetComponentInParent<VRCAvatarDescriptor>() : null;
            if (avatar == null) {
                throw new VRCFBuilderException(
                    "No avatar found. Please select your avatar (or any object inside it) first.");
            }

            var root = avatar.gameObject;
            foreach (var vf in root.GetComponents<VRCFury>()) {
                if (vf.GetAllFeatures().Any(f => f is SpsOptions)) {
                    Selection.SetActiveObjectWithContext(root, root);
                    DialogUtils.DisplayDialog(DialogTitle,
                        "SPS Options already exists on this avatar. The avatar root has been selected.",
                        "Ok");
                    return;
                }
            }

            var c = root.AddComponent<VRCFury>();
            c.content = new SpsOptions();
            VRCFury.MarkDirty(c);
            Selection.SetActiveObjectWithContext(root, root);
            DialogUtils.DisplayDialog(DialogTitle,
                "SPS Options created on the avatar root!",
                "Ok");
        }
    }
}
