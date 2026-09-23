using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VF.Model;
using VF.Model.StateAction;
using Object = UnityEngine.Object;

namespace VfStubTests {
    /**
     * The stub ships the upstream VRCFury inspectors verbatim. These tests pin down that they render for the stub's
     * own types exactly as SPSNDMF's do, that edits and undo go through, and that the shipped editor assemblies
     * stay separated from SPSNDMF's in the same project (init hooks, Harmony, menus, settings files).
     */
    public class StubInspectorTests : TempSceneFixture {
        private VisualElement host;
        private EditorWindow window;
        private GameObject avatar;

        [OneTimeSetUp]
        public void OpenWindow() {
            host = StubTestUtil.OpenHost(out window);
        }

        [OneTimeTearDown]
        public void CloseWindow() {
            StubTestUtil.CloseHost(host, window);
        }

        [SetUp]
        public void SetUp() {
            avatar = MakeAvatar("Avatar");
        }

        // The last hosted inspector would otherwise keep ticking its schedulers against a destroyed target.
        [TearDown]
        public void ClearHost() {
            host.Clear();
        }

        // An Animator is enough for the editors to find the avatar root. A VRCAvatarDescriptor would also make
        // the VRChat SDK control panel (if open) validate the throwaway avatar and log its own errors.
        private static GameObject MakeAvatar(string name) {
            var root = new GameObject(name);
            root.AddComponent<Animator>();
            return root;
        }

