using System;
using nadena.dev.modular_avatar.core.menu;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Component {
    [AddComponentMenu("VRCFury/SPS Menus (VRCFury)")]
    internal class VRCFurySpsMenus : MenuSourceComponent {
        public bool createParentMenu = true;
        public Texture2D menuIcon;
        public bool saveSockets = false;
        public bool legacyModeEnabledOnAvatarLoad = true;

        [NonSerialized] public VRCExpressionsMenu builtMenu;
        [NonSerialized] public Texture2D builtIcon;

        public override void Visit(NodeContext context) {
            if (createParentMenu) {
                context.PushControl(new VRCExpressionsMenu.Control {
                    name = gameObject.name,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    icon = menuIcon != null ? menuIcon : builtIcon,
                    subMenu = builtMenu,
                });
            } else if (builtMenu != null) {
                context.PushMenuContents(builtMenu);
            }
        }
    }
}
