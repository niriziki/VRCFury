using System.Reflection;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator.Convert {
    internal static class GlobalColliderConverter {
        private const BindingFlags FieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void Plan(GameObject root, ConversionPlan plan) {
            var srcType = PackageBinding.VrcfGlobalColliderType;
            if (srcType == null) return;

            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = new ConversionEntry {
                    Target = go,
                    SourceTypeName = srcType.FullName,
                    TargetDescription = nameof(ModularAvatarGlobalCollider),
                };
                if (go.GetComponent<ModularAvatarGlobalCollider>() != null) {
                    entry.Outcome = ConversionOutcome.Skipped;
                    entry.Reason = "ModularAvatarGlobalCollider already exists";
                } else {
                    entry.Outcome = ConversionOutcome.Converted;
                }
                plan.Entries.Add(entry);
            }
        }

        public static void Execute(GameObject root, ConversionPlan plan) {
            var srcType = PackageBinding.VrcfGlobalColliderType;
            if (srcType == null) return;

            var radiusField = srcType.GetField("radius", FieldFlags);
            var heightField = srcType.GetField("height", FieldFlags);
            var rootTransformField = srcType.GetField("rootTransform", FieldFlags);

            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = plan.Entries.Find(e => e.Target == go && e.SourceTypeName == srcType.FullName);
                if (entry == null || entry.Outcome == ConversionOutcome.Skipped) continue;

                var dst = Undo.AddComponent<ModularAvatarGlobalCollider>(go);
                dst.Radius = (float)radiusField.GetValue(src);
                dst.Height = (float)heightField.GetValue(src);
                dst.RootTransform = (Transform)rootTransformField.GetValue(src);
                dst.ManualRemap = false;

                Undo.DestroyObjectImmediate(src);
            }
        }
    }
}
