using System;
using UnityEditor;
using UnityEngine;
using Nrzk.SpsMigrator.Copy;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator.Convert {
    internal static class PlugSocketConverter {
        public static void Plan(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan) {
            var srcPlug = vrcfToSpsNdmf ? PackageBinding.VrcfPlugType : PackageBinding.SpsNdmfPlugType;
            var dstPlug = vrcfToSpsNdmf ? PackageBinding.SpsNdmfPlugType : PackageBinding.VrcfPlugType;
            var srcSock = vrcfToSpsNdmf ? PackageBinding.VrcfSocketType : PackageBinding.SpsNdmfSocketType;
            var dstSock = vrcfToSpsNdmf ? PackageBinding.SpsNdmfSocketType : PackageBinding.VrcfSocketType;

            if (srcPlug == null || dstPlug == null || srcSock == null || dstSock == null) return;

            PlanOne(root, srcPlug, dstPlug, plan);
            PlanOne(root, srcSock, dstSock, plan);
        }

        private static void PlanOne(GameObject root, Type srcType, Type dstType, ConversionPlan plan) {
            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = new ConversionEntry {
                    Target = go,
                    SourceTypeName = srcType.FullName,
                    TargetDescription = dstType.FullName,
                };

                if (go.GetComponent(dstType) != null) {
                    entry.Outcome = ConversionOutcome.Skipped;
                    entry.Reason = $"{dstType.Name} already exists on this GameObject";
                } else {
                    entry.Outcome = ConversionOutcome.Converted;
                }
                plan.Entries.Add(entry);
            }
        }

        public static void Execute(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan) {
            var srcPlug = vrcfToSpsNdmf ? PackageBinding.VrcfPlugType : PackageBinding.SpsNdmfPlugType;
            var dstPlug = vrcfToSpsNdmf ? PackageBinding.SpsNdmfPlugType : PackageBinding.VrcfPlugType;
            var srcSock = vrcfToSpsNdmf ? PackageBinding.VrcfSocketType : PackageBinding.SpsNdmfSocketType;
            var dstSock = vrcfToSpsNdmf ? PackageBinding.SpsNdmfSocketType : PackageBinding.VrcfSocketType;

            if (srcPlug == null || dstPlug == null || srcSock == null || dstSock == null) return;

            ExecuteOne(root, srcPlug, dstPlug, plan);
            ExecuteOne(root, srcSock, dstSock, plan);
        }

        private static void ExecuteOne(GameObject root, Type srcType, Type dstType, ConversionPlan plan) {
            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = plan.Entries.Find(e => e.Target == go && e.SourceTypeName == srcType.FullName);
                if (entry == null) continue;
                if (entry.Outcome == ConversionOutcome.Skipped) continue;

                var dst = Undo.AddComponent(go, dstType);
                var ctx = new CopyContext();
                StructuralFieldCopier.CopyFields(src, dst, ctx, go.name);
                entry.Warnings.AddRange(ctx.Warnings);

                Undo.DestroyObjectImmediate(src);
            }
        }
    }
}
