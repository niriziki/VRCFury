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
            if (spsCtx.AvatarOutput == null) {
                RemoveConsumedComponents(context.AvatarRootObject);
                return;
            }

            var output = spsCtx.AvatarOutput;
            var avatarObj = context.AvatarRootObject;

            var outputObj = new GameObject("SPS-NDMF Output");
            outputObj.transform.SetParent(avatarObj.transform, false);

            // Controllers → MA Merge Animator
            foreach (var kvp in output.Controllers) {
                if (kvp.Value is AnimatorController ac) {
                    PreserveFirstLayerWeight(ac);
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
                    if (spsControl != null && spsControl.subMenu.controls.Count > 0) {
                        spsMenus.builtMenu = spsControl.subMenu;
                        spsMenus.builtIcon = spsControl.icon;
                    } else if (spsControl == null && output.Menu.controls.Count > 0) {
                        // Unexpected menu shape: pass everything through unchanged.
                        spsMenus.builtMenu = output.Menu;
                    } else {
                        // Nothing to show (e.g. all menu toggles disabled):
                        // remove the component so no empty folder is emitted.
                        UnityEngine.Object.DestroyImmediate(spsMenus);
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

            RemoveConsumedComponents(avatarObj);
        }

        /**
         * Modular Avatar forces the weight of the merged controller's first layer to 1, because in a
         * standalone controller unity ignores layer 0's serialized weight. VRCFury builds controllers
         * that are merged into an existing one, so it can legitimately give its first layer a weight of
         * 0 (which CleanupEmptyLayersService does for any layer without a real clip). Prepend an empty
         * layer so the weights VRCFury chose survive the merge.
         */
        private static void PreserveFirstLayerWeight(AnimatorController controller) {
            var layers = controller.layers;
            if (layers.Length == 0 || layers[0].defaultWeight == 1) return;

            var stateMachine = new AnimatorStateMachine {
                name = "SPS Base",
                hideFlags = HideFlags.HideInHierarchy
            };
            if (UnityEditor.AssetDatabase.Contains(controller)) {
                UnityEditor.AssetDatabase.AddObjectToAsset(stateMachine, controller);
            }

            controller.layers = new[] {
                new AnimatorControllerLayer {
                    name = "SPS Base",
                    defaultWeight = 1,
                    stateMachine = stateMachine
                }
            }.Concat(layers).ToArray();
        }

        /**
         * These have already been consumed by the build. Avatar Optimizer runs later in the Optimizing
         * phase and assumes unknown components depend on everything they reference, so leaving them
         * behind for the VRChat SDK's final IEditorOnly sweep degrades its optimization.
         */
        private static void RemoveConsumedComponents(GameObject avatarObj) {
            if (avatarObj == null) return;
            foreach (var c in avatarObj.GetComponentsInChildren<VRCFuryHapticPlug>(true)) {
                UnityEngine.Object.DestroyImmediate(c);
            }
            foreach (var c in avatarObj.GetComponentsInChildren<VRCFuryHapticSocket>(true)) {
                UnityEngine.Object.DestroyImmediate(c);
            }
            foreach (var c in avatarObj.GetComponentsInChildren<VF.Model.VRCFury>(true)) {
                UnityEngine.Object.DestroyImmediate(c);
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
