using JetBrains.Annotations;
using nadena.dev.modular_avatar.core;

namespace VF.Utils {
    /**
     * Modular Avatar のコンポーネント（Bone Proxy / Replace Object / Merge Armature）によって
     * ビルド時に移動されるオブジェクトについて、移動後に実質の親系譜となる位置を返す。
     * SPSはMAより前に実行されるため、closest-bone判定はこれで移動先を先読みする必要がある。
     */
    internal static class MaHierarchyResolver {
        [CanBeNull]
        public static VFGameObject GetProbableParent(VFGameObject current) {
            var boneProxy = current.GetComponent<ModularAvatarBoneProxy>();
            if (boneProxy != null) {
                VFGameObject target = boneProxy.target;
                if (target != null && target != current) return target;
            }

            var replaceObject = current.GetComponent<ModularAvatarReplaceObject>();
            if (replaceObject != null) {
                VFGameObject target = replaceObject.targetObject?.Get(replaceObject);
                if (target != null && target != current) return target;
            }

            var mergeArmature = current.GetComponentInSelfOrParent<ModularAvatarMergeArmature>();
            if (mergeArmature != null) {
                var mapped = MapMergedBone(mergeArmature, current);
                if (mapped != null && mapped != current) return mapped;
            }

            return null;
        }

        /**
         * Merge Armature 配下の衣装ボーンを、prefix/suffix除去の名前照合で本体側の対応ボーンへ写す。
         * 途中で照合に失敗したら、解決できた最深の本体ボーンを返す（closest-bone用途では十分）。
         * MAのファジーマッチ（HeuristicBoneMapper）は再実装しない。
         */
        [CanBeNull]
        private static VFGameObject MapMergedBone(ModularAvatarMergeArmature mergeArmature, VFGameObject current) {
            VFGameObject mergeRoot = mergeArmature.transform;
            VFGameObject baseBone = mergeArmature.mergeTargetObject;
            if (baseBone == null) return null;
            if (current == mergeRoot) return baseBone;

            var segments = new System.Collections.Generic.List<string>();
            for (var walk = current; walk != null && walk != mergeRoot; walk = walk.parent) {
                segments.Add(walk.name);
            }
            segments.Reverse();

            foreach (var segment in segments) {
                var stripped = segment;
                if (!string.IsNullOrEmpty(mergeArmature.prefix) && stripped.StartsWith(mergeArmature.prefix)) {
                    stripped = stripped.Substring(mergeArmature.prefix.Length);
                }
                if (!string.IsNullOrEmpty(mergeArmature.suffix) && stripped.EndsWith(mergeArmature.suffix)) {
                    stripped = stripped.Substring(0, stripped.Length - mergeArmature.suffix.Length);
                }
                var next = baseBone.Find(stripped) ?? baseBone.Find(segment);
                if (next == null) break;
                baseBone = next;
            }
            return baseBone;
        }
    }
}
