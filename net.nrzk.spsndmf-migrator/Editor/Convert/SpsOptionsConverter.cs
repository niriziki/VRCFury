using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Nrzk.SpsMigrator.Copy;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator.Convert {
    internal static class SpsOptionsConverter {
        private const BindingFlags FieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void Plan(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan) {
            var (srcContainer, srcOptions, dstContainer, dstOptions) = Bind(vrcfToSpsNdmf);
            if (srcContainer == null) return;

            foreach (var src in FindContainers(root, srcContainer, srcOptions)) {
                var go = src.gameObject;
                var entry = new ConversionEntry {
                    Target = go,
                    SourceTypeName = srcOptions.FullName,
                    TargetDescription = dstOptions.FullName,
                };
                if (FindContainerOn(go, dstContainer, dstOptions) != null) {
                    entry.Outcome = ConversionOutcome.Skipped;
                    entry.Reason = $"{dstOptions.Name} already exists on this GameObject";
                } else {
                    entry.Outcome = ConversionOutcome.Converted;
                }
                plan.Entries.Add(entry);
            }
        }

        public static void Execute(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan, IConversionOps ops) {
            var (srcContainer, srcOptions, dstContainer, dstOptions) = Bind(vrcfToSpsNdmf);
            if (srcContainer == null) return;

            foreach (var src in FindContainers(root, srcContainer, srcOptions)) {
                var go = src.gameObject;
                var entry = plan.Entries.Find(e => e.Target == go && e.SourceTypeName == srcOptions.FullName);
                if (entry == null) continue;
                if (entry.Outcome == ConversionOutcome.Skipped) continue;

                var dst = ops.AddComponent(go, dstContainer);
                var ctx = new CopyContext();
                StructuralFieldCopier.CopyFields(src, dst, ctx, go.name);
                entry.Warnings.AddRange(ctx.Warnings);

                ops.Destroy(src);
            }
        }

        private static (Type srcContainer, Type srcOptions, Type dstContainer, Type dstOptions) Bind(bool vrcfToSpsNdmf) {
            if (!PackageBinding.SpsOptionsConvertible) return (null, null, null, null);
            return vrcfToSpsNdmf
                ? (PackageBinding.VrcfVrcFuryType, PackageBinding.VrcfSpsOptionsType,
                    PackageBinding.SpsNdmfVrcFuryType, PackageBinding.SpsNdmfSpsOptionsType)
                : (PackageBinding.SpsNdmfVrcFuryType, PackageBinding.SpsNdmfSpsOptionsType,
                    PackageBinding.VrcfVrcFuryType, PackageBinding.VrcfSpsOptionsType);
        }

        private static List<Component> FindContainers(GameObject root, Type containerType, Type optionsType) {
            var result = new List<Component>();
            foreach (var c in root.GetComponentsInChildren(containerType, true)) {
                if (GetContent(c)?.GetType() == optionsType) result.Add(c);
            }
            return result;
        }

        private static Component FindContainerOn(GameObject go, Type containerType, Type optionsType) {
            foreach (var c in go.GetComponents(containerType)) {
                if (GetContent(c)?.GetType() == optionsType) return c;
            }
            return null;
        }

        private static object GetContent(Component container) {
            var f = container.GetType().GetField("content", FieldFlags);
            return f?.GetValue(container);
        }
    }
}
