using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nadena.dev.ndmf.localization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace VF.Prefabs {
    /**
     * The rest of this package is english only, but these strings gate an irreversible project-wide operation and
     * explain what it did to the user's data, so they are worth understanding. NDMF picks the language, which means
     * this follows whatever the user already chose for Modular Avatar.
     */
    internal static class SpsLocalization {
        private static readonly string[] languages = { "en-US", "ja-JP" };

        // Editor-Common/Prefabs/Localization
        private const string folderGuid = "76059a7146ec10982afc4c99b7fd25b7";

        private static readonly Lazy<Localizer> localizer = new Lazy<Localizer>(
            () => new Localizer(languages[0], () => languages.Select(l => (l, Load(l))).ToList()));

        public static string Get(string key) {
            return localizer.Value.GetLocalizedString(key);
        }

        public static string Get(string key, params object[] args) {
            return string.Format(Get(key), args);
        }

        private static Func<string, string> Load(string language) {
            var path = AssetDatabase.GUIDToAssetPath(folderGuid) + "/" + language + ".json";
            Dictionary<string, string> strings;
            try {
                strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
            } catch (Exception e) {
                Debug.LogWarning($"Failed to load SPS translations for {language}: {e.Message}");
                return _ => null;
            }
            return key => strings.TryGetValue(key, out var value) ? value : null;
        }
    }
}
