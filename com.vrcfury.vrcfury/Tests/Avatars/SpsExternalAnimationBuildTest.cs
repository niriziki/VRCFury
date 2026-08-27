using System.Linq;
using System.Reflection;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VF.Component;
using VRC.SDK3.Avatars.Components;

namespace VF.Tests {
    /// <summary>
    /// Full NDMF build (through Transforming) of a minimal avatar with an SPS plug
    /// moved by MA Bone Proxy and user animations delivered via MA Merge Animator,
    /// verifying that SpsExternalAnimationRewritePass rewrites them in the merged
    /// FX: Animated Toggle, resolver propagation, SPS blendshape remapping, and
    /// material-swap patching (including the real SpsPatcher via configureMaterial).
    /// </summary>
    [Category("VRCFury")]
    public class SpsExternalAnimationBuildTest {
        private GameObject avatar;
        private Mesh plugMesh;
        private Material plugMat;
        private Material swapMat;

        [SetUp]
        public void SetUp() {
            avatar = MaHierarchyResolverTest.BuildMinimalHumanoidAvatar();

            var tmp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            plugMesh = Object.Instantiate(tmp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(tmp);
            plugMesh.AddBlendShapeFrame("Foo", 100, new Vector3[plugMesh.vertexCount], null, null);
            plugMat = new Material(Shader.Find("Standard"));
            swapMat = new Material(Shader.Find("Standard")) { name = "SwapTarget" };
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            if (avatar != null) Object.DestroyImmediate(avatar);
            if (plugMesh != null) Object.DestroyImmediate(plugMesh);
            if (plugMat != null) Object.DestroyImmediate(plugMat);
            if (swapMat != null) Object.DestroyImmediate(swapMat);
        }

        [Test]
        public void RewritesExternalAnimationsInMergedFx() {
            // The NDMF build emits benign Unity-internal [Assert] logs (DontSave
            // objects touched by asset persistence); the build outcome is verified
            // by the assertions below instead.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;

            var plugObj = new GameObject("TestPlug");
            plugObj.transform.SetParent(avatar.transform, false);
            plugObj.transform.localPosition = new Vector3(0, 1f, 0.2f);
            plugObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            var smr = plugObj.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = plugMesh;
            smr.sharedMaterials = new[] { plugMat };
            var plug = plugObj.AddComponent<VRCFuryHapticPlug>();
            plug.spsBlendshapes.Add("Foo");

            var proxy = plugObj.AddComponent<ModularAvatarBoneProxy>();
            proxy.boneReference = HumanBodyBones.Hips;
            proxy.attachmentMode = BoneProxyAttachmentMode.AsChildAtRoot;

            var clip = new AnimationClip { name = "PlugExternalAnims" };
            clip.SetCurve("TestPlug", typeof(VRCFuryHapticPlug), "spsAnimatedEnabled", AnimationCurve.Linear(0, 1, 1, 0));
            clip.SetCurve("TestPlug", typeof(SkinnedMeshRenderer), "material._SPS_Enabled", AnimationCurve.Linear(0, 1, 1, 0));
            clip.SetCurve("TestPlug", typeof(SkinnedMeshRenderer), "blendShape.Foo", AnimationCurve.Constant(0, 1, 100));
            AnimationUtility.SetObjectReferenceCurve(
                clip,
                EditorCurveBinding.PPtrCurve("TestPlug", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]"),
                new[] { new ObjectReferenceKeyframe { time = 0, value = swapMat } });

            var controller = new AnimatorController { name = "ExternalToggleController" };
            controller.AddLayer("PlugToggle");
            controller.layers[0].stateMachine.AddState("Toggle").motion = clip;

            var mergeObj = new GameObject("TestToggleMA");
            mergeObj.transform.SetParent(avatar.transform, false);
            var merge = mergeObj.AddComponent<ModularAvatarMergeAnimator>();
            merge.animator = controller;
            merge.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            merge.pathMode = MergeAnimatorPathMode.Absolute;

            // Internal AvatarProcessor.ProcessAvatar(GameObject, BuildPhase): run
            // in place through Transforming (includes MA and the SPS late pass).
            var processorType = typeof(BuildContext).Assembly.GetType("nadena.dev.ndmf.AvatarProcessor");
            var process = processorType
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => {
                    var ps = m.GetParameters();
                    return m.Name == "ProcessAvatar" && ps.Length == 2
                        && ps[0].ParameterType == typeof(GameObject) && ps[1].ParameterType == typeof(BuildPhase);
                });
            process.Invoke(null, new object[] { avatar, BuildPhase.Transforming });

            var fx = avatar.GetComponent<VRCAvatarDescriptor>().baseAnimationLayers
                .Where(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX)
                .Select(l => l.animatorController)
                .FirstOrDefault(c => c != null);
            Assert.That(fx, Is.Not.Null, "no FX controller after build");

            var userClip = fx.animationClips.Where(c => c != null).Distinct()
                .FirstOrDefault(c => c.name.Contains("PlugExternalAnims"));
            Assert.That(userClip, Is.Not.Null, "user clip missing from merged FX");

            var bindings = AnimationUtility.GetCurveBindings(userClip)
                .Select(b => (b.path, b.type, b.propertyName))
                .ToArray();

            // Bone Proxy moved the plug under Hips; all rewritten targets must
            // resolve to the relocated path.
            const string plugPath = "Armature/Hips/TestPlug";
            Assert.That(bindings.Any(b =>
                b.path.StartsWith(plugPath + "/") && b.propertyName == "material._SPS_Enabled" && b.path.Contains("SpsResolver")),
                Is.True, "Animated Toggle was not propagated to the SPS resolver");
            Assert.That(bindings.Any(b =>
                b.path.StartsWith(plugPath + "/") && b.type == typeof(GameObject) && b.propertyName == "m_IsActive"),
                Is.True, "Animated Toggle does not toggle the bake root");
            Assert.That(bindings, Does.Contain((plugPath, typeof(SkinnedMeshRenderer), "material._SPS_Blendshape0")),
                "SPS blendshape was not remapped to its material property");

            var objBindings = AnimationUtility.GetObjectReferenceCurveBindings(userClip);
            var matBinding = objBindings.FirstOrDefault(b => b.propertyName == "m_Materials.Array.data[0]");
            Assert.That(matBinding.propertyName, Is.Not.Null.And.Not.Empty, "material swap binding missing");
            var swapped = AnimationUtility.GetObjectReferenceCurve(userClip, matBinding)[0].value as Material;
            Assert.That(swapped, Is.Not.Null);
            Assert.That(swapped, Is.Not.EqualTo(swapMat), "swap target material was not routed through configureMaterial");
            Assert.That(swapped.shader, Is.Not.EqualTo(swapMat.shader), "swapped-in material was not SPS-patched");
        }
    }
}
