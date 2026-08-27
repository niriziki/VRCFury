using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using nadena.dev.ndmf.animator;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VF.Plugin.Passes;
using VF.Service;
using VF.Utils;

namespace VF.Tests {
    /// <summary>
    /// Runs the real upstream rewrite (BakeHapticPlugsService.RewriteClipForSps)
    /// through the SpsExternalAnimationRewritePass bridge, covering the external
    /// animation cases beyond Animated Toggle: resolver propagation,
    /// MeshRenderer-to-SkinnedMeshRenderer retargeting, SPS blendshape remapping,
    /// and material-swap patching. configureMaterial is a recording stand-in here;
    /// the real SpsPatcher behavior is only observable in a full build.
    /// </summary>
    [Category("VRCFury")]
    public class SpsRewriteClipForSpsTest {
        private GameObject avatar;
        private VFGameObject plug;
        private SkinnedMeshRenderer skin;
        private VFGameObject bakeRoot;
        private MeshRenderer resolver;

        private Material originalMat;
        private Material patchedMat;
        private List<int> patchedSlots;

        private BakeHapticPlugsService bake;
        private BakeHapticPlugsService.SpsRewriteToDo rewrite;
        private Dictionary<string, VFGameObject> targets;
        private Dictionary<VFGameObject, string> virtualPaths;

        [SetUp]
        public void SetUp() {
            avatar = new GameObject("avatar");
            GameObject NewChild(string name, GameObject parent) {
                var obj = new GameObject(name);
                obj.transform.SetParent(parent.transform);
                return obj;
            }
            var plugObj = NewChild("Plug", avatar);
            plug = plugObj;
            skin = plugObj.AddComponent<SkinnedMeshRenderer>();
            originalMat = new Material(Shader.Find("Standard"));
            skin.sharedMaterials = new[] { originalMat };
            bakeRoot = NewChild("BakeRoot", plugObj);
            resolver = NewChild("Resolver", plugObj).AddComponent<MeshRenderer>();

            patchedMat = new Material(Shader.Find("Standard"));
            patchedSlots = new List<int>();

            // RewriteClipForSps only touches avatarBindingStateService (for material
            // slot parsing, which is stateless), so uninitialized instances suffice.
            bake = (BakeHapticPlugsService)FormatterServices.GetUninitializedObject(typeof(BakeHapticPlugsService));
            var bindingState = (AvatarBindingStateService)FormatterServices.GetUninitializedObject(typeof(AvatarBindingStateService));
            typeof(BakeHapticPlugsService)
                .GetField("avatarBindingStateService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(bake, bindingState);

            rewrite = new BakeHapticPlugsService.SpsRewriteToDo {
                plugObject = plug,
                skin = skin,
                resolverRenderer = resolver,
                bakeRoot = bakeRoot,
                spsBlendshapes = new List<string> { "Foo" },
                configureMaterial = (slot, mat) => {
                    patchedSlots.Add(slot);
                    return patchedMat;
                },
            };
            targets = new Dictionary<string, VFGameObject> {
                ["Plug"] = plug,
                ["Skin"] = plug, // skin lives on the plug object itself
            };
            virtualPaths = new Dictionary<VFGameObject, string> {
                [plug] = "Plug",
                [bakeRoot] = "Plug/BakeRoot",
                [(GameObject)resolver.gameObject] = "Plug/Resolver",
            };
        }

        [TearDown]
        public void TearDown() {
            if (avatar != null) Object.DestroyImmediate(avatar);
            if (originalMat != null) Object.DestroyImmediate(originalMat);
            if (patchedMat != null) Object.DestroyImmediate(patchedMat);
        }

        private void Run(VirtualClip clip) {
            SpsExternalAnimationRewritePass.RewriteVirtualClip(
                clip,
                targets,
                bridge => bake.RewriteClipForSps(bridge, rewrite),
                obj => virtualPaths[obj]
            );
        }

        private static (string, System.Type, string)[] FloatBindings(VirtualClip clip) =>
            clip.GetFloatCurveBindings().Select(b => (b.path, b.type, b.propertyName)).ToArray();

        [Test]
        public void PropagatesSpsMaterialPropertiesToResolver() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "material._SPS_Enabled"),
                AnimationCurve.Constant(0, 1, 0));

            Run(clip);

