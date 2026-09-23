using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Nrzk.SpsMigrator.Copy;
using Nrzk.SpsMigrator.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VF.Component;
using VF.Upgradeable;
using Object = UnityEngine.Object;

namespace VfStubTests {
    internal class StubHostWindow : EditorWindow {
    }

    /**
     * Every test object lives in a scene of its own so the user's open scene is never dirtied. NewScene(Additive)
     * is refused while NDMF's preview scene (a scene without an asset) is loaded, so the scene is written to disk
     * as an empty scene file and opened from there. NDMF's scene preview is paused meanwhile: it captures
     * renderers and throws once a test destroys them.
     */
    public abstract class TempSceneFixture {
        protected const string TempDir = "Assets/_VfStubTests";
        protected const string ScenePath = TempDir + "/scene.unity";
        protected Scene scene;

        [SetUp]
        public void OpenTempScene() {
            if (AssetDatabase.IsValidFolder(TempDir)) AssetDatabase.DeleteAsset(TempDir);
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(TempDir));
            File.WriteAllText(ScenePath, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            AssetDatabase.ImportAsset(ScenePath);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            StubTestUtil.SetNdmfPreviewDisabled(true);
        }

        [TearDown]
        public void CloseTempScene() {
            try {
                StubTestUtil.DestroyHostedEditors();
                if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                if (AssetDatabase.IsValidFolder(TempDir)) AssetDatabase.DeleteAsset(TempDir);
            } finally {
                StubTestUtil.SetNdmfPreviewDisabled(false);
            }
        }

        protected static GameObject NewObject(string name) {
            return new GameObject(name);
        }
    }

    internal static class StubTestUtil {
        public const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static readonly Assembly VfAssembly = typeof(VRCFuryComponent).Assembly;
        public static readonly Type SpsNdmfComponentType = PackageBinding.FindType("SpsNdmf.Component.VRCFuryComponent");
        public static readonly Type SpsNdmfUpgradeableUtility = PackageBinding.FindType("SpsNdmf.Upgradeable.IUpgradeableUtility");

        // Same rule as StructuralFieldCopier.ShouldCopy, which is the rule the migrator relies on.
        public static IEnumerable<FieldInfo> SerializableFields(Type t) {
            var seen = new HashSet<string>();
            for (var cur = t; cur != null && cur != typeof(object); cur = cur.BaseType) {
                foreach (var f in cur.GetFields(FieldFlags | BindingFlags.DeclaredOnly)) {
                    if (f.IsStatic || Attribute.IsDefined(f, typeof(NonSerializedAttribute))) continue;
                    if (!f.IsPublic && !Attribute.IsDefined(f, typeof(SerializeField)) && !Attribute.IsDefined(f, typeof(SerializeReference))) continue;
                    if (seen.Add(f.Name)) yield return f;
                }
            }
        }

        // Leaf types only: a non-abstract base such as StateAction.Action is never instantiated by VRCFury itself,
        // and SPSNDMF rejects a bare instance of it.
        public static IEnumerable<Type> ConcreteSubtypes(Type baseType) {
            var all = VfAssembly.GetTypes();
            return all
                .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
                .Where(t => !all.Any(o => o != t && t.IsAssignableFrom(o)))
                .OrderBy(t => t.FullName);
        }

        public static Type Twin(Type vfType) {
            return new CopyContext().ResolveTargetType(vfType, SpsNdmfComponentType);
        }

        public static Type ElementType(Type t) {
            if (t.IsArray) return t.GetElementType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) return t.GetGenericArguments()[0];
            return null;
        }

