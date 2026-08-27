using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf.animator;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VF.Plugin.Passes;
using VF.Utils;
using VF.Utils.Controller;

namespace VF.Tests {
    /// <summary>
    /// Tests the VirtualClip/VFClip bridge of SpsExternalAnimationRewritePass.
    /// The rewrite logic itself is upstream's RewriteClip (exercised via
    /// BakeHapticPlugsService.RewriteClipForSps in real builds); here a stand-in
    /// rewriter with the same VFClip API shape verifies the bridge mechanics.
    /// </summary>
    [Category("VRCFury")]
    public class SpsExternalAnimationRewriteTest {
        private GameObject avatar;
        private VFGameObject plug;
        private VFGameObject bakeRoot;
        private VFGameObject resolver;

        // Simulates post-BoneProxy state: the clip refers to objects by their
        // original ("virtual") paths, which differ from the live hierarchy paths.
        private const string PlugVirtualPath = "Costume/Plug";
        private const string BakeRootVirtualPath = "Costume/Plug/BakeRoot";
        private const string ResolverVirtualPath = "Costume/Plug/BakeRoot/Resolver";

        private Dictionary<string, VFGameObject> targets;
        private Dictionary<VFGameObject, string> virtualPaths;

        [SetUp]
        public void SetUp() {
            avatar = new GameObject("avatar");
            VFGameObject NewChild(string name, VFGameObject parent) {
                var obj = new GameObject(name);
                obj.transform.SetParent(((GameObject)parent).transform);
                return obj;
            }
            plug = NewChild("Plug", avatar);
            bakeRoot = NewChild("BakeRoot", plug);
            resolver = NewChild("Resolver", bakeRoot);
            targets = new Dictionary<string, VFGameObject> {
                [PlugVirtualPath] = plug,
            };
            virtualPaths = new Dictionary<VFGameObject, string> {
                [plug] = PlugVirtualPath,
                [bakeRoot] = BakeRootVirtualPath,
                [resolver] = ResolverVirtualPath,
            };
        }

        [TearDown]
        public void TearDown() {
            if (avatar != null) Object.DestroyImmediate(avatar);
        }

        private string GetVirtualPath(VFGameObject obj) => virtualPaths[obj];

        private static EditorCurveBinding PlugEnabledBinding() =>
            EditorCurveBinding.FloatCurve(PlugVirtualPath, typeof(Transform), "spsAnimatedEnabled");

        private static AnimationCurve OnOffCurve() => AnimationCurve.Constant(0, 1, 0);

        [Test]
        public void RewritesMatchingBindingToLiveObjectsAndBack() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(PlugEnabledBinding(), OnOffCurve());

            SpsExternalAnimationRewritePass.RewriteVirtualClip(clip, targets, bridge => {
                foreach (var (binding, curve) in bridge.GetAllCurves()) {
                    Assert.That(binding.Targets(plug), Is.True);
                    Assert.That(binding.propertyName, Is.EqualTo("spsAnimatedEnabled"));
                    // Same call shapes as upstream RewriteClip
                    bridge.SetCurve(((GameObject)resolver).transform, "material._SPS_Enabled", curve);
                    bridge.SetEnabled((GameObject)bakeRoot, curve);
                }
            }, GetVirtualPath);

            var bindings = clip.GetFloatCurveBindings()
                .Select(b => (b.path, b.type, b.propertyName))
                .ToArray();
            // Original binding round-trips (upstream keeps it), plus the two outputs
            // written back at virtual paths.
            Assert.That(bindings, Does.Contain((PlugVirtualPath, typeof(Transform), "spsAnimatedEnabled")));
            Assert.That(bindings, Does.Contain((ResolverVirtualPath, typeof(Transform), "material._SPS_Enabled")));
            Assert.That(bindings, Does.Contain((BakeRootVirtualPath, typeof(GameObject), "m_IsActive")));
            var written = clip.GetFloatCurve(
                EditorCurveBinding.FloatCurve(BakeRootVirtualPath, typeof(GameObject), "m_IsActive"));
            Assert.That(written, Is.Not.Null);
            Assert.That(written.keys.Select(k => k.value), Is.All.EqualTo(0f));
        }

