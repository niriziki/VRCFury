using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VF.Component;
using VF.Model.StateAction;
using VRC.SDK3.Avatars.Components;

namespace VF.Tests {
    /// <summary>
    /// Full NDMF build (through Transforming) of a minimal avatar with a socket whose
    /// depth actions write AAPs to parameters the SPS controller does not own, verifying
    /// that the curves survive into the merged FX and that the parameters are declared.
    /// </summary>
    [Category("VRCFury")]
    public class SpsExternalAapBuildTest {
        private const string ActionParam = "SpsTestFxFloat";
        private const string ClipParam = "SpsTestClipAap";

        private GameObject avatar;
        private Mesh bodyMesh;
        private Material bodyMat;
        private AnimationClip aapClip;

        [SetUp]
        public void SetUp() {
            avatar = MaHierarchyResolverTest.BuildMinimalHumanoidAvatar();

            var tmp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyMesh = Object.Instantiate(tmp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(tmp);
            bodyMesh.AddBlendShapeFrame("Foo", 100, new Vector3[bodyMesh.vertexCount], null, null);
            bodyMat = new Material(Shader.Find("Standard"));
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            if (avatar != null) Object.DestroyImmediate(avatar);
            if (bodyMesh != null) Object.DestroyImmediate(bodyMesh, true);
            if (bodyMat != null) Object.DestroyImmediate(bodyMat, true);
            if (aapClip != null) Object.DestroyImmediate(aapClip, true);
        }

        [Test]
        public void KeepsAapsTargetingParametersSpsDoesNotOwn() {
            // The NDMF build emits benign Unity-internal [Assert] logs (DontSave
            // objects touched by asset persistence); the build outcome is verified
            // by the assertions below instead.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;

            var bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(avatar.transform, false);
            var smr = bodyObj.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = bodyMesh;
            smr.sharedMaterials = new[] { bodyMat };

            aapClip = new AnimationClip { name = "UserAapClip" };
            AnimationUtility.SetEditorCurve(
                aapClip,
                EditorCurveBinding.FloatCurve("", typeof(Animator), ClipParam),
                AnimationCurve.Constant(0, 1, 0.5f));

            var socketObj = new GameObject("TestSocket");
            socketObj.transform.SetParent(avatar.transform, false);
            socketObj.transform.localPosition = new Vector3(0, 1f, 0.1f);
            var socket = socketObj.AddComponent<VRCFuryHapticSocket>();

            // "Set an FX Float" alongside a blendshape, in one action set: the user-visible
            // report was that the blendshape survived in the built clip but the AAP did not.
            socket.depthActions2.Add(new VRCFuryHapticSocket.DepthActionNew {
                actionSet = {
                    actions = {
                        new FxFloatAction { name = ActionParam, value = 1 },
                        new BlendShapeAction { blendShape = "Foo", blendShapeValue = 100 }
                    }
                }
            });
            // The same failure reaches users through an "Animation Clip" action whose clip
            // already contains AAP curves of its own.
            socket.depthActions2.Add(new VRCFuryHapticSocket.DepthActionNew {
                actionSet = {
                    actions = { new AnimationClipAction { motion = aapClip } }
                }
            });

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
                .FirstOrDefault(c => c != null) as AnimatorController;
            Assert.That(fx, Is.Not.Null, "no FX controller after build");

            bool HasAap(AnimationClip clip, string param) => AnimationUtility.GetCurveBindings(clip)
                .Any(b => b.type == typeof(Animator) && b.path == "" && b.propertyName == param);

            var clips = fx.animationClips.Where(c => c != null).Distinct().ToArray();

            // Every action in one set becomes its own clip under a direct blend tree, so the
            // blendshape only proves the set was built; the AAP lives in a sibling clip.
            Assert.That(clips.Any(c => AnimationUtility.GetCurveBindings(c)
                .Any(b => b.type == typeof(SkinnedMeshRenderer) && b.propertyName == "blendShape.Foo")),
                Is.True, "depth action clips missing from merged FX");

            var aapDump = string.Join(", ", clips.SelectMany(c => AnimationUtility.GetCurveBindings(c)
                .Where(b => b.type == typeof(Animator) && b.path == "")
                .Select(b => c.name + ":" + b.propertyName)));
            var paramTypes = fx.parameters.ToDictionary(p => p.name, p => p.type);

            Assert.That(clips.Any(c => HasAap(c, ActionParam)), Is.True,
                $"Set an FX Float was dropped. AAPs in FX: {aapDump}");
            Assert.That(clips.Any(c => HasAap(c, ClipParam)), Is.True,
                $"AAP authored in a user Animation Clip was dropped. AAPs in FX: {aapDump}");

            foreach (var param in new[] { ActionParam, ClipParam }) {
                Assert.That(paramTypes.ContainsKey(param), Is.True,
                    $"{param} was not declared in FX. Params: {string.Join(", ", paramTypes.Keys)}");
                Assert.That(paramTypes[param], Is.EqualTo(AnimatorControllerParameterType.Float),
                    $"{param} was not declared as a float");
            }
        }
    }
}