        // Serialized state for comparison, one line per property in list order. EditorJsonUtility is unsuitable:
        // it writes scene references as instanceID 0 and lists [SerializeReference] instances in rid allocation
        // order, which changes whenever the object is rebuilt.
        public static string Json(Object o) {
            var lines = new List<string>();
            var prop = new SerializedObject(o).GetIterator();
            while (prop.Next(true)) {
                var path = prop.propertyPath;
                // Unity's own bookkeeping (script, owner, prefab links, hide flags): not component data.
                if (path.StartsWith("m_") && path != "m_Enabled") continue;
                string value;
                switch (prop.propertyType) {
                    case SerializedPropertyType.Integer: value = prop.longValue.ToString(); break;
                    case SerializedPropertyType.Boolean: value = prop.boolValue.ToString(); break;
                    case SerializedPropertyType.Float: value = prop.doubleValue.ToString("R"); break;
                    case SerializedPropertyType.String: value = prop.stringValue; break;
                    case SerializedPropertyType.Enum: value = prop.intValue.ToString(); break;
                    case SerializedPropertyType.ArraySize: value = prop.intValue.ToString(); break;
                    case SerializedPropertyType.ObjectReference: value = prop.objectReferenceInstanceIDValue.ToString(); break;
                    case SerializedPropertyType.ManagedReference: value = prop.managedReferenceFullTypename; break;
                    case SerializedPropertyType.Vector2: value = prop.vector2Value.ToString("R"); break;
                    case SerializedPropertyType.Vector3: value = prop.vector3Value.ToString("R"); break;
                    case SerializedPropertyType.Vector4: value = prop.vector4Value.ToString("R"); break;
                    case SerializedPropertyType.Quaternion: value = prop.quaternionValue.ToString("R"); break;
                    case SerializedPropertyType.Color: value = prop.colorValue.ToString("R"); break;
                    case SerializedPropertyType.Generic: value = ""; break;
                    default: value = prop.propertyType.ToString(); break;
                }
                lines.Add(path + "=" + value);
            }
            return string.Join("\n", lines);
        }

        public static void Walk(object root, Action<object> visit) {
            var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
            void Rec(object o, bool isRoot) {
                if (o == null || o is string || o.GetType().IsPrimitive || o.GetType().IsEnum) return;
                if (o is Object && !isRoot) return;
                if (!seen.Add(o)) return;
                visit(o);
                if (o is IList list) {
                    foreach (var e in list) Rec(e, false);
                    return;
                }
                foreach (var f in SerializableFields(o.GetType())) Rec(f.GetValue(o), false);
            }
            Rec(root, true);
        }

        public static void SetLatestVersions(object root) {
            Walk(root, o => {
                if (o is IUpgradeable u) u.Version = u.GetLatestVersion();
            });
        }

        public static void UpgradeSpsNdmf(Component created) {
            SpsNdmfUpgradeableUtility.GetMethod("UpgradeRecursive").Invoke(null, new object[] { created });
        }

        public static Component CopyToTwin(Component stub, GameObject holder, out CopyContext ctx) {
            ctx = new CopyContext();
            var twin = holder.AddComponent(Twin(stub.GetType()));
            StructuralFieldCopier.CopyFields(stub, twin, ctx, "to");
            return twin;
        }

        // NDMF's scene preview captures renderers and throws once a test destroys them; pause it while tests run.
        public static void SetNdmfPreviewDisabled(bool disabled) {
            var prop = PackageBinding.FindType("nadena.dev.ndmf.preview.NDMFPreview")?.GetProperty("DisablePreviewDepth");
            if (prop == null) return;
            prop.SetValue(null, (int)prop.GetValue(null) + (disabled ? 1 : -1));
        }

        /**
         * The inspector under test needs a live panel. Opening any window (floating or docked) brings Unity to the
         * front and steals focus from the user, so the tests borrow a window that is already showing: a container is
         * laid over the Scene view (or any other visible window) for the duration of the fixture. A window is
         * created only if nothing is showing at all.
         */
        public static VisualElement OpenHost(out EditorWindow window) {
            // A hidden dock tab has no panel, so only a window that is actually showing can host the UI.
            window = HasPanel(SceneView.lastActiveSceneView)
                ? SceneView.lastActiveSceneView
                : Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(HasPanel);
            if (window == null) window = EditorWindow.CreateWindow<StubHostWindow>(typeof(SceneView));
            var host = new VisualElement { name = "VfStubTestHost" };
            host.style.position = Position.Absolute;
            host.style.left = 0;
            host.style.top = 0;
            host.style.width = 420;
            window.rootVisualElement.Add(host);
            Assume.That(host.panel, Is.Not.Null, "no showing editor window could host the inspector under test");
            return host;
        }

        private static bool HasPanel(EditorWindow w) {
            return w != null && w.rootVisualElement.panel != null;
        }

        public static void CloseHost(VisualElement host, EditorWindow window) {
            host.RemoveFromHierarchy();
            if (window is StubHostWindow created) created.Close();
        }