            var bindings = FloatBindings(clip);
            Assert.That(bindings, Does.Contain(("Plug", typeof(SkinnedMeshRenderer), "material._SPS_Enabled")));
            Assert.That(bindings, Does.Contain(("Plug/Resolver", typeof(MeshRenderer), "material._SPS_Enabled")));
        }

        [Test]
        public void ConvertsLegacyTpsToggleToResolverAndBakeRoot() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "material._TPS_AnimatedToggle"),
                AnimationCurve.Constant(0, 1, 0));

            Run(clip);

            var bindings = FloatBindings(clip);
            Assert.That(bindings, Does.Contain(("Plug/Resolver", typeof(MeshRenderer), "material._SPS_Enabled")));
            Assert.That(bindings, Does.Contain(("Plug/BakeRoot", typeof(GameObject), "m_IsActive")));
        }

        [Test]
        public void RetargetsMeshRendererBindingsToSkinnedMeshRenderer() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(MeshRenderer), "material._Color.r"),
                AnimationCurve.Constant(0, 1, 1));

            Run(clip);

            var bindings = FloatBindings(clip);
            Assert.That(bindings, Does.Contain(("Plug", typeof(SkinnedMeshRenderer), "material._Color.r")));
            Assert.That(bindings, Has.No.Member(("Plug", typeof(MeshRenderer), "material._Color.r")));
        }

        [Test]
        public void KeepsDiscreteCurveFlagAcrossMeshRendererRetargeting() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.DiscreteCurve("Plug", typeof(MeshRenderer), "m_Enabled"),
                AnimationCurve.Constant(0, 1, 1));

            Run(clip);

            var retyped = clip.GetFloatCurveBindings()
                .Single(b => b.propertyName == "m_Enabled");
            Assert.That(retyped.type, Is.EqualTo(typeof(SkinnedMeshRenderer)));
            Assert.That(retyped.isDiscreteCurve, Is.True,
                "discrete curve flag was lost across the MeshRenderer retargeting");
        }

        [Test]
        public void KeepsBothCurvesWhenRetargetingConvergesOnAnotherTypeVariant() {
            // A MeshRenderer discrete curve and a SkinnedMeshRenderer continuous
            // curve on the same property: the retargeting would make them collide
            // inside the bridge, so only one may be imported and neither curve nor
            // its flags may be lost.
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.DiscreteCurve("Plug", typeof(MeshRenderer), "m_Enabled"),
                AnimationCurve.Constant(0, 1, 1));
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "m_Enabled"),
                AnimationCurve.Constant(0, 1, 0.5f));

            Run(clip);

            var bindings = clip.GetFloatCurveBindings()
                .Where(b => b.propertyName == "m_Enabled")
                .ToArray();
            Assert.That(bindings, Has.Length.EqualTo(2), "one of the curve variants was lost");
            var discrete = bindings.Single(b => b.isDiscreteCurve);
            var continuous = bindings.Single(b => !b.isDiscreteCurve);
            // The MeshRenderer variant must have been the imported one and been
            // retargeted; unconverted it would drive nothing after the bake.
            Assert.That(discrete.type, Is.EqualTo(typeof(SkinnedMeshRenderer)),
                "the MeshRenderer variant was not converted");
            Assert.That(clip.GetFloatCurve(discrete).keys.Select(k => k.value), Is.All.EqualTo(1f));
            Assert.That(clip.GetFloatCurve(continuous).keys.Select(k => k.value), Is.All.EqualTo(0.5f));
        }

        [Test]
        public void ConvertsMeshRendererVariantRegardlessOfCurveOrder() {
            // Same as above but with the SkinnedMeshRenderer curve authored
            // first: the MeshRenderer variant must still win the import.
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "m_Enabled"),
                AnimationCurve.Constant(0, 1, 0.5f));
            clip.SetFloatCurve(
                EditorCurveBinding.DiscreteCurve("Plug", typeof(MeshRenderer), "m_Enabled"),
                AnimationCurve.Constant(0, 1, 1));

            Run(clip);

            var bindings = clip.GetFloatCurveBindings()
                .Where(b => b.propertyName == "m_Enabled")
                .ToArray();
            Assert.That(bindings, Has.Length.EqualTo(2), "one of the curve variants was lost");
            var discrete = bindings.Single(b => b.isDiscreteCurve);
            Assert.That(discrete.type, Is.EqualTo(typeof(SkinnedMeshRenderer)),
                "the MeshRenderer variant was not converted");
            Assert.That(clip.GetFloatCurve(discrete).keys.Select(k => k.value), Is.All.EqualTo(1f));
        }

        [Test]
        public void PrefersMeshRendererVariantAcrossCurveKinds() {
            // A SkinnedMeshRenderer float variant and a MeshRenderer
            // object-reference material swap on the same property: the
            // MeshRenderer curve must win the import even though object curves
            // are enumerated separately, so it gets retargeted and patched.
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]"),
                AnimationCurve.Constant(0, 1, 1));
            clip.SetObjectCurve(
                EditorCurveBinding.PPtrCurve("Plug", typeof(MeshRenderer), "m_Materials.Array.data[0]"),
                new[] { new ObjectReferenceKeyframe { time = 0, value = originalMat } });

            Run(clip);

            Assert.That(patchedSlots, Is.EqualTo(new[] { 0 }), "the MeshRenderer material swap was not patched");
            var retyped = EditorCurveBinding.PPtrCurve("Plug", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
            var curve = clip.GetObjectCurve(retyped);
            Assert.That(curve, Is.Not.Null, "the MeshRenderer material swap was not retargeted");
            Assert.That(curve[0].value, Is.EqualTo(patchedMat));
            // The skipped float variant stays untouched.
            var floatBinding = EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
            Assert.That(clip.GetFloatCurve(floatBinding), Is.Not.Null, "the skipped float variant was lost");
        }

        [Test]
        public void RetargetsMeshRendererTypedBlendshapeDestinationCurves() {
            // An existing MeshRenderer-typed curve on a generated-destination
            // property name is still imported: without the retargeting it would
            // drive nothing after the bake.
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.DiscreteCurve("Plug", typeof(MeshRenderer), "material._SPS_Blendshape1"),
                AnimationCurve.Constant(0, 1, 7));

            Run(clip);

            var retyped = clip.GetFloatCurveBindings()
                .Single(b => b.propertyName == "material._SPS_Blendshape1");
            Assert.That(retyped.type, Is.EqualTo(typeof(SkinnedMeshRenderer)));
            Assert.That(retyped.isDiscreteCurve, Is.True);
            Assert.That(clip.GetFloatCurve(retyped).keys.Select(k => k.value), Is.All.EqualTo(7f));
        }

        [Test]
        public void LeavesExistingSpsBlendshapeDestinationCurvesIntact() {
            // A clip already containing a flag variant of the blendshape
            // destination property: the generated curve must not consume or
            // overwrite it.
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.DiscreteCurve("Plug", typeof(SkinnedMeshRenderer), "material._SPS_Blendshape0"),
                AnimationCurve.Constant(0, 1, 7));
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "blendShape.Foo"),
                AnimationCurve.Constant(0, 1, 100));

            Run(clip);

            var destinations = clip.GetFloatCurveBindings()
                .Where(b => b.propertyName == "material._SPS_Blendshape0")
                .ToArray();
            var existing = destinations.Single(b => b.isDiscreteCurve);
            var generated = destinations.Single(b => !b.isDiscreteCurve);
            Assert.That(clip.GetFloatCurve(existing).keys.Select(k => k.value), Is.All.EqualTo(7f),
                "the pre-existing destination curve was overwritten");
            Assert.That(clip.GetFloatCurve(generated).keys.Select(k => k.value), Is.All.EqualTo(100f));
        }

        [Test]
        public void RemapsSpsBlendshapesToMaterialProperty() {
            var clip = VirtualClip.Create("test");
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "blendShape.Foo"),
                AnimationCurve.Constant(0, 1, 100));
            clip.SetFloatCurve(
                EditorCurveBinding.FloatCurve("Plug", typeof(SkinnedMeshRenderer), "blendShape.Bar"),
                AnimationCurve.Constant(0, 1, 100));

            Run(clip);

            var bindings = FloatBindings(clip);
            Assert.That(bindings, Does.Contain(("Plug", typeof(SkinnedMeshRenderer), "material._SPS_Blendshape0")));
            // Non-registered blendshapes are untouched and get no material curve.
            Assert.That(bindings, Does.Contain(("Plug", typeof(SkinnedMeshRenderer), "blendShape.Bar")));
            Assert.That(bindings.Select(b => b.Item3), Does.Not.Contain("material._SPS_Blendshape1"));
        }

        [Test]
        public void PatchesMaterialSwapCurvesThroughConfigureMaterial() {
            var clip = VirtualClip.Create("test");
            var binding = EditorCurveBinding.PPtrCurve("Plug", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
            clip.SetObjectCurve(binding, new[] {
                new ObjectReferenceKeyframe { time = 0, value = originalMat },
            });

            Run(clip);

            Assert.That(patchedSlots, Is.EqualTo(new[] { 0 }));
            var curve = clip.GetObjectCurve(binding);
            Assert.That(curve, Is.Not.Null);
            Assert.That(curve[0].value, Is.EqualTo(patchedMat));
        }

        [Test]
        public void PatchesMaterialSwapOnMeshRendererTypedBinding() {
            var clip = VirtualClip.Create("test");
            clip.SetObjectCurve(
                EditorCurveBinding.PPtrCurve("Plug", typeof(MeshRenderer), "m_Materials.Array.data[0]"),
                new[] { new ObjectReferenceKeyframe { time = 0, value = originalMat } });

            Run(clip);

            Assert.That(patchedSlots, Is.EqualTo(new[] { 0 }));
            var retyped = EditorCurveBinding.PPtrCurve("Plug", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
            var curve = clip.GetObjectCurve(retyped);
            Assert.That(curve, Is.Not.Null);
            Assert.That(curve[0].value, Is.EqualTo(patchedMat));
        }
    }
}