        private static GameObject Child(GameObject parent, string name, bool cube = false) {
            var go = cube ? GameObject.CreatePrimitive(PrimitiveType.Cube) : new GameObject();
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private VF.Component.VRCFuryHapticSocket Socket(GameObject parent = null) {
            var socket = Child(parent ?? avatar, "socket").AddComponent<VF.Component.VRCFuryHapticSocket>();
            StubTestUtil.SetLatestVersions(socket);
            return socket;
        }

        private VF.Component.VRCFuryHapticPlug Plug(GameObject parent = null) {
            var plug = Child(parent ?? avatar, "plug", cube: true).AddComponent<VF.Component.VRCFuryHapticPlug>();
            StubTestUtil.SetLatestVersions(plug);
            return plug;
        }

        // A depth action set reaches the action editors, which need the avatar injected by the shipped hook.
        private static void AddDepthActions(VF.Component.VRCFuryHapticSocket socket, VF.Component.VRCFuryHapticPlug target) {
            socket.enableDepthAnimations = true;
            socket.depthActions2.Add(new VF.Component.VRCFuryHapticSocket.DepthActionNew {
                actionSet = new State { actions = {
                    new SpsOnAction { target = target },
                    new ObjectToggleAction { obj = target.gameObject },
                    new BlendShapeAction { blendShape = "x" },
                    new AnimationClipAction(),
                    new MaterialPropertyAction { propertyName = "_X" },
                } }
            });
        }

        // Some sections resolve on later frames (scheduled refreshes, bake previews); give both sides the same time.
        private IEnumerator Show(Component c) {
            StubTestUtil.Host(host, c);
            for (var i = 0; i < 6; i++) yield return null;
        }

        private string Rendered() {
            return StubTestUtil.Describe(host);
        }

        [UnityTest]
        public IEnumerator PlugAndSocketUseTheUpstreamEditorsUnderTheStubBrand() {
            var socket = Socket();
            yield return Show(socket);
            var labels = StubTestUtil.Labels(host).ToList();
            Assert.That(StubTestUtil.Host(host, socket).GetType().FullName, Is.EqualTo("VF.Inspector.VRCFuryHapticSocketEditor"));
            // The brand badge is only overlaid on the Inspector window's own header; a hosted editor shows the title.
            Assert.That(labels, Does.Contain("SPS Socket (VRCFuryStub)"));
            Assert.That(labels, Has.None.Contains("Failed to render editor"));
            Assert.That(labels, Has.None.Contains("not available in this type of project"));

            var plug = Plug();
            yield return Show(plug);
            labels = StubTestUtil.Labels(host).ToList();
            Assert.That(StubTestUtil.Host(host, plug).GetType().FullName, Is.EqualTo("VF.Inspector.VRCFuryHapticPlugEditor"));
            Assert.That(labels, Does.Contain("SPS Plug (VRCFuryStub)"));
            Assert.That(labels, Has.None.Contains("Failed to render editor"));
        }

        private static string Normalize(string described) {
            return described.Replace("(VRCFuryStub)", "(SPSNDMF)").Replace("|VRCFuryStub|", "|SPSNDMF|");
        }

        [UnityTest]
        public IEnumerator ASocketRendersTheSameTreeAsItsSpsNdmfTwin([Values(false, true)] bool fuzzed) {
            var socket = Socket();
            var plug = Plug();
            AddDepthActions(socket, plug);
            if (fuzzed) {
                using (var fuzzer = new Fuzzer(11)) {
                    fuzzer.Fill(socket);
                    StubTestUtil.SetLatestVersions(socket);
                    yield return Compare(socket);
                }
            } else {
                yield return Compare(socket);
            }
        }

        [UnityTest]
        public IEnumerator APlugRendersTheSameTreeAsItsSpsNdmfTwin() {
            var plug = Plug();
            plug.enableSps = true;
            plug.spsAnimatedEnabled = 0.5f;
            yield return Compare(plug);
        }

        // Mirrors the stub's avatar: same hierarchy, each VF component replaced by its SpsNdmf twin, and every
        // reference into the stub avatar re-pointed at the mirrored object.
        private static Component MirrorAvatar(GameObject avatar, Component stub) {
            var mirror = Object.Instantiate(avatar);
            mirror.name = avatar.name;
            var byOriginal = new Dictionary<Object, Object>();
            var originals = avatar.GetComponentsInChildren<Transform>(true);
            var copies = mirror.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < originals.Length; i++) {
                byOriginal[originals[i].gameObject] = copies[i].gameObject;
                byOriginal[originals[i]] = copies[i];
            }
            Component twinOfStub = null;
            foreach (var vf in avatar.GetComponentsInChildren<VF.Component.VRCFuryComponent>(true)) {
                var copyGo = (GameObject)byOriginal[vf.gameObject];
                foreach (var stale in copyGo.GetComponents<VF.Component.VRCFuryComponent>()) Object.DestroyImmediate(stale);
                var twin = copyGo.AddComponent(StubTestUtil.Twin(vf.GetType()));
                var ctx = new Nrzk.SpsMigrator.Copy.CopyContext();
                Nrzk.SpsMigrator.Copy.StructuralFieldCopier.CopyFields(vf, twin, ctx, vf.name);
                byOriginal[vf] = twin;
                if (vf == stub) twinOfStub = twin;
            }
            foreach (var twin in mirror.GetComponentsInChildren<Component>(true)) {
                var so = new SerializedObject(twin);
                var prop = so.GetIterator();
                var changed = false;
                while (prop.Next(true)) {
                    if (prop.propertyType != SerializedPropertyType.ObjectReference || prop.objectReferenceValue == null) continue;
                    if (!byOriginal.TryGetValue(prop.objectReferenceValue, out var mapped)) continue;
                    prop.objectReferenceValue = mapped;
                    changed = true;
                }
                if (changed) so.ApplyModifiedPropertiesWithoutUndo();
            }
            return twinOfStub;
        }

        private IEnumerator Compare(Component stub) {
            var twin = MirrorAvatar(avatar, stub);
            Assert.That(twin, Is.Not.Null);

            yield return Show(stub);
            var stubTree = Normalize(Rendered());
            Assert.That(stubTree, Does.Not.Contain("Failed to render editor"));

            yield return Show(twin);
            var twinTree = Rendered();
            Assert.That(twinTree, Does.Not.Contain("Failed to render editor"));
            Assert.That(StubTestUtil.Host(host, twin).GetType().Namespace, Does.StartWith("SpsNdmf."), "SPSNDMF's component must not pick up the stub's editor");

            yield return Show(stub);
            var stubTreeAgain = Normalize(Rendered());

            if (stubTree != twinTree || stubTreeAgain != twinTree) {
                System.IO.File.WriteAllText($"Temp/vfstub-tree-{stub.GetType().Name}-stub.txt", stubTree);
                System.IO.File.WriteAllText($"Temp/vfstub-tree-{stub.GetType().Name}-twin.txt", twinTree);
                System.IO.File.WriteAllText($"Temp/vfstub-tree-{stub.GetType().Name}-stub2.txt", stubTreeAgain);
            }
            Assert.That(stubTreeAgain, Is.EqualTo(twinTree), "second display of the stub differs from the twin, see Temp/vfstub-tree-*.txt");
            Assert.That(stubTree, Is.EqualTo(twinTree), "first display of the stub differs from the twin, see Temp/vfstub-tree-*.txt");
        }

