using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using UnityEngine;
using VF.Component;

namespace VF.Plugin {
    /// <summary>
    /// Reports SPS configuration problems to the NDMF Console. Unlike a thrown exception,
    /// an error reported here carries a clickable reference to the offending object.
    /// </summary>
    internal static class SpsErrors {
        private const string DuplicateComponent = "spsndmf.duplicateComponent";
        private const string PlayModeContacts = "spsndmf.playModeContacts";

        private static readonly Dictionary<string, string> English = new Dictionary<string, string> {
            { DuplicateComponent, "A GameObject has {2} \"{0}\" components" },
            { DuplicateComponent + ":description", "Only one \"{0}\" is allowed per GameObject." },
            { DuplicateComponent + ":hint", "Remove the extra components from the object below." },
            { PlayModeContacts, "Every contact interacts with every other one in play mode" },
            { PlayModeContacts + ":description", "VRChat contacts do not interact in play mode on their own, so SPS forces them to. While this is in effect, contacts cannot tell \"self\" from \"others\", and contacts that have nothing to do with SPS are affected too." },
            { PlayModeContacts + ":hint", "Turn off Tools -> SPSNDMF -> Settings -> Enable SPS contacts in play mode if you do not want this." },
        };

        private static readonly Dictionary<string, string> Japanese = new Dictionary<string, string> {
            { DuplicateComponent, "1つの GameObject に「{0}」が {2} 個付いています" },
            { DuplicateComponent + ":description", "「{0}」は1つの GameObject に1つしか付けられません。" },
            { DuplicateComponent + ":hint", "下のオブジェクトから余分なコンポーネントを削除してください。" },
            { PlayModeContacts, "プレイモード中はすべてのコンタクトが相互作用します" },
            { PlayModeContacts + ":description", "VRChat のコンタクトはプレイモードでは本来相互作用しないため、SPS が反応するよう強制的に相互作用させています。この間、コンタクトの「自分」と「他人」の区別は無効になり、SPS とは無関係なコンタクトにも影響します。" },
            { PlayModeContacts + ":hint", "不要な場合は Tools → SPSNDMF → Settings → Enable SPS contacts in play mode をオフにしてください。" },
        };

        private static readonly Localizer localizer = new Localizer(
            "en",
            () => new List<(string, Func<string, string>)> {
                ("en", key => English.TryGetValue(key, out var v) ? v : null),
                ("ja", key => Japanese.TryGetValue(key, out var v) ? v : null),
            }
        );

        public static void ReportPlayModeContacts() {
            ErrorReport.ReportError(localizer, ErrorSeverity.Information, PlayModeContacts);
        }

        /// <returns>false if the avatar must not be built</returns>
        public static bool CheckDuplicateComponents(GameObject avatarObject) {
            var plugsOk = ReportDuplicates<VRCFuryHapticPlug>(avatarObject, "SPS Plug");
            var socketsOk = ReportDuplicates<VRCFuryHapticSocket>(avatarObject, "SPS Socket");
            return plugsOk && socketsOk;
        }

        private static bool ReportDuplicates<T>(GameObject avatarObject, string displayName)
            where T : UnityEngine.Component {
            var ok = true;
            foreach (var t in avatarObject.GetComponentsInChildren<Transform>(true)) {
                var count = t.GetComponents<T>().Length;
                if (count < 2) continue;
                ErrorReport.ReportError(
                    localizer,
                    ErrorSeverity.Error,
                    DuplicateComponent,
                    displayName,
                    t.gameObject,
                    count
                );
                ok = false;
            }
            return ok;
        }
    }
}
