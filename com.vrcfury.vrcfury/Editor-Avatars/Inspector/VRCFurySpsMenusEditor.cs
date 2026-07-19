using nadena.dev.modular_avatar.core;
using nadena.dev.modular_avatar.core.menu;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VF.Component;

namespace VF.Inspector {
    [CustomEditor(typeof(VRCFurySpsMenus))]
    internal class VRCFurySpsMenusEditor : UnityEditor.Editor {
        public override VisualElement CreateInspectorGUI() {
            var c = new VisualElement();
            c.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox(
                "SPS Menus places the SPS menu, working like a MA Menu Item:" +
                " its menu position is where this object sits in the menu tree," +
                " and the submenu is named after this object.",
                MessageType.None)));
            c.Add(VRCFuryEditorUtils.Prop(serializedObject.FindProperty("createParentMenu"),
                "Create Parent Menu (submenu named after this object)"));
            c.Add(VRCFuryEditorUtils.Prop(serializedObject.FindProperty("menuIcon"),
                "Menu Icon (used when Create Parent Menu is on)"));
            c.Add(VRCFuryEditorUtils.Prop(serializedObject.FindProperty("saveSockets"),
                "Save Sockets Between Worlds"));
            c.Add(VRCFuryEditorUtils.Prop(serializedObject.FindProperty("legacyModeEnabledOnAvatarLoad"),
                "Legacy Mode enabled on avatar load"));
            c.Add(new IMGUIContainer(DrawConnectionUi));
            return c;
        }

        private void DrawConnectionUi() {
            var self = (VRCFurySpsMenus)target;
            if (self == null) return;
            var go = self.gameObject;
            var hasInstaller = go.GetComponent<ModularAvatarMenuInstaller>() != null;
            var parentIsMenu = go.transform.parent != null
                               && go.transform.parent.GetComponent<MenuSource>() != null;
            if (hasInstaller && parentIsMenu) {
                EditorGUILayout.HelpBox(
                    "This object is inside a menu tree AND has a MA Menu Installer," +
                    " so the SPS menu may appear twice. Remove the MA Menu Installer.",
                    MessageType.Warning);
            } else if (!hasInstaller && !parentIsMenu) {
                EditorGUILayout.HelpBox(
                    "This object is not connected to the avatar's menu." +
                    " Move it under a MA Menu Item, or add a MA Menu Installer" +
                    " to install it to the menu root.",
                    MessageType.Info);
                if (GUILayout.Button("Add MA Menu Installer")) {
                    Undo.AddComponent<ModularAvatarMenuInstaller>(go);
                }
            }
        }
    }
}
