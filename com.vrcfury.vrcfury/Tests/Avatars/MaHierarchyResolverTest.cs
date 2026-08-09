using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using nadena.dev.modular_avatar.core;
using UnityEngine;
using VF.Utils;
using VRC.SDK3.Avatars.Components;

namespace VF.Tests {
    [Category("VRCFury")]
    public class MaHierarchyResolverTest {
        private GameObject avatar;
        private Animator animator;

        [SetUp]
        public void SetUp() {
            avatar = BuildMinimalHumanoidAvatar();
            animator = avatar.GetComponent<Animator>();
        }

        [TearDown]
        public void TearDown() {
            if (avatar != null) Object.DestroyImmediate(avatar);
        }

        private HumanBodyBones? GetClosestBone(GameObject obj) {
            return ClosestBoneUtils.GetPerFrame(avatar).GetClosestHumanoidBone(obj);
        }

        [Test]
        public void PlainObjectAtRootResolvesToNull() {
            var obj = NewChild(avatar.transform, "T_NoProxy");
            Assert.That(GetClosestBone(obj), Is.Null);
        }

        [Test]
        public void BoneProxyByBoneReferenceResolves() {
            var obj = NewChild(avatar.transform, "T_Proxy");
            var proxy = obj.AddComponent<ModularAvatarBoneProxy>();
            proxy.boneReference = HumanBodyBones.Hips;
            proxy.subPath = null;
            Assert.That(GetClosestBone(obj), Is.EqualTo(HumanBodyBones.Hips));
        }

        [Test]
        public void BoneProxyIntoMergeArmatureChainResolves() {
            var ofHips = BuildOutfitWithMergeArmature();
            var obj = NewChild(avatar.transform, "T_Chain");
            var proxy = obj.AddComponent<ModularAvatarBoneProxy>();
            proxy.target = ofHips.transform;
            Assert.That(GetClosestBone(obj), Is.EqualTo(HumanBodyBones.Hips));
        }

        [Test]
        public void ObjectUnderMergedOutfitBoneResolves() {
            var ofHips = BuildOutfitWithMergeArmature();
            var obj = NewChild(ofHips.transform, "T_Socket");
            Assert.That(GetClosestBone(obj), Is.EqualTo(HumanBodyBones.Hips));
        }

        [Test]
        public void ReplaceObjectResolvesToTargetBone() {
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var obj = NewChild(avatar.transform, "T_Replace");
            var replace = obj.AddComponent<ModularAvatarReplaceObject>();
            replace.targetObject.Set(head.gameObject);
            Assert.That(GetClosestBone(obj), Is.EqualTo(HumanBodyBones.Head));
        }

        [Test]
        public void CircularBoneProxiesTerminateWithNull() {
            var a = NewChild(avatar.transform, "T_CircularA");
            var b = NewChild(avatar.transform, "T_CircularB");
            var proxyA = a.AddComponent<ModularAvatarBoneProxy>();
            proxyA.boneReference = HumanBodyBones.LastBone;
            proxyA.subPath = "T_CircularB";
            var proxyB = b.AddComponent<ModularAvatarBoneProxy>();
            proxyB.boneReference = HumanBodyBones.LastBone;
            proxyB.subPath = "T_CircularA";
            Assert.That(GetClosestBone(a), Is.Null);
        }

        private GameObject BuildOutfitWithMergeArmature() {
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            var outfit = NewChild(avatar.transform, "Outfit");
            var ofArmature = NewChild(outfit.transform, "OF_Armature");
            var ofHips = NewChild(ofArmature.transform, "OF_Hips");
            var merge = ofArmature.AddComponent<ModularAvatarMergeArmature>();
            merge.mergeTarget.Set(hips.parent.gameObject);
            merge.prefix = "OF_";
            merge.suffix = "";
            return ofHips;
        }

        private static GameObject NewChild(Transform parent, string name) {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject BuildMinimalHumanoidAvatar() {
            var root = new GameObject("MaHierarchyResolverTestAvatar");
            var armature = NewChild(root.transform, "Armature");
            var bones = new Dictionary<string, Transform>();

            Transform Bone(string name, Transform parent, Vector3 localPos) {
                var go = NewChild(parent, name);
                go.transform.localPosition = localPos;
                bones[name] = go.transform;
                return go.transform;
            }

            var hips = Bone("Hips", armature.transform, new Vector3(0, 1, 0));
            var spine = Bone("Spine", hips, new Vector3(0, 0.1f, 0));
            Bone("Head", spine, new Vector3(0, 0.4f, 0));
            var lUpLeg = Bone("LeftUpperLeg", hips, new Vector3(-0.1f, -0.05f, 0));
            var lLowLeg = Bone("LeftLowerLeg", lUpLeg, new Vector3(0, -0.45f, 0));
            Bone("LeftFoot", lLowLeg, new Vector3(0, -0.45f, 0));
            var rUpLeg = Bone("RightUpperLeg", hips, new Vector3(0.1f, -0.05f, 0));
            var rLowLeg = Bone("RightLowerLeg", rUpLeg, new Vector3(0, -0.45f, 0));
            Bone("RightFoot", rLowLeg, new Vector3(0, -0.45f, 0));
            var lUpArm = Bone("LeftUpperArm", spine, new Vector3(-0.15f, 0.3f, 0));
            var lLowArm = Bone("LeftLowerArm", lUpArm, new Vector3(-0.25f, 0, 0));
            Bone("LeftHand", lLowArm, new Vector3(-0.25f, 0, 0));
            var rUpArm = Bone("RightUpperArm", spine, new Vector3(0.15f, 0.3f, 0));
            var rLowArm = Bone("RightLowerArm", rUpArm, new Vector3(0.25f, 0, 0));
            Bone("RightHand", rLowArm, new Vector3(0.25f, 0, 0));

            var skeleton = root.GetComponentsInChildren<Transform>()
                .Select(t => new SkeletonBone {
                    name = t.name,
                    position = t.localPosition,
                    rotation = t.localRotation,
                    scale = t.localScale
                })
                .ToArray();
            var human = bones.Keys
                .Select(name => new HumanBone {
                    humanName = name,
                    boneName = name,
                    limit = new HumanLimit { useDefaultValues = true }
                })
                .ToArray();
            var description = new HumanDescription {
                human = human,
                skeleton = skeleton,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0,
                hasTranslationDoF = false
            };
            var humanAvatar = AvatarBuilder.BuildHumanAvatar(root, description);
            Assert.That(humanAvatar.isValid, Is.True, "test humanoid avatar failed to build");

            var animator = root.AddComponent<Animator>();
            animator.avatar = humanAvatar;
            root.AddComponent<VRCAvatarDescriptor>();
            return root;
        }
    }
}
