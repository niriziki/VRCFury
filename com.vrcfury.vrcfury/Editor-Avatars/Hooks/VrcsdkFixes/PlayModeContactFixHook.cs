using UnityEditor;
using VF.Menu;
using VF.Utils;
using VRC.Dynamics;

namespace VF.Hooks.VrcsdkFixes {
    /**
     * Makes "self" and "others" contacts actually work properly in play mode
     */
    internal static class PlayModeContactFixHook {
        private static int nextPlayerId = (new System.Random()).Next(1, 100_000_000);

        // SPS-NDMF: whether the override below is ours, so that builds can report it to the NDMF Console
        public static bool Applied { get; private set; }

        [VFInit]
        private static void Init() {
            // SPS-NDMF: opt-in, because this override applies to every contact in the editor
            if (!SpsPlayModeContactsMenuItem.Get()) return;
            if (ContactBase.OnValidatePlayers == null) {
                ContactBase.OnValidatePlayers = (a, b) => true;
                Applied = true;
            }
        }
        
        internal class PlayerBuilt : VrcfAvatarPreprocessor {
            protected override int order => int.MaxValue;
            protected override void Process(VFGameObject obj) {
                var playerId = nextPlayerId++;
                foreach (var contact in obj.GetComponentsInSelfAndChildren<ContactBase>()) {
                    contact.playerId = playerId;
                }
            }
        }
    }
}
