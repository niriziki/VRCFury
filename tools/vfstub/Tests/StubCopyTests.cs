using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Nrzk.SpsMigrator.Copy;
using UnityEngine;
using VF.Component;
using VF.Model;
using VF.Model.Feature;
using VF.Upgradeable;

namespace VfStubTests {
    /**
     * Data written by the stub inspector is later converted to SPSNDMF by copying every serializable field between
     * the VF and SpsNdmf twins of a type. These tests pin down what makes that safe: the twins have identical field
     * sets, a copy in each direction loses nothing, and the stub upgrades legacy data exactly as SPSNDMF does.
     */
    public class StubCopyTests : TempSceneFixture {

        [Test]
        public void EverySerializedVfTypeHasAnIdenticalSpsNdmfTwin() {
            var failures = new List<string>();
            var seen = new HashSet<Type>();
            var queue = new Queue<Type>(StubTestUtil.ConcreteSubtypes(typeof(VRCFuryComponent)));
            while (queue.Count > 0) {
                var t = queue.Dequeue();
                if (!seen.Add(t)) continue;
                var twin = StubTestUtil.Twin(t);
                if (twin == null || twin.Namespace == null || !twin.Namespace.StartsWith("SpsNdmf")) {
                    failures.Add($"{t.FullName}: no SpsNdmf twin");
                    continue;
                }
                var a = StubTestUtil.SerializableFields(t).ToDictionary(f => f.Name, f => f.FieldType);
                var b = StubTestUtil.SerializableFields(twin).ToDictionary(f => f.Name, f => f.FieldType);
                foreach (var name in a.Keys) {
                    if (!b.TryGetValue(name, out var bt)) failures.Add($"{t.FullName}.{name}: missing on {twin.FullName}");
                    else if (!SameShape(a[name], bt)) failures.Add($"{t.FullName}.{name}: {a[name]} vs {bt}");
                }
                foreach (var name in b.Keys) {
                    if (!a.ContainsKey(name)) failures.Add($"{twin.FullName}.{name}: missing on {t.FullName}");
                }
                foreach (var ft in a.Values) {
                    var inner = StubTestUtil.ElementType(ft) ?? ft;
                    if (inner.Assembly != StubTestUtil.VfAssembly) continue;
                    // Subtypes always: a [SerializeReference] base such as StateAction.Action is not abstract.
                    queue.Enqueue(inner);
                    foreach (var c in StubTestUtil.ConcreteSubtypes(inner)) queue.Enqueue(c);
                }
            }
            Assert.That(seen, Does.Contain(typeof(VF.Model.StateAction.SpsOnAction)), "type walk did not reach the action models");
            Assert.That(seen.Count, Is.GreaterThan(70), "type walk did not reach the feature and action models");
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        private static bool SameShape(Type a, Type b) {
            if (a == b) return true;
            var ea = StubTestUtil.ElementType(a);
            var eb = StubTestUtil.ElementType(b);
            if (ea != null || eb != null) return ea != null && eb != null && a.IsArray == b.IsArray && SameShape(ea, eb);
            if (a.Assembly != StubTestUtil.VfAssembly) return false;
            return a.Name == b.Name && a.DeclaringType?.Name == b.DeclaringType?.Name;
        }

        public static IEnumerable<TestCaseData> RoundTripCases() {
            foreach (var t in StubTestUtil.ConcreteSubtypes(typeof(VRCFuryComponent))) {
                if (t == typeof(VRCFury)) {
                    foreach (var f in StubTestUtil.ConcreteSubtypes(typeof(FeatureModel))) {
                        yield return new TestCaseData(t, f).SetName($"RoundTrip VRCFury({f.Name})");
                    }
                } else {
                    yield return new TestCaseData(t, null).SetName($"RoundTrip {t.Name}");
                }
            }
        }

        private Component FuzzedStub(Fuzzer fuzzer, Type componentType, Type featureType, int version = -1) {
            var stub = NewObject("stub").AddComponent(componentType);
            fuzzer.Fill(stub);
            if (featureType != null) {
                var feature = Activator.CreateInstance(featureType);
                fuzzer.Fill(feature);
                ((VRCFury)stub).content = (FeatureModel)feature;
#pragma warning disable 0612
                ((VRCFury)stub).config.features.Clear();
#pragma warning restore 0612
            }
            StubTestUtil.SetLatestVersions(stub);
            if (version >= 0) ((IUpgradeable)stub).Version = version;
            return stub;
        }

        [TestCaseSource(nameof(RoundTripCases))]
        public void CopyingToTheTwinAndBackIsLossless(Type componentType, Type featureType) {
            using (var fuzzer = new Fuzzer(1)) {
                var stub = FuzzedStub(fuzzer, componentType, featureType);
                Assert.That(fuzzer.Unfilled, Is.Empty, "the generator left fields at their defaults, so the round trip cannot vouch for them:\n" + string.Join("\n", fuzzer.Unfilled));
                AssertRoundTrip(stub, componentType);
            }
        }

        public static IEnumerable<TestCaseData> ActionCases() {
            foreach (var a in StubTestUtil.ConcreteSubtypes(typeof(VF.Model.StateAction.Action))) {
                yield return new TestCaseData(a).SetName($"RoundTrip Socket action {a.Name}");
            }
        }

        // Every action type on its own, inside a socket's depth action set: the component round trip only ever
        // reaches a couple of action types per list.
        [TestCaseSource(nameof(ActionCases))]
        public void CopyingASocketWithEachActionTypeIsLossless(Type actionType) {
            using (var fuzzer = new Fuzzer(3)) {
                var socket = NewObject("stub").AddComponent<VF.Component.VRCFuryHapticSocket>();
                var action = (VF.Model.StateAction.Action)Activator.CreateInstance(actionType);
                fuzzer.Fill(action);
                socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew { actionSet = new State { actions = { action } } });
                StubTestUtil.SetLatestVersions(socket);
                Assert.That(fuzzer.Unfilled, Is.Empty, string.Join("\n", fuzzer.Unfilled));
                AssertRoundTrip(socket, typeof(VF.Component.VRCFuryHapticSocket));
            }
        }

