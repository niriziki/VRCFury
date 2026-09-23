using System.Linq;
using NUnit.Framework;
using Nrzk.SpsMigrator.Convert;
using Nrzk.SpsMigrator.Reflection;
using UnityEditor;
using UnityEngine;
using VF.Component;
using VF.Model;
using VF.Model.StateAction;
using Object = UnityEngine.Object;

namespace VfStubTests {
    /**
     * A socket or plug can carry an "SPS On" action that references a plug component. The migrator converts
     * components by copying fields, and that reference must end up pointing at the converted plug rather than
     * being dropped.
     */
    public class MigratorReferenceTests : TempSceneFixture {
        private GameObject root;

        [SetUp]
        public void SetUp() {
            root = new GameObject("root");
        }

        private GameObject Child(string name) {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform);
            return go;
        }

        private static Object Target(Component c, string path) {
            return new SerializedObject(c).FindProperty(path).objectReferenceValue;
        }

        private const string SocketTargetPath = "depthActions2.Array.data[0].actionSet.actions.Array.data[0].target";
        private const string PlugTargetPath = "postBakeActions.actions.Array.data[0].target";

        [Test]
        public void SpsOnTargetsFollowTheConvertedPlugInBothDirections() {
            var plugGo = Child("plug");
            var otherPlugGo = Child("otherPlug");
            var socketGo = Child("socket");
            var plug = plugGo.AddComponent<VF.Component.VRCFuryHapticPlug>();
            var otherPlug = otherPlugGo.AddComponent<VF.Component.VRCFuryHapticPlug>();
            var socket = socketGo.AddComponent<VF.Component.VRCFuryHapticSocket>();
            socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew {
                actionSet = new State { actions = { new SpsOnAction { target = plug } } }
            });
            // A plug referencing another plug exercises the order-independence of the conversion.
            plug.postBakeActions = new State { actions = { new SpsOnAction { target = otherPlug } } };
            foreach (var c in new Component[] { plug, otherPlug, socket }) StubTestUtil.SetLatestVersions(c);

            var plan = new ConversionPlan();
            PlugSocketConverter.Plan(root, vrcfToSpsNdmf: true, plan);
            PlugSocketConverter.Execute(root, vrcfToSpsNdmf: true, plan, new BuildConversionOps());
            Assert.That(plan.Entries.SelectMany(e => e.Warnings), Is.Empty, string.Join("\n", plan.Entries.SelectMany(e => e.Warnings)));

            var spsSocket = socketGo.GetComponent(PackageBinding.SpsNdmfSocketType);
            var spsPlug = plugGo.GetComponent(PackageBinding.SpsNdmfPlugType);
            var spsOtherPlug = otherPlugGo.GetComponent(PackageBinding.SpsNdmfPlugType);
            Assert.That(socketGo.GetComponent<VF.Component.VRCFuryHapticSocket>() == null, "source socket was not removed");
            Assert.That(Target(spsSocket, SocketTargetPath), Is.EqualTo(spsPlug), "socket lost its SPS On target");
            Assert.That(Target(spsPlug, PlugTargetPath), Is.EqualTo(spsOtherPlug), "plug lost its SPS On target");

