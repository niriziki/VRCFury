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
