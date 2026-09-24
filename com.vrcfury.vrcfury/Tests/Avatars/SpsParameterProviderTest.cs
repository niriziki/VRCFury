using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using UnityEngine;
using VF.Component;
using VF.Model.Feature;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VF.Tests {
    /// <summary>
    /// The synced bits NDMF's parameter introspection reports for SPS components before the build
    /// must match what the build actually adds to the expression parameters. Each case also pins
    /// the expected count, so the estimate and the build cannot drift together unnoticed.
    /// </summary>
    [Category("VRCFury")]
    public class SpsParameterProviderTest {
        private GameObject avatar;
        private Mesh plugMesh;
        private Material plugMat;
        private float nextZ;

        [SetUp]
        public void SetUp() {
            avatar = MaHierarchyResolverTest.BuildMinimalHumanoidAvatar();
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            plugMesh = Object.Instantiate(tmp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(tmp);
            plugMat = new Material(Shader.Find("Standard"));
            nextZ = 0.1f;
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            if (avatar != null) Object.DestroyImmediate(avatar);
            if (plugMesh != null) Object.DestroyImmediate(plugMesh, true);
            if (plugMat != null) Object.DestroyImmediate(plugMat, true);
        }

        private GameObject NewChild(string name, Transform parent = null) {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent != null ? parent : avatar.transform, false);
            obj.transform.localPosition = new Vector3(0, 1f, nextZ);
            nextZ += 0.1f;
            return obj;
        }

        private VRCFuryHapticSocket AddSocket(string name = "Socket", Transform parent = null) {
            return NewChild(name, parent).AddComponent<VRCFuryHapticSocket>();
        }

        private VRCFuryHapticPlug AddPlug(string name = "Plug", Transform parent = null) {
            var obj = NewChild(name, parent);
            obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            var smr = obj.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = plugMesh;
            smr.sharedMaterials = new[] { plugMat };
            return obj.AddComponent<VRCFuryHapticPlug>();
        }

        private void AssertEstimateMatchesBuild(int expectedBits) {
            var estimated = nadena.dev.ndmf.ParameterInfo.ForPreview(ComputeContext.NullContext)
                .GetParametersForObject(avatar)
                .Sum(p => p.BitUsage);

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
        public void NoSpsComponents() {
            AssertEstimateMatchesBuild(0);
        }

        // Its own toggle, plus Stealth and Legacy Compatibility for the avatar.
        [Test]
        public void DefaultSocket() {
            AddSocket();
            AssertEstimateMatchesBuild(3);
        }

        [Test]
        public void SocketWithoutLights() {
            AddSocket().useLights = false;
            AssertEstimateMatchesBuild(2);
        }

        [Test]
        public void SocketWithoutMenuItem() {
            AddSocket().addMenuItem = false;
            AssertEstimateMatchesBuild(0);
        }

        // Each socket keeps its own toggle; the avatar-wide ones are only counted once.
        [Test]
        public void SocketsSharingAName() {
            AddSocket("Socket").name = "Same";
            AddSocket("Socket").name = "Same";
            AssertEstimateMatchesBuild(4);
        }

        [Test]
        public void SocketsSharingAnOscId() {
            AddSocket("SocketA").oscId = "Same";
            AddSocket("SocketB").oscId = "Same";
            AssertEstimateMatchesBuild(4);
        }

        // Legacy Compatibility exists as soon as one socket with a menu item uses lights.
        [Test]
        public void SocketsWithMixedLights() {
            AddSocket("SocketA");
            AddSocket("SocketB").useLights = false;
            AddSocket("SocketC").useLights = false;
            AssertEstimateMatchesBuild(5);
        }

        // Only sockets with a menu item decide whether Legacy Compatibility exists.
        [Test]
        public void LightsOnlyOnASocketWithoutMenuItem() {
            AddSocket("SocketA").useLights = false;
            AddSocket("SocketB").addMenuItem = false;
            AssertEstimateMatchesBuild(2);
        }

        // Disable Depth Pass and Disable Realtime Shadows.
        [Test]
        public void DefaultPlug() {
            AddPlug();
            AssertEstimateMatchesBuild(2);
        }

        [Test]
        public void PlugWithoutSps() {
            AddPlug().enableSps = false;
            AssertEstimateMatchesBuild(0);
        }

        [Test]
        public void TipLightOnPlugWithoutSps() {
            var plug = AddPlug();
            plug.enableSps = false;
            plug.addDpsTipLight = true;
            AssertEstimateMatchesBuild(1);
        }

        [Test]
        public void PlugsShareTheirToggles() {
            AddPlug("PlugA").addDpsTipLight = true;
            AddPlug("PlugB").addDpsTipLight = true;
            AddPlug("PlugC");
            AssertEstimateMatchesBuild(3);
        }

        [Test]
        public void SocketsAndPlugs() {
            AddSocket("SocketA");
            AddSocket("SocketB").useLights = false;
            AddSocket("SocketNoMenu").addMenuItem = false;
            AddPlug("PlugA");
            AddPlug("PlugB").addDpsTipLight = true;
            AssertEstimateMatchesBuild(7);
        }

        [Test]
        public void ComponentsOnInactiveObjects() {
            AddSocket().gameObject.SetActive(false);
            AddPlug().gameObject.SetActive(false);
            AssertEstimateMatchesBuild(5);
        }

        [Test]
        public void ComponentsUnderEditorOnly() {
            var editorOnly = NewChild("EditorOnly");
            editorOnly.tag = "EditorOnly";
            AddSocket(parent: editorOnly.transform);
            AddPlug(parent: editorOnly.transform).addDpsTipLight = true;
            AddPlug();
            AssertEstimateMatchesBuild(2);
        }

        [Test]
        public void NestedComponents() {
            var socket = AddSocket("Outer");
            AddSocket("Inner", socket.transform);
            AddPlug("Plug", socket.transform);
            AssertEstimateMatchesBuild(6);
        }

        // SPS adds sockets for legacy markers during the build, without menu items.
        [Test]
        public void AutoUpgradedLegacySocket() {
            AddSocket();
            NewChild("OGB_Marker_Hole", NewChild("LegacyHole").transform);
            AssertEstimateMatchesBuild(3);
        }

        [Test]
        public void WithSpsOptions() {
            NewChild("Options").AddComponent<VF.Model.VRCFury>().content = new SpsOptions {
                saveSockets = true,
                legacyModeEnabledOnAvatarLoad = false
            };
            AddSocket();
            AssertEstimateMatchesBuild(3);
        }

        [Test]
        public void WithSpsMenus() {
            var menus = NewChild("SPS").AddComponent<VRCFurySpsMenus>();
            menus.saveSockets = true;
            AddSocket();
            AddPlug();
            AssertEstimateMatchesBuild(5);
        }

        // SPS-NDMF only runs the SPS part of the build, so other VRCFury features add nothing.
        [Test]
        public void WithNonSpsVrcFuryFeature() {
            NewChild("Toggle").AddComponent<VF.Model.VRCFury>().content = new Toggle { name = "Test" };
            AddSocket();
            AssertEstimateMatchesBuild(3);
        }
    }
}
