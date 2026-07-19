using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;
using UnityEngine;
using VF.Component;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Plugin.Passes {
    internal class SpsOutputPass : Pass<SpsOutputPass> {
        public override string DisplayName => "SPS Output (MA Components)";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            if (spsCtx.AvatarOutput == null) return;

            var output = spsCtx.AvatarOutput;
            var avatarObj = context.AvatarRootObject;

            var outputObj = new GameObject("SPS-NDMF Output");
            outputObj.transform.SetParent(avatarObj.transform, false);

            // Controllers → MA Merge Animator
            foreach (var kvp in output.Controllers) {
                if (kvp.Value is AnimatorController ac) {
                    var merge = outputObj.AddComponent<ModularAvatarMergeAnimator>();
                    merge.animator = ac;
                    merge.layerType = kvp.Key;
                    merge.pathMode = MergeAnimatorPathMode.Absolute;
                    merge.matchAvatarWriteDefaults = true;
                }
            }

            // Menu → SPS Menus component if present, else MA Menu Installer.
            // The upstream pipeline generated everything under a root-level
            // "SPS" folder (default menuPath); hand its contents to the component.
            if (output.Menu != null) {
                var spsMenus = avatarObj.GetComponentInChildren<VRCFurySpsMenus>(true);
                if (spsMenus != null) {
                    var spsControl = output.Menu.controls.FirstOrDefault(c =>
                        c.type == VRCExpressionsMenu.Control.ControlType.SubMenu
                        && c.subMenu != null
                        && c.name == "SPS");
                    if (spsControl != null) {
                        spsMenus.builtMenu = spsControl.subMenu;
                        spsMenus.builtIcon = spsControl.icon;
                    } else {
                        // Unexpected menu shape: pass everything through unchanged.
                        spsMenus.builtMenu = output.Menu;
                    }
                } else {
                    var installer = outputObj.AddComponent<ModularAvatarMenuInstaller>();
                    installer.menuToAppend = output.Menu;
                }
            }

            // Params → MA Parameters
            if (output.Params != null) {
                var maParams = outputObj.AddComponent<ModularAvatarParameters>();
                foreach (var param in output.Params.parameters) {
                    maParams.parameters.Add(new ParameterConfig {
                        nameOrPrefix = param.name,
                        syncType = ConvertSyncType(param.valueType),
                        defaultValue = param.defaultValue,
                        saved = param.saved,
                        localOnly = !param.networkSynced
                    });
                }
            }
        }

        private static ParameterSyncType ConvertSyncType(
            VRCExpressionParameters.ValueType vrcType) {
            switch (vrcType) {
                case VRCExpressionParameters.ValueType.Bool: return ParameterSyncType.Bool;
                case VRCExpressionParameters.ValueType.Int: return ParameterSyncType.Int;
                case VRCExpressionParameters.ValueType.Float: return ParameterSyncType.Float;
                default: return ParameterSyncType.NotSynced;
            }
        }
    }
}
