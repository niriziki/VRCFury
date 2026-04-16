using JetBrains.Annotations;
using VF.Builder;
using VF.Injector;
using VF.Plugin;
using VF.Utils;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Service {
    [VFService]
    internal class MenuService {
        [VFAutowired] private readonly GlobalsService globals;
        [VFAutowired] private readonly VRCAvatarDescriptor avatar;
        [VFAutowired] private readonly IAvatarOutput avatarOutput;
        
        private MenuManager _menu;
        public MenuManager GetMenu() {
            if (_menu == null) {
                var menu = VrcfObjectFactory.Create<VRCExpressionsMenu>();
                var initializing = true;
                _menu = new MenuManager(menu, () => initializing ? 0 : globals.currentMenuSortPosition);

                var origMenu = avatarOutput.GetAvatarMenu();
                if (origMenu != null) _menu.MergeMenu(origMenu);

                avatarOutput.SetAvatarMenu(menu);
                initializing = false;
            }
            return _menu;
        }

        [CanBeNull]
        public VRCExpressionsMenu GetReadOnlyMenu() {
            return avatarOutput.GetAvatarMenu();
        }
    }
}
