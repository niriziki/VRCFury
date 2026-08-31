using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using VF.Component;
using VRC.SDK3.Avatars.Components;

namespace VF.Tests {
    /// <summary>
    /// Full NDMF build (through Transforming) verifying that a socket parented under the head
    /// still gets its first-person head chop entry. Upstream delivers this through a
    /// FeatureBuilder, which SpsServiceFilter rejects, so SpsSelfRequestedFeaturesService has to
    /// apply it instead.
    /// </summary>
    [Category("VRCFury")]
    public class SpsHeadChopBuildTest {
        private GameObject avatar;

        [SetUp]
        public void SetUp() {
            avatar = MaHierarchyResolverTest.BuildMinimalHumanoidAvatar();
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            if (avatar != null) Object.DestroyImmediate(avatar);
        }

        private GameObject AddSocket(string name, Transform parent) {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<VRCFuryHapticSocket>();
            return obj;
        }

        [Test]
        public void SocketUnderHeadKeepsItsHeadChopEntry() {
            var head = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            Assert.That(head, Is.Not.Null, "test avatar has no head bone");

            AddSocket("HeadSocket", head);
            AddSocket("HipsSocket", avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips));

            var processorType = typeof(BuildContext).Assembly.GetType("nadena.dev.ndmf.AvatarProcessor");
            var process = processorType
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => {
                    var ps = m.GetParameters();
                    return m.Name == "ProcessAvatar" && ps.Length == 2
                        && ps[0].ParameterType == typeof(GameObject) && ps[1].ParameterType == typeof(BuildPhase);
                });
            process.Invoke(null, new object[] { avatar, BuildPhase.Transforming });

            var targets = avatar.GetComponentsInChildren<VRCHeadChop>(true)
                .SelectMany(c => c.targetBones)
                .Where(b => b.transform != null)
                .ToArray();

            var headSocketBake = targets
                .Where(b => b.transform.name == "BakedSpsSocket")
                .Where(b => b.transform.IsChildOf(head))
                .ToArray();
            Assert.That(headSocketBake.Length, Is.EqualTo(1),
                "the head socket's bake root is not registered for a head chop. Registered: "
                + string.Join(", ", targets.Select(b => b.transform.name)));
            Assert.That(headSocketBake[0].applyCondition,
                Is.EqualTo(VRCHeadChop.HeadChopBone.ApplyCondition.AlwaysApply));

            // The hips socket is not under the head, so upstream would not register it either.
            Assert.That(targets.Any(b => !b.transform.IsChildOf(head)), Is.False,
                "a bake root outside the head was registered for a head chop");
        }
    }
}