        [UnityTest]
        public IEnumerator EditsThroughTheInspectorWriteToTheComponentAndUndoRestoresThem() {
            var socket = Socket();
            socket.enableAuto = true;
            Undo.IncrementCurrentGroup();
            yield return Show(socket);
            var toggle = host.Query<Toggle>().ToList().FirstOrDefault(t => t.bindingPath == "enableAuto");
            Assert.That(toggle, Is.Not.Null, "no bound field for enableAuto:\n" + Rendered());
            Assert.That(toggle.enabledInHierarchy, Is.True);

            toggle.value = false;
            yield return null;
            Assert.That(socket.enableAuto, Is.False, "the bound field did not write to the component");

            Undo.PerformUndo();
            yield return null;
            Assert.That(socket.enableAuto, Is.True, "undo did not restore the component");
        }

        [UnityTest]
        public IEnumerator PrefabInstancesAreShownReadOnlyLikeUpstream() {
            var source = new GameObject("prefabSocket");
            source.AddComponent<VF.Component.VRCFuryHapticSocket>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(source, TempDir + "/socket.prefab");
            Object.DestroyImmediate(source);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetParent(avatar.transform, false);
            var socket = instance.GetComponent<VF.Component.VRCFuryHapticSocket>();

            yield return Show(socket);
            var toggle = host.Query<Toggle>().ToList().FirstOrDefault(t => t.bindingPath == "enableAuto");
            Assert.That(toggle, Is.Not.Null, Rendered());
            Assert.That(toggle.enabledInHierarchy, Is.False, "a prefab instance must render as a read-only preview");
            Assert.That(StubTestUtil.Labels(host), Has.Some.Contains("prefab").IgnoreCase);
        }

        [UnityTest]
        public IEnumerator TheFeatureContainerShowsPlainFieldsAndOtherComponentsKeepTheirUpstreamEditors() {
            var vrcf = Child(avatar, "features").AddComponent<VRCFury>();
            vrcf.content = new VF.Model.Feature.Toggle { name = "t" };
            yield return Show(vrcf);
            Assert.That(StubTestUtil.Host(host, vrcf).GetType().FullName, Is.EqualTo("VF.Inspector.VRCFuryStubEditor"));
            Assert.That(StubTestUtil.Labels(host), Has.Some.Contains("does not build it"));
            Assert.That(StubTestUtil.Labels(host), Has.None.Contains("Failed to render editor"));

            var collider = Child(avatar, "collider").AddComponent<VF.Component.VRCFuryGlobalCollider>();
            yield return Show(collider);
            Assert.That(StubTestUtil.Host(host, collider).GetType().FullName, Is.EqualTo("VF.Inspector.VRCFGlobalColliderEditor"));
            Assert.That(StubTestUtil.Labels(host), Has.None.Contains("Failed to render editor"));
        }

        [UnityTest]
        public IEnumerator RenderingWritesNoAssets() {
            var before = AssetDatabase.FindAssets("", new[] { "Assets", "Packages/net.nrzk.vfstub" }).Length;
            var socket = Socket();
            var plug = Plug();
            AddDepthActions(socket, plug);
            yield return Show(socket);
            yield return Show(plug);
            AssetDatabase.Refresh();
            Assert.That(AssetDatabase.FindAssets("", new[] { "Assets", "Packages/net.nrzk.vfstub" }).Length, Is.EqualTo(before));
        }

