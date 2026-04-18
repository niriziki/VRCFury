using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using VF.Feature;
using VF.Feature.Base;
using VF.Menu;
using VF.Model;
using VF.Model.Feature;
using VF.Updater;
using VF.Utils;

namespace VF.Hooks {
    internal static class AddComponentHook {
        private abstract class Reflection : ReflectionHelper {
            public static readonly MethodInfo MenuChangedAddHandler = typeof(UnityEditor.Menu)
                .VFEvent("menuChanged")
                ?.GetAddMethod(true);

            public delegate void RemoveMenuItem_(string path);
            public delegate void AddMenuItem_(string path, string shortcut, bool @checked, int priority, Action execute, Func<bool> validate);
            public delegate IEnumerable GetMenuItems_(string path, bool includeSeparators, bool localized);
            public static readonly RemoveMenuItem_ RemoveMenuItem = typeof(UnityEditor.Menu).GetMatchingDelegate<RemoveMenuItem_>("RemoveMenuItem");
            public static readonly AddMenuItem_ AddMenuItem = typeof(UnityEditor.Menu).GetMatchingDelegate<AddMenuItem_>("AddMenuItem");
            public static readonly GetMenuItems_ GetMenuItems = typeof(UnityEditor.Menu).GetMatchingDelegate<GetMenuItems_>("GetMenuItems");
        }

        private static bool addedThisFrame = false;
        
        [InitializeOnLoadMethod]
        private static void Init() {
            // SPS-NDMF: disabled, removes Unity standard Component menus and registers VRCFury update item
            return;
            #pragma warning disable CS0162
            EditorApplication.delayCall += AddToMenu;
            if (Reflection.MenuChangedAddHandler != null) {
                Action onMenuChange = () => {
                    if (addedThisFrame) return;
                    EditorApplication.delayCall -= AddToMenu;
                    EditorApplication.delayCall += AddToMenu;
                };
                Reflection.MenuChangedAddHandler.Invoke(null, new object[] { onMenuChange });
            }
        }

        private static void Add(string path, string shortcut, bool @checked, int priority, Action execute, Func<bool> validate) =>
            Reflection.AddMenuItem?.Invoke(path, shortcut, @checked, priority, execute, validate);
        private static void Remove(string path) => Reflection.RemoveMenuItem?.Invoke(path);
        private static IList<string> List(string path) {
            if (Reflection.GetMenuItems == null) return new string[] { };
            var l = Reflection.GetMenuItems(path, false, false);
            return l.OfType<object>()
                .Select(o => o.GetType().VFProperty("path")?.GetValue(o))
                .NotNull()
                .OfType<string>()
                .ToList();
        }

        private static void ResetAddedThisFrame() {
            addedThisFrame = false;
        }

        private static void AddToMenu() {
            //Debug.Log("Adding VRCFury components to menu");
            addedThisFrame = true;
            EditorApplication.delayCall -= ResetAddedThisFrame;
            EditorApplication.delayCall += ResetAddedThisFrame;

            if (Reflection.GetMenuItems == null) {
                Remove("Component/UI/Button");
                Remove("Component/UI/Slider");
                Remove("Component/UI/Toggle");
                Remove("Component/UI/Legacy/Dropdown");
                Remove("Component/UI/Toggle Group");
            } else {
                foreach (var path in List("Component/UI")) {
                    Remove(path);
                }
                foreach (var path in List("Component/UI Toolkit")) {
                    Remove(path);
                }
                foreach (var path in List("Component/Physics 2D")) {
                    Remove(path);
                }
            }

            if (UpdateMenuItem.ShouldShow()) {
                Add(
                    MenuItems.update,
                    "",
                    false,
                    MenuItems.updatePriority,
                    UpdateMenuItem.Upgrade,
                    null
                );
            }

            // SPS-NDMF: VRCFury feature menu items disabled.
            // SPS components (HapticPlug, HapticSocket, etc.) are registered
            // via [AddComponentMenu] attributes on their MonoBehaviour classes.
        }
    }
}