        private void AssertRoundTrip(Component stub, Type componentType) {
            var before = StubTestUtil.Json(stub);
            var twin = StubTestUtil.Twin(componentType);
            var ctx = new CopyContext();
            var clone = NewObject("clone").AddComponent(twin);
            StructuralFieldCopier.CopyFields(stub, clone, ctx, "to");
            var back = NewObject("back").AddComponent(componentType);
            StructuralFieldCopier.CopyFields(clone, back, ctx, "from");
            Assert.That(ctx.Warnings, Is.Empty, string.Join("\n", ctx.Warnings));
            Assert.That(StubTestUtil.Json(back), Is.EqualTo(before), "round trip changed the data");

            var clone2 = NewObject("clone2").AddComponent(twin);
            StructuralFieldCopier.CopyFields(back, clone2, ctx, "to2");
            var back2 = NewObject("back2").AddComponent(componentType);
            StructuralFieldCopier.CopyFields(clone2, back2, ctx, "from2");
            Assert.That(StubTestUtil.Json(back2), Is.EqualTo(before), "second round trip changed the data");
        }

        // Showing a component upgrades legacy data in place and lets the editors normalize it. The stub does this
        // with its own (upstream) code; the result must be what SPSNDMF's inspector produces for the same data, or a
        // component shown in the stub and one converted by the migrator would diverge.
        [TestCase(typeof(VF.Component.VRCFuryHapticSocket))]
        [TestCase(typeof(VF.Component.VRCFuryHapticPlug))]
        public void LegacyDataIsUpgradedOnDisplayTheSameWayAsSpsNdmf(Type componentType) {
            using (var fuzzer = new Fuzzer(7)) {
                var stub = FuzzedStub(fuzzer, componentType, null, version: 0);
                var twin = StubTestUtil.CopyToTwin(stub, NewObject("twin"), out var ctx);
                Assert.That(ctx.Warnings, Is.Empty, string.Join("\n", ctx.Warnings));

                var host = StubTestUtil.OpenHost(out var window);
                try {
                    StubTestUtil.Host(host, twin);
                    host.Clear();
                    StubTestUtil.Host(host, stub);
                    host.Clear();
                } finally {
                    StubTestUtil.DestroyHostedEditors();
                    StubTestUtil.CloseHost(host, window);
                }
                var expected = NewObject("expected").AddComponent(componentType);
                StructuralFieldCopier.CopyFields(twin, expected, ctx, "from");
                Assert.That(ctx.Warnings, Is.Empty, string.Join("\n", ctx.Warnings));

                Assert.That(((IUpgradeable)stub).Version, Is.EqualTo(((IUpgradeable)stub).GetLatestVersion()), "display did not upgrade the component");
                var actualJson = StubTestUtil.Json(stub);
                var expectedJson = StubTestUtil.Json(expected);
                if (actualJson != expectedJson) {
                    System.IO.File.WriteAllText($"Temp/vfstub-legacy-{componentType.Name}-expected.json", expectedJson);
                    System.IO.File.WriteAllText($"Temp/vfstub-legacy-{componentType.Name}-actual.json", actualJson);
                }
                Assert.That(actualJson, Is.EqualTo(expectedJson), "see Temp/vfstub-legacy-*.json");
            }
        }
    }
}