        private static readonly List<UnityEditor.Editor> hostedEditors = new List<UnityEditor.Editor>();

        public static UnityEditor.Editor Host(VisualElement host, Component component) {
            var editor = UnityEditor.Editor.CreateEditor(component);
            hostedEditors.Add(editor);
            host.Clear();
            host.Add(new InspectorElement(editor));
            return editor;
        }

        public static void DestroyHostedEditors() {
            foreach (var e in hostedEditors) if (e != null) Object.DestroyImmediate(e);
            hostedEditors.Clear();
        }

        public static IEnumerable<string> Labels(VisualElement root) {
            return root.Query<TextElement>().ToList().Select(t => t.text);
        }

        // Structural description of a rendered inspector. The version/debug line differs between packages by design.
        // An empty label's visibility is left out: GuidWrapperPropertyDrawer hides its "last seen" label from a
        // value-changed callback that the initial binding fires on some frames and not others, on either side.
        public static string Describe(VisualElement root) {
            var lines = new List<string>();
            void Rec(VisualElement e, int depth) {
                if (e.ClassListContains("vfVersionLabel")) return;
                var text = e is TextElement te ? te.text : "";
                var binding = e is IBindable b ? b.bindingPath : "";
                var display = e is TextElement && text == "" ? "-" : e.resolvedStyle.display.ToString();
                lines.Add($"{new string(' ', depth)}{e.GetType().Name}|{text}|{binding}|{e.enabledInHierarchy}|{display}");
                foreach (var c in e.Children()) Rec(c, depth + 1);
            }
            Rec(root, 0);
            return string.Join("\n", lines);
        }

