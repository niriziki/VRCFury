using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using Nrzk.SpsMigrator.Reflection;
using UnityEngine;
using VF.Component;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VfStubTests {
    /**
     * Stub components reach the SPS build through the migrator, so the synced bits NDMF's parameter
     * introspection reports for them must match what that build adds, attributed to SPSNDMF.
     */
    public class StubParameterProviderTests : TempSceneFixture {
        private GameObject avatar;
        private Mesh plugMesh;
        private Material plugMat;
        private float nextZ;

        [SetUp]
        public void SetUp() {
            avatar = NewObject("Avatar");
            avatar.AddComponent<Animator>();
            avatar.AddComponent<VRCAvatarDescriptor>();
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            plugMesh = Object.Instantiate(tmp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(tmp);
            plugMat = new Material(Shader.Find("Standard"));
            nextZ = 0.1f;
            // The build and the VRC SDK's own validation of the new descriptor log benign errors; the
            // outcome is checked through the build report and the parameters instead.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            if (plugMesh != null) Object.DestroyImmediate(plugMesh, true);
            if (plugMat != null) Object.DestroyImmediate(plugMat, true);
        }

        private GameObject NewChild(string name) {
            var obj = NewObject(name);
            obj.transform.SetParent(avatar.transform, false);
            obj.transform.localPosition = new Vector3(0, 1f, nextZ);
            nextZ += 0.1f;
            return obj;
        }

        private GameObject NewPlugObject(string name) {
            var obj = NewChild(name);
            obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            var smr = obj.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = plugMesh;
            smr.sharedMaterials = new[] { plugMat };
            return obj;
        }

        private void AssertEstimateMatchesBuild(int expectedBits) {
            var provided = nadena.dev.ndmf.ParameterInfo.ForPreview(ComputeContext.NullContext)
                .GetParametersForObject(avatar)
                .ToList();
            var estimated = provided.Sum(p => p.BitUsage);
            var fromStub = provided.Where(p => p.Source is VRCFuryHapticSocket || p.Source is VRCFuryHapticPlug).ToList();
            Assert.That(fromStub.Where(p => !p.IsHidden).Select(p => p.EffectiveName), Is.Empty,
                "stub placeholder parameters must be hidden");
            Assert.That(fromStub.Select(p => p.Plugin?.QualifiedName).Distinct(),
                Is.EquivalentTo(fromStub.Count == 0 ? new string[0] : new[] { "net.nrzk.spsndmf" }),
                "stub parameters must be attributed to SPSNDMF");

            var processorType = typeof(BuildContext).Assembly.GetType("nadena.dev.ndmf.AvatarProcessor");
            var process = processorType
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => {
                    var ps = m.GetParameters();
                    return m.Name == "ProcessAvatar" && ps.Length == 2
                        && ps[0].ParameterType == typeof(GameObject) && ps[1].ParameterType == typeof(BuildPhase);
                });
            var errors = ErrorReport.CaptureErrors(
                () => process.Invoke(null, new object[] { avatar, BuildPhase.Transforming }));
            Assert.That(errors.Where(e => e.TheError.Severity >= ErrorSeverity.Error), Is.Empty,
                "the build reported an error");

            // With no expression parameters on the descriptor, the build starts from VRChat's
            // defaults (VRCEmote, VRCFaceBlendH/V), which no component supplies.
            var synced = (avatar.GetComponent<VRCAvatarDescriptor>().expressionParameters?.parameters
                    ?? new VRCExpressionParameters.Parameter[0])
                .Where(p => p.networkSynced && !p.name.StartsWith("VRC"))
                .ToArray();
            var builtBits = synced.Sum(p => p.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8);
            var dump = "built synced parameters: " + string.Join(", ", synced.Select(p => $"{p.name}:{p.valueType}"));

            Assert.That(builtBits, Is.EqualTo(expectedBits), dump);
            Assert.That(estimated, Is.EqualTo(builtBits), dump);
        }

        [Test]
        public void StubSocketAndPlug() {
            NewChild("Socket").AddComponent<VRCFuryHapticSocket>();
            NewPlugObject("Plug").AddComponent<VRCFuryHapticPlug>();
            AssertEstimateMatchesBuild(5);
        }

        // Avatar-wide toggles are one parameter whichever package the components come from.
        [Test]
        public void StubAndSpsNdmfSockets() {
            NewChild("StubSocket").AddComponent<VRCFuryHapticSocket>();
            NewChild("SpsNdmfSocket").AddComponent(PackageBinding.SpsNdmfSocketType);
            AssertEstimateMatchesBuild(4);
        }

        // The migrator skips a stub component whose SPSNDMF counterpart is already on the same object.
        [Test]
        public void StubAndSpsNdmfSocketOnOneObject() {
            var obj = NewChild("Socket");
            obj.AddComponent<VRCFuryHapticSocket>();
            // Without lights on the counterpart, counting the stub socket would also add Legacy.
            var twin = obj.AddComponent(PackageBinding.SpsNdmfSocketType);
            twin.GetType().GetField("useLights").SetValue(twin, false);
            AssertEstimateMatchesBuild(2);
        }

        [Test]
        public void StubPlugSavedBeforeVersion3() {
            var plug = NewPlugObject("Plug").AddComponent<VRCFuryHapticPlug>();
            plug.Version = 2;
#pragma warning disable 0612
            plug.configureSps = false;
            AssertEstimateMatchesBuild(0);
        }
    }
}
