using System;
using System.Collections.Generic;
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

        public static void Execute(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan, IConversionOps ops) {
            var srcPlug = vrcfToSpsNdmf ? PackageBinding.VrcfPlugType : PackageBinding.SpsNdmfPlugType;
            var dstPlug = vrcfToSpsNdmf ? PackageBinding.SpsNdmfPlugType : PackageBinding.VrcfPlugType;
            var srcSock = vrcfToSpsNdmf ? PackageBinding.VrcfSocketType : PackageBinding.SpsNdmfSocketType;
            var dstSock = vrcfToSpsNdmf ? PackageBinding.SpsNdmfSocketType : PackageBinding.VrcfSocketType;

            if (srcPlug == null || dstPlug == null || srcSock == null || dstSock == null) return;

            // Every counterpart is created before any field is copied, so a reference from one converted
            // component to another (SpsOnAction.target) resolves regardless of conversion order.
            var pending = new List<(Component src, Component dst, ConversionEntry entry)>();
            Create(root, srcPlug, dstPlug, plan, ops, pending);
            Create(root, srcSock, dstSock, plan, ops, pending);
            foreach (var (src, dst, entry) in pending) {
                var ctx = new CopyContext();
                foreach (var (s, d, _) in pending) ctx.Counterparts[s] = d;
                StructuralFieldCopier.CopyFields(src, dst, ctx, src.gameObject.name);
                entry.Warnings.AddRange(ctx.Warnings);
            }
            foreach (var (src, _, _) in pending) ops.Destroy(src);
        }

        private static void Create(GameObject root, Type srcType, Type dstType, ConversionPlan plan, IConversionOps ops,
            List<(Component, Component, ConversionEntry)> pending) {
            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = plan.Entries.Find(e => e.Target == go && e.SourceTypeName == srcType.FullName);
                if (entry == null) continue;
                if (entry.Outcome == ConversionOutcome.Skipped) continue;
                pending.Add((src, ops.AddComponent(go, dstType), entry));
            }
        }
    }
}