        [Test]
        public void LeavesUnrelatedBindingsAlone() {
            var clip = VirtualClip.Create("test");
            var unrelated = EditorCurveBinding.FloatCurve("Body", typeof(SkinnedMeshRenderer), "blendShape.Smile");
            clip.SetFloatCurve(unrelated, OnOffCurve());
            clip.SetFloatCurve(PlugEnabledBinding(), OnOffCurve());

            var seen = new List<string>();
            SpsExternalAnimationRewritePass.RewriteVirtualClip(clip, targets, bridge => {
                seen.AddRange(bridge.GetAllCurves().Select(pair => pair.Item1.propertyName));
            }, GetVirtualPath);

            Assert.That(seen, Is.EqualTo(new[] { "spsAnimatedEnabled" }));
            Assert.That(
                clip.GetFloatCurveBindings().Select(b => (b.path, b.type, b.propertyName)),
                Does.Contain(("Body", typeof(SkinnedMeshRenderer), "blendShape.Smile")));
        }

        [Test]
        public void RemovesBindingsDeletedByRewriter() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(PlugEnabledBinding(), OnOffCurve());

            SpsExternalAnimationRewritePass.RewriteVirtualClip(clip, targets, bridge => {
                foreach (var (binding, _) in bridge.GetAllCurves()) {
                    bridge.SetCurve(binding, null);
                }
            }, GetVirtualPath);

            Assert.That(clip.GetFloatCurveBindings(), Is.Empty);
        }

        [Test]
        public void PreservesDiscreteCurveFlagsOnRoundTrip() {
            var clip = VirtualClip.Create("test");
            var discrete = EditorCurveBinding.DiscreteCurve(
                PlugVirtualPath, typeof(SkinnedMeshRenderer), "m_UpdateWhenOffscreen");
            clip.SetFloatCurve(discrete, OnOffCurve());
            clip.SetFloatCurve(PlugEnabledBinding(), OnOffCurve());

            SpsExternalAnimationRewritePass.RewriteVirtualClip(clip, targets, bridge => { }, GetVirtualPath);

            var roundTripped = clip.GetFloatCurveBindings()
                .Single(b => b.propertyName == "m_UpdateWhenOffscreen");
            Assert.That(roundTripped.isDiscreteCurve, Is.True,
                "discrete curve flag was lost by the bridge round-trip");
        }

        [Test]
        public void LeavesCoexistingFlagVariantCurvesIntact() {
            // NDMF allows a float curve and an object-reference curve on the same
            // (path, type, property); only the first import wins, the other must
            // survive untouched and the build must not throw.
            var mat = new Material(Shader.Find("Standard"));
            try {
                var clip = VirtualClip.Create("test");
                var floatBinding = EditorCurveBinding.FloatCurve(
                    PlugVirtualPath, typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
                var objectBinding = EditorCurveBinding.PPtrCurve(
                    PlugVirtualPath, typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
                clip.SetFloatCurve(floatBinding, OnOffCurve());
                clip.SetObjectCurve(objectBinding, new[] {
                    new ObjectReferenceKeyframe { time = 0, value = mat },
                });

                SpsExternalAnimationRewritePass.RewriteVirtualClip(clip, targets, bridge => { }, GetVirtualPath);

                Assert.That(clip.GetFloatCurve(floatBinding), Is.Not.Null);
                var objectCurve = clip.GetObjectCurve(objectBinding);
                Assert.That(objectCurve, Is.Not.Null, "the skipped variant curve was lost");
                Assert.That(objectCurve[0].value, Is.EqualTo(mat));
            } finally {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void SkipsRewriterWhenNothingMatches() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Body", typeof(SkinnedMeshRenderer), "blendShape.Smile"),
                OnOffCurve());

            var called = false;
            SpsExternalAnimationRewritePass.RewriteVirtualClip(
                clip, targets, bridge => called = true, GetVirtualPath);

            Assert.That(called, Is.False);
        }
    }
}