        private class ReferenceEqualityComparer : IEqualityComparer<object> {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object a, object b) => ReferenceEquals(a, b);
            public int GetHashCode(object o) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);
        }
    }

    /**
     * Fills every serializable field of an object graph with non-default values so a round trip has to carry
     * everything: primitives, enums, Unity structs, nested classes, lists (including [SerializeReference] lists,
     * which get one element per concrete subtype in turn) and Unity object references.
     */
    internal class Fuzzer : IDisposable {
        private readonly System.Random rng;
        private readonly GameObject holder;
        private readonly List<Object> created = new List<Object>();
        private int subtypeCursor;
        public readonly List<string> Unfilled = new List<string>();
        public readonly List<string> SkippedStubReferences = new List<string>();
        public readonly List<string> Unconstructible = new List<string>();
        public readonly List<string> Recursed = new List<string>();
        private readonly HashSet<Type> inProgress = new HashSet<Type>();

        public Fuzzer(int seed) {
            rng = new System.Random(seed);
            // Inactive so NDMF's preview pipeline never picks up the throwaway renderers.
            holder = new GameObject("FuzzAssets");
            holder.SetActive(false);
            created.Add(holder);
        }

        public void Dispose() {
            foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
        }

        public void Fill(object obj, int depth = 8, string path = "") {
            var type = obj.GetType();
            var entered = inProgress.Add(type);
            try {
                foreach (var f in StubTestUtil.SerializableFields(type)) {
                    var value = Make(f.FieldType, f.GetValue(obj), depth, path + "." + f.Name);
                    if (value != null) f.SetValue(obj, value);
                }
            } finally {
                if (entered) inProgress.Remove(type);
            }
        }

        private object Make(Type t, object current, int depth, string path) {
            if (t == typeof(string)) return "s" + rng.Next(1, 100000);
            if (t == typeof(bool)) return !(current is bool b && b);
            if (t == typeof(int)) return rng.Next(1, 100000);
            if (t == typeof(long)) return (long)rng.Next(1, 100000);
            if (t == typeof(float)) return (float)rng.Next(1, 100000) / 8f;
            if (t == typeof(double)) return rng.Next(1, 100000) / 8.0;
            if (t.IsEnum) {
                // Never the current value, so a lost enum shows up as a difference.
                var values = Enum.GetValues(t).Cast<object>().Where(v => !Equals(v, current)).ToList();
                return values.Count == 0 ? current : values[rng.Next(values.Count)];
            }
            if (t == typeof(Vector2)) return new Vector2(rng.Next(1, 100), rng.Next(1, 100));
            if (t == typeof(Vector3)) return new Vector3(rng.Next(1, 100), rng.Next(1, 100), rng.Next(1, 100));
            if (t == typeof(Vector4)) return new Vector4(rng.Next(1, 100), rng.Next(1, 100), rng.Next(1, 100), rng.Next(1, 100));
            if (t == typeof(Quaternion)) return Quaternion.Euler(rng.Next(0, 90), rng.Next(0, 90), rng.Next(0, 90));
            if (t == typeof(Color)) return new Color(rng.Next(0, 100) / 100f, rng.Next(0, 100) / 100f, rng.Next(0, 100) / 100f, 1);
            if (typeof(Object).IsAssignableFrom(t)) return Asset(t, path);

            var elementType = StubTestUtil.ElementType(t);
            if (elementType != null) {
                var count = 2;
                var items = new List<object>();
                for (var i = 0; i < count; i++) {
                    var item = Make(elementType, null, depth - 1, $"{path}[{i}]");
                    if (item == null) break;
                    items.Add(item);
                }
                if (t.IsArray) {
                    var arr = Array.CreateInstance(elementType, items.Count);
                    for (var i = 0; i < items.Count; i++) arr.SetValue(items[i], i);
                    return arr;
                }
                var list = (IList)Activator.CreateInstance(t);
                foreach (var i in items) list.Add(i);
                return list;
            }

            if (t.IsClass) {
                if (depth <= 0) { Unfilled.Add(path); return current; }
                var concrete = t;
                if (t.IsAbstract || t.Assembly == StubTestUtil.VfAssembly) {
                    var options = StubTestUtil.ConcreteSubtypes(t).ToList();
                    if (options.Count == 0) { Unfilled.Add(path); return null; }
                    concrete = options[subtypeCursor++ % options.Count];
                }
                if (concrete.GetConstructor(Type.EmptyTypes) == null) { Unconstructible.Add(path); return null; }
                // A recursive model (State -> Action -> State) is covered at its first level only.
                if (inProgress.Contains(concrete)) { Recursed.Add(path); return current; }
                var instance = Activator.CreateInstance(concrete);
                Fill(instance, depth - 1, path);
                return instance;
            }

            if (t.IsValueType && !t.IsPrimitive) {
                if (!StubTestUtil.SerializableFields(t).Any()) { Unfilled.Add(path); return null; }
                var boxed = current ?? Activator.CreateInstance(t);
                Fill(boxed, depth - 1, path);
                return boxed;
            }

            Unfilled.Add(path);
            return null;
        }

        private Object Asset(Type t, string path) {
            if (t.IsAssignableFrom(typeof(GameObject))) return holder;
            if (t.IsAssignableFrom(typeof(Transform))) return holder.transform;
            if (typeof(Component).IsAssignableFrom(t)) {
                // A reference to another stub component has no SpsNdmf counterpart on the fuzz holder.
                if (t.Assembly == StubTestUtil.VfAssembly) { SkippedStubReferences.Add(path); return null; }
                // Prefer a leaf subclass: Renderer is not abstract in C# but cannot be added as a component.
                var concrete = t.Assembly.GetTypes()
                    .FirstOrDefault(x => x != t && !x.IsAbstract && t.IsAssignableFrom(x) && x.GetCustomAttribute<ObsoleteAttribute>() == null) ?? t;
                if (concrete.IsAbstract) { Unfilled.Add(path); return null; }
                var go = new GameObject("FuzzAsset " + t.Name);
                go.transform.SetParent(holder.transform);
                var c = go.AddComponent(concrete);
                if (c == null) { Object.DestroyImmediate(go); Unfilled.Add(path); return null; }
                return c;
            }
            Object o = null;
            if (t == typeof(AnimationClip)) o = new AnimationClip();
            else if (t == typeof(Material)) o = new Material(Shader.Find("Standard"));
            else if (t == typeof(Texture2D) || t == typeof(Texture)) o = new Texture2D(2, 2);
            else if (t == typeof(RuntimeAnimatorController) || t == typeof(AnimatorController)) o = new AnimatorController();
            else if (t == typeof(Shader)) return Shader.Find("Standard");
            else if (typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract) o = ScriptableObject.CreateInstance(t);
            if (o == null) { Unfilled.Add(path); return null; }
            o.name = "FuzzAsset " + t.Name;
            created.Add(o);
            return o;
        }
    }
}
