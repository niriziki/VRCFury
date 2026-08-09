using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VF.Builder;
using VF.Builder.Haptics;
using VF.Component;
using VF.Menu;
using VF.Utils;
using VRC.SDK3.Avatars.Components;
using UnityEngine.SceneManagement;

namespace VF {
    internal static class PlayModeTrigger {
        /**
         * SPS-NDMF: bakes outside of an avatar run without an NDMF build, so they have no asset
         * container to write to. They get this directory instead, next to NDMF's own bake output.
         */
        private const string DetachedTempDir = "Assets/ZZZ_GeneratedAssets/spsndmf-playmode";

        [VFInit]
        private static void Init() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingEditMode) {
                AssetDatabase.DeleteAsset(DetachedTempDir);
            }
        }

        internal class SceneProcessor : IProcessSceneWithReport {
            public int callbackOrder => int.MinValue + 100;

            public void OnProcessScene(Scene scene, BuildReport report) {
                if (!Application.isPlaying) return;
                if (!PlayModeMenuItem.Get()) return;
                ProcessScene(scene);
            }
        }

        private static void ProcessScene(Scene scene) {
            var activeSockets = new List<VRCFuryHapticSocket>();
            var activePlugs = new List<VRCFuryHapticPlug>();
            foreach (var rootObj in scene.GetRootGameObjects()) {
                ProcessTree(rootObj, activeSockets, activePlugs);
            }
            ProcessSps(activeSockets, activePlugs);
        }

        private static void ProcessTree(
            VFGameObject obj,
            List<VRCFuryHapticSocket> activeSockets,
            List<VRCFuryHapticPlug> activePlugs
        ) {
            if (obj == null) return;
            if (IsAv3EmulatorClone(obj)) return;

            // SPS-NDMF: avatars are handled by NDMF, only the rest of the scene is baked here
            var avatar = obj.GetComponent<VRCAvatarDescriptor>();
            if (avatar != null) return;

            var socket = obj.GetComponent<VRCFuryHapticSocket>();
            if (socket != null) {
                if (obj.activeInHierarchy) {
                    activeSockets.Add(socket);
                    return;
                }
                ProcessOnStartComponent.Process(obj, () => {
                    if (socket != null) {
                        ProcessSps(new[] { socket }, Array.Empty<VRCFuryHapticPlug>());
                    }
                });
                return;
            }

            var plug = obj.GetComponent<VRCFuryHapticPlug>();
            if (plug != null) {
                if (obj.activeInHierarchy) {
                    activePlugs.Add(plug);
                    return;
                }
                ProcessOnStartComponent.Process(obj, () => {
                    if (plug != null) {
                        ProcessSps(Array.Empty<VRCFuryHapticSocket>(), new[] { plug });
                    }
                });
                return;
            }

            foreach (var child in obj.Children()) {
                ProcessTree(child, activeSockets, activePlugs);
            }
        }

        private static void ProcessSps(
            IList<VRCFuryHapticSocket> sockets,
            IList<VRCFuryHapticPlug> plugs
        ) {
            VRCFuryBuildContext.Run(() => {
                TmpFilePackage.TmpDirPath = DetachedTempDir;
                SpsDetachedBakeAndSave.Run(sockets, plugs);
            });
        }

        public static bool IsAv3EmulatorClone(VFGameObject obj) {
            return obj.name.Contains("(ShadowClone)")
                   || obj.name.Contains("(MirrorReflection)");
        }

        [DefaultExecutionOrder(-10000)]
        public class ProcessOnStartComponent : VRCFuryPlayComponent {
            public Action action;

            private void Start() {
                if (!Application.isPlaying || !PlayModeMenuItem.Get()) return;
                try {
                    action?.Invoke();
                } finally {
                    DestroyImmediate(this);
                }
            }

            public static void Process(VFGameObject obj, Action action) {
                if (!Application.isPlaying) return;
                if (obj == null) return;
                if (obj.activeInHierarchy) {
                    action?.Invoke();
                    return;
                }
                var component = obj.AddComponent<ProcessOnStartComponent>();
                component.action = action;
            }
        }
    }
}