            var back = new ConversionPlan();
            PlugSocketConverter.Plan(root, vrcfToSpsNdmf: false, back);
            PlugSocketConverter.Execute(root, vrcfToSpsNdmf: false, back, new BuildConversionOps());
            Assert.That(back.Entries.SelectMany(e => e.Warnings), Is.Empty);
            var vfSocket = socketGo.GetComponent<VF.Component.VRCFuryHapticSocket>();
            var vfPlug = plugGo.GetComponent<VF.Component.VRCFuryHapticPlug>();
            var vfOtherPlug = otherPlugGo.GetComponent<VF.Component.VRCFuryHapticPlug>();
            Assert.That(Target(vfSocket, SocketTargetPath), Is.EqualTo(vfPlug));
            Assert.That(Target(vfPlug, PlugTargetPath), Is.EqualTo(vfOtherPlug));
        }

        // Two plugs on one GameObject: each reference must follow its own plug, not the first counterpart found.
        [Test]
        public void ReferencesDistinguishPlugsOnTheSameGameObject() {
            var plugGo = Child("plugs");
            var first = plugGo.AddComponent<VF.Component.VRCFuryHapticPlug>();
            var second = plugGo.AddComponent<VF.Component.VRCFuryHapticPlug>();
            var socketGo = Child("socket");
            var socket = socketGo.AddComponent<VF.Component.VRCFuryHapticSocket>();
            socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew {
                actionSet = new State { actions = { new SpsOnAction { target = second } } }
            });
            foreach (var c in new Component[] { first, second, socket }) StubTestUtil.SetLatestVersions(c);
            second.length = 0.25f;

            var plan = new ConversionPlan();
            PlugSocketConverter.Plan(root, vrcfToSpsNdmf: true, plan);
            PlugSocketConverter.Execute(root, vrcfToSpsNdmf: true, plan, new BuildConversionOps());
            Assert.That(plan.Entries.SelectMany(e => e.Warnings), Is.Empty);

            var target = Target(socketGo.GetComponent(PackageBinding.SpsNdmfSocketType), SocketTargetPath);
            Assert.That(target, Is.Not.Null);
            Assert.That(new SerializedObject(target).FindProperty("length").floatValue, Is.EqualTo(0.25f), "the reference followed the wrong plug");
        }

        // A plug skipped because its counterpart already exists is not part of the conversion, so a reference
        // to it is reported rather than redirected to the unrelated existing component.
        [Test]
        public void ReferenceToASkippedPlugIsReportedNotRedirected() {
            var plugGo = Child("plug");
            plugGo.AddComponent(PackageBinding.SpsNdmfPlugType);
            var skipped = plugGo.AddComponent<VF.Component.VRCFuryHapticPlug>();
            var socketGo = Child("socket");
            var socket = socketGo.AddComponent<VF.Component.VRCFuryHapticSocket>();
            socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew {
                actionSet = new State { actions = { new SpsOnAction { target = skipped } } }
            });
            foreach (var c in new Component[] { skipped, socket }) StubTestUtil.SetLatestVersions(c);

            var plan = new ConversionPlan();
            PlugSocketConverter.Plan(root, vrcfToSpsNdmf: true, plan);
            PlugSocketConverter.Execute(root, vrcfToSpsNdmf: true, plan, new BuildConversionOps());
            var warnings = plan.Entries.SelectMany(e => e.Warnings).ToList();
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Path, Does.EndWith(".target"));
            Assert.That(Target(socketGo.GetComponent(PackageBinding.SpsNdmfSocketType), SocketTargetPath), Is.Null);
        }

        [Test]
        public void ReferenceToAPlugOutsideTheConversionIsReportedNotSilentlyDropped() {
            var outside = new GameObject("outside").AddComponent<VF.Component.VRCFuryHapticPlug>();
            try {
                var socketGo = Child("socket");
                var socket = socketGo.AddComponent<VF.Component.VRCFuryHapticSocket>();
                socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew {
                    actionSet = new State { actions = { new SpsOnAction { target = outside } } }
                });
                StubTestUtil.SetLatestVersions(socket);

                var plan = new ConversionPlan();
                PlugSocketConverter.Plan(root, vrcfToSpsNdmf: true, plan);
                PlugSocketConverter.Execute(root, vrcfToSpsNdmf: true, plan, new BuildConversionOps());
                var warnings = plan.Entries.SelectMany(e => e.Warnings).ToList();
                Assert.That(warnings, Has.Count.EqualTo(1));
                Assert.That(warnings[0].Path, Does.EndWith(".target"));
                Assert.That(Target(socketGo.GetComponent(PackageBinding.SpsNdmfSocketType), SocketTargetPath), Is.Null);
            } finally {
                Object.DestroyImmediate(outside.gameObject);
            }
        }

        [Test]
        public void ReferenceToADestroyedPlugIsReportedNotThrown() {
            var socket = Child("socket").AddComponent<VF.Component.VRCFuryHapticSocket>();
            var doomed = Child("doomed").AddComponent<VF.Component.VRCFuryHapticPlug>();
            socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew {
                actionSet = new State { actions = { new SpsOnAction { target = doomed } } }
            });
            StubTestUtil.SetLatestVersions(socket);
            Object.DestroyImmediate(doomed.gameObject);

            var plan = new ConversionPlan();
            PlugSocketConverter.Plan(root, vrcfToSpsNdmf: true, plan);
            PlugSocketConverter.Execute(root, vrcfToSpsNdmf: true, plan, new BuildConversionOps());
            var warnings = plan.Entries.SelectMany(e => e.Warnings).ToList();
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Not.Contain("copy failed"), "a destroyed reference threw instead of being reported");
        }
    }
}