        // Everything that runs on load in the stub. A new [VFInit] arriving from upstream lands here for review.
        private static readonly string[] ExpectedVfInit = {
            "VF.Builder.VRCFArmatureCache.Init",
            "VF.Builder.VRCFObjectPathCache.Init",
            "VF.Component.VRCFuryHideGizmoUnlessSelectedEditor.Init",
            "VF.Component.VRCFuryPlayComponentEditor.Init",
            "VF.GuidWrapperExtensions.Init",
            "VF.Hooks.VRCFuryAvatarHook.Init",
            "VF.Hooks.VrcsdkFixes.DisableVpmResolverAutoInitHook.Init",
            "VF.Injector.VRCFuryPerFrameInjector.Init",
            "VF.Inspector.VRCFuryHapticPlaySocketEditor.Init",
            "VF.Menu.ConstrainedProportionsMenuItem.Init",
            "VF.Utils.ClosestBoneUtils.Init",
            "VF.Utils.CollapseUtils.Init",
            "VF.Utils.Controller.VFLayer.ClearCreatedStates",
            "VF.Utils.DirtyUtils.MakeMarkDirtyAvailableToRuntime",
            "VF.Utils.EditorWindowFinder.Init",
            "VF.Utils.HarmonyUtils.Init",
            "VF.Utils.ObjectExtensions.Init",
            "VF.Utils.PoiyomiUtils.Init",
            "VF.Utils.RecorderUtils.Init",
            "VF.Utils.ReflectionHelper.Init",
            "VF.Utils.RendererExtensions.Init",
            "VF.Utils.Scheduler.Init",
            "VF.Utils.VrcfObjectCloner.Init",
            "VF.Utils.VrcfObjectFactory.OnLoad",
            "VF.VRCFPackageUtils.SendToComponents",
            "VF.VRCFProgressWindow.Init",
            "VF.VRCFurySpsGreenScreenFixEditor.Init",
        };

        [Test]
        public void OnlyTheReviewedInitHooksRunOnLoad() {
            var attr = Type.GetType("VF.Utils.VFInitAttribute, VRCFury-Editor-Common");
            Assert.That(attr, Is.Not.Null);
            var actual = TypeCache.GetMethodsWithAttribute(attr).Select(m => m.DeclaringType.FullName + "." + m.Name).OrderBy(x => x).ToArray();
            Assert.That(actual, Is.EqualTo(ExpectedVfInit.OrderBy(x => x).ToArray()));
        }

        [Test]
        public void TheStubsHarmonyInstanceIsSeparateAndPatchesNothingOfUnity() {
            var utils = Type.GetType("VF.Utils.HarmonyUtils, VRCFury-Editor-Common");
            var harmony = (HarmonyLib.Harmony)utils.GetField("harmony", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            Assert.That(harmony.Id, Is.EqualTo("com.vrcfury.stub.harmony"));
            var patched = harmony.GetPatchedMethods().Select(m => m.DeclaringType.FullName + "." + m.Name).ToList();
            Assert.That(patched, Is.EqualTo(new[] { "VF.Utils.HarmonyTest.TestPatchTarget" }));
        }

        [Test]
        public void MenusLiveUnderTheStubPrefixOnly() {
            var mine = Unsupported.GetSubmenus("Tools").Where(m => m.Contains("VRCFury")).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            Assert.That(mine, Is.EqualTo(new[] {
                "Tools/VRCFury Stub/SPS/Allow Editing on Prefab Instances",
                "Tools/VRCFury Stub/SPS/Create Plug",
                "Tools/VRCFury Stub/SPS/Create Socket",
                "Tools/VRCFury Stub/SPS/Migrate Project Data",
                "Tools/VRCFury Stub/Settings/Enable SPS Haptics",
            }));
        }

        [Test]
        public void ThePrefabInstanceSettingHasItsOwnProjectFile() {
            var type = Type.GetType("VF.Prefabs.PrefabInstanceMode, VRCFury-Editor-Common");
            var filePath = type.GetCustomAttributes(true).First(a => a.GetType().Name == "FilePathAttribute");
            var path = (string)filePath.GetType().GetProperty("filepath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(filePath);
            Assert.That(path, Does.EndWith("ProjectSettings/VfStubPrefabInstanceMode.asset"));
        }

        [Test]
        public void TheStubStampsItsOwnVersionOnSave() {
            var utils = Type.GetType("VF.VRCFPackageUtils, VRCFury-Editor-Avatars");
            var version = (string)utils.GetProperty("Version", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            Assert.That(version, Is.Not.Empty);
            Assert.That(version, Is.EqualTo(UnityEditor.PackageManager.PackageInfo.FindForAssembly(StubTestUtil.VfAssembly).version));
        }
    }
}
