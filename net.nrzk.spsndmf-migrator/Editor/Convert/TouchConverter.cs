using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator.Convert {
    internal static class TouchConverter {
        private const BindingFlags FieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void PlanReceiver(GameObject root, ConversionPlan plan) {
            var srcType = PackageBinding.VrcfTouchReceiverType;
            if (srcType == null) return;

            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = new ConversionEntry {
                    Target = go,
                    SourceTypeName = srcType.FullName,
                    TargetDescription = "VRCContactReceiver x2 (Self / Others child GameObjects)",
                    Outcome = ConversionOutcome.Converted,
                };
                plan.Entries.Add(entry);
            }
        }

        public static void PlanSender(GameObject root, ConversionPlan plan) {
            var srcType = PackageBinding.VrcfTouchSenderType;
            if (srcType == null) return;

            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = src.gameObject;
                var entry = new ConversionEntry {
                    Target = go,
                    SourceTypeName = srcType.FullName,
                    TargetDescription = "VRCContactSender (Finger tag)",
                };
                if (go.GetComponent<VRCContactSender>() != null) {
                    entry.Outcome = ConversionOutcome.Skipped;
                    entry.Reason = "VRCContactSender already exists";
                } else {
                    entry.Outcome = ConversionOutcome.Converted;
                }
                plan.Entries.Add(entry);
            }
        }

        public static void ExecuteReceiver(GameObject root, ConversionPlan plan) {
            var srcType = PackageBinding.VrcfTouchReceiverType;
            if (srcType == null) return;

            var radiusField = srcType.GetField("radius", FieldFlags);
            var nameField = srcType.GetField("name", FieldFlags);

            var usedIds = new HashSet<string>();

            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = ((Component)src).gameObject;
                var entry = plan.Entries.Find(e => e.Target == go && e.SourceTypeName == srcType.FullName);
                if (entry == null || entry.Outcome == ConversionOutcome.Skipped) continue;

                var radius = (float)radiusField.GetValue(src);
                var id = ComputeId((string)nameField.GetValue(src), go, usedIds);
                var paramPrefix = $"VFH/Zone/Touch/{id.Replace('/', '_')}";

                var selfTags = HapticTagConstants.SelfContacts
                    .Concat(new[] { HapticTagConstants.CONTACT_PEN_CLOSE }).ToArray();
                var othersTags = HapticTagConstants.BodyContacts
                    .Concat(new[] { HapticTagConstants.CONTACT_PEN_CLOSE }).ToArray();

                CreateChildReceiver(go, UniqueChildName(go, "Self"),
                    radius, selfTags, paramPrefix + "/Self", party: true, others: false);
                CreateChildReceiver(go, UniqueChildName(go, "Others"),
                    radius, othersTags, paramPrefix + "/Others", party: false, others: true);

                Undo.DestroyObjectImmediate(src);
            }
        }

        public static void ExecuteSender(GameObject root, ConversionPlan plan) {
            var srcType = PackageBinding.VrcfTouchSenderType;
            if (srcType == null) return;

            var radiusField = srcType.GetField("radius", FieldFlags);

            foreach (var src in root.GetComponentsInChildren(srcType, true)) {
                var go = ((Component)src).gameObject;
                var entry = plan.Entries.Find(e => e.Target == go && e.SourceTypeName == srcType.FullName);
                if (entry == null || entry.Outcome == ConversionOutcome.Skipped) continue;

                var radius = (float)radiusField.GetValue(src);
                var sender = Undo.AddComponent<VRCContactSender>(go);
                sender.shape = ContactBase.ShapeType.Sphere;
                sender.radius = radius;
                sender.position = Vector3.zero;
                sender.collisionTags = new List<string> { "Finger" };

                Undo.DestroyObjectImmediate(src);
            }
        }

        private static void CreateChildReceiver(
            GameObject parent, string childName, float radius, string[] tags,
            string paramName, bool party, bool others) {

            var child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, "Create Touch Receiver child");
            child.transform.SetParent(parent.transform, worldPositionStays: false);

            var recv = Undo.AddComponent<VRCContactReceiver>(child);
            recv.shape = ContactBase.ShapeType.Sphere;
            recv.radius = radius;
            recv.position = Vector3.zero;
            recv.collisionTags = new List<string>(tags);
            recv.receiverType = ContactReceiver.ReceiverType.Constant;
            recv.parameter = paramName;
            recv.allowSelf = party;
            recv.allowOthers = others;
            recv.localOnly = true;
        }

        private static string ComputeId(string explicitName, GameObject owner, HashSet<string> used) {
            var id = string.IsNullOrWhiteSpace(explicitName)
                ? Sanitize(owner.name)
                : Sanitize(explicitName);
            if (string.IsNullOrEmpty(id)) id = "Zone";

            var candidate = id;
            var i = 1;
            while (!used.Add(candidate)) {
                candidate = $"{id}_{i++}";
            }
            return candidate;
        }

        private static string Sanitize(string raw) {
            if (string.IsNullOrEmpty(raw)) return "";
            return Regex.Replace(raw, "[^a-zA-Z0-9_]", "_");
        }

        private static string UniqueChildName(GameObject parent, string baseName) {
            var name = baseName;
            var i = 1;
            while (parent.transform.Find(name) != null) {
                name = $"{baseName}_{i++}";
            }
            return name;
        }
    }
}
