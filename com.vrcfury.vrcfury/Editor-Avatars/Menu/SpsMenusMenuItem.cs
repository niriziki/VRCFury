using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using VF.Component;
using VF.Exceptions;
using VF.Utils;
using VRC.SDK3.Avatars.Components;

namespace VF.Menu {
    internal static class SpsMenusMenuItem {
        [MenuItem("GameObject/VRCFury/Create SPS Menus", priority = 42)]
        [MenuItem("Tools/VRCFury/SPS/Create SPS Menus", priority = 1313)]
        public static void Run() {
            VRCFExceptionUtils.ErrorDialogBoundary(Create);
        }

        private const string DialogTitle = "VRCFury SPS";

        private static void Create() {
            var sel = Selection.activeTransform;
            var avatar = sel != null ? sel.GetComponentInParent<VRCAvatarDescriptor>(true) : null;
            if (avatar == null) {
                throw new VRCFBuilderException(
                    "No avatar found. Please select your avatar (or any object inside it) first.");
            }

            var root = avatar.gameObject;
            var existing = root.GetComponentInChildren<VRCFurySpsMenus>(true);
            if (existing != null) {
                Selection.SetActiveObjectWithContext(existing.gameObject, existing.gameObject);
                DialogUtils.DisplayDialog(DialogTitle,
                    "SPS Menus already exists on this avatar. The existing object has been selected.",
                    "Ok");
                return;
            }

            var obj = new GameObject("SPS");
            obj.transform.SetParent(root.transform, false);
            obj.AddComponent<VRCFurySpsMenus>();
            obj.AddComponent<ModularAvatarMenuInstaller>();
            Undo.RegisterCreatedObjectUndo(obj, "Create SPS Menus");
            Selection.SetActiveObjectWithContext(obj, obj);
            DialogUtils.DisplayDialog(DialogTitle,
                "SPS Menus created!\n\nBy default the SPS menu is installed to your avatar's menu root." +
                " To place it inside an existing menu, move the created object under a MA Menu Item" +
                " and remove the MA Menu Installer.",
                "Ok");
        }
    }
}
