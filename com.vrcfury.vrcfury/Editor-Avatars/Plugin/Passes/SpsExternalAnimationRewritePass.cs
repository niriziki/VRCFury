using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEngine;
using VF.Service;
using VF.Utils;
using VF.Utils.Controller;

namespace VF.Plugin.Passes {
    /// <summary>
    /// SpsBuildPass can only rewrite clips that SPS generated itself; user-authored
    /// clips (in the avatar's own controllers or delivered via MA Merge Animator)
    /// only become reachable after Modular Avatar merges them. This pass runs after
    /// MA and feeds the merged clips that target a plug or its renderer through the
    /// unmodified upstream rewrite (BakeHapticPlugsService.RewriteClipForSps) via a
    /// temporary VFClip, so Animated Toggle, resolver propagation, renderer-type
    /// retargeting, SPS blendshape remapping and material-swap patching also work
    /// for animations SPS did not create.
    /// </summary>
    internal class SpsExternalAnimationRewritePass : Pass<SpsExternalAnimationRewritePass> {
        public override string DisplayName => "SPS External Animation Rewrite";

        protected override void Execute(BuildContext context) {
            var spsCtx = context.GetState<SpsContext>();
            if (spsCtx?.Injector == null) return;
            var bake = spsCtx.Injector.GetService<BakeHapticPlugsService>();
            if (bake == null) return;
            var rewrites = bake.GetSpsRewrites();
            if (rewrites.Count == 0) return;

            var animatorServices = context.Extension<AnimatorServicesContext>();
            var remapper = animatorServices.ObjectPathRemapper;

            foreach (var rewrite in rewrites) {
                // MA passes running between SPS and this pass (e.g. Replace Object)
                // may have destroyed the baked objects. The upstream rewrite
                // dereferences them, so skip the entry instead of crashing; that
                // plug's external animations then stay unrewritten.
                if (rewrite.plugObject == null || rewrite.skin == null) {
                    Debug.LogWarning(
                        "SPS-NDMF: skipping external animation rewrite for an SPS plug whose baked objects were destroyed after SPS ran");
                    continue;
                }

                var targets = new Dictionary<string, VFGameObject>();
                void AddTarget(VFGameObject obj) {
                    if (obj != null) targets[remapper.GetVirtualPathForObject((GameObject)obj)] = obj;
                }
                AddTarget(rewrite.plugObject);
                AddTarget(rewrite.skin.owner());

                var clips = targets.Keys
                    .SelectMany(path => animatorServices.AnimationIndex.GetClipsForObjectPath(path))
                    .Distinct()
                    .ToArray();
                foreach (var clip in clips) {
                    RewriteVirtualClip(
                        clip,
                        targets,
                        bridge => bake.RewriteClipForSps(bridge, rewrite),
                        obj => remapper.GetVirtualPathForObject((GameObject)obj)
                    );
                }
            }
        }

        /// <summary>
        /// Bridges NDMF VirtualClips to the object-reference-based VFClip the
        /// upstream rewrite operates on: curves of <paramref name="clip"/> that
        /// target one of <paramref name="targets"/> (keyed by the path used inside
        /// the clip) are copied into a temporary VFClip bound to the live objects,
        /// <paramref name="rewriteVfClip"/> is applied, and the result replaces the
        /// copied curves, with objects translated back to clip paths via
        /// <paramref name="getVirtualPath"/>.
        /// </summary>
        internal static void RewriteVirtualClip(
            VirtualClip clip,
            IReadOnlyDictionary<string, VFGameObject> targets,
            Action<VFClip> rewriteVfClip,
            Func<VFGameObject, string> getVirtualPath
        ) {
            var bridge = VFClip.Create();
            var importedFloat = new List<EditorCurveBinding>();
            var importedObject = new List<EditorCurveBinding>();
            // Original bindings by (target, key type, property), so write-back can
            // return each curve with its exact EditorCurveBinding (a reconstructed
            // binding would drop flags like isDiscreteCurve that are part of
            // NDMF's curve identity).
            var importedBindings = new Dictionary<(VFGameObject, Type, string), EditorCurveBinding>();

            // NDMF treats bindings that differ only in those flags as separate
            // curves that may coexist on the same (path, type, property); the
            // VFClip bridge cannot carry that distinction, so only the first
            // curve per key is imported and later ones stay in the clip
            // untouched (unconverted but intact). MeshRenderer and
            // SkinnedMeshRenderer share one key: the upstream rewrite retargets
            // MeshRenderer bindings to SkinnedMeshRenderer, so type variants of
            // the same property would collide inside the bridge after that
            // conversion. MeshRenderer variants are imported with priority
            // because an unconverted MeshRenderer binding drives nothing once
            // the bake replaced the renderer, while an unconverted
            // SkinnedMeshRenderer binding still drives the live renderer.
            bool ShouldImport(EditorCurveBinding binding, out VFGameObject target) {
                if (!targets.TryGetValue(binding.path, out target)) return false;
                if (binding.type == null || binding.propertyName == null) return false;
                // The destination properties the rewrite can generate
                // (blendShape remapping) are never conversion inputs; importing
                // them could only collide with the generated curves, so leave
                // them be. MeshRenderer-typed ones are still imported so they
                // get the renderer retargeting; if one coincides with a
                // generated index, one of the two curves wins inside the bridge.
                if (binding.type != typeof(MeshRenderer)
                    && IsGeneratedBlendshapeProp(binding.propertyName)) return false;
                return !importedBindings.ContainsKey((target, KeyType(binding.type), binding.propertyName));
            }

            // One combined, MeshRenderer-first ordering across float and object
            // curves, so the MR-priority rule holds regardless of curve kind.
            var candidates = clip.GetFloatCurveBindings().Select(b => (binding: b, isFloat: true))
                .Concat(clip.GetObjectCurveBindings().Select(b => (binding: b, isFloat: false)))
                .OrderBy(c => c.binding.type == typeof(MeshRenderer) ? 0 : 1)
                .ToArray();
            foreach (var (binding, isFloat) in candidates) {
                if (!ShouldImport(binding, out var target)) continue;
                if (isFloat) {
                    bridge.SetCurve(target, binding.type, binding.propertyName, clip.GetFloatCurve(binding));
                    importedFloat.Add(binding);
                } else {
                    bridge.SetCurve(target, binding.type, binding.propertyName, clip.GetObjectCurve(binding));
                    importedObject.Add(binding);
                }
                importedBindings[(target, KeyType(binding.type), binding.propertyName)] = binding;
            }
            if (importedFloat.Count == 0 && importedObject.Count == 0) return;

            rewriteVfClip(bridge);

            foreach (var binding in importedFloat) clip.SetFloatCurve(binding, null);
            foreach (var binding in importedObject) clip.SetObjectCurve(binding, null);
            foreach (var (binding, curve) in bridge.GetAllCurves()) {
                if (binding.target == null) continue;
                var finalBinding = ResolveWriteBackBinding(binding, curve.IsFloat, importedBindings, getVirtualPath);
                if (curve.IsFloat) {
                    clip.SetFloatCurve(finalBinding, curve.FloatCurve);
                } else {
                    clip.SetObjectCurve(finalBinding, curve.ObjectCurve);
                }
            }
        }

        // MeshRenderer and SkinnedMeshRenderer are one key family because the
        // upstream rewrite converts exactly between those two; other
        // Renderer-derived types are never retargeted, so they keep their own
        // keys and their curves are imported independently. See ShouldImport.
        private static Type KeyType(Type type) {
            return type == typeof(MeshRenderer) ? typeof(SkinnedMeshRenderer) : type;
        }

        // Matches exactly material._SPS_Blendshape0 .. material._SPS_Blendshape15,
        // the only destination properties the rewrite can generate (16 slots).
        // Deliberately not a prefix match: _SPS_BlendshapeCount and the like are
        // real shader properties that must keep being imported normally.
        private static bool IsGeneratedBlendshapeProp(string propertyName) {
            const string prefix = "material._SPS_Blendshape";
            if (!propertyName.StartsWith(prefix)) return false;
            return int.TryParse(propertyName.Substring(prefix.Length), out var index)
                && index >= 0 && index <= 15;
        }

        private static EditorCurveBinding ResolveWriteBackBinding(
            VFBinding binding,
            bool isFloat,
            Dictionary<(VFGameObject, Type, string), EditorCurveBinding> importedBindings,
            Func<VFGameObject, string> getVirtualPath
        ) {
            // Same identity as an imported curve: hand its original binding back,
            // flags included. When the upstream rewrite retargeted the binding to
            // a different renderer type (MeshRenderer to SkinnedMeshRenderer),
            // copy the original struct and overwrite only its type, which keeps
            // the flags. The PPtr check guards against ever pairing a float curve
            // with an object-reference binding (NDMF rejects that).
            if (importedBindings.TryGetValue((binding.target, KeyType(binding.type), binding.propertyName), out var original)
                && original.isPPtrCurve == !isFloat) {
                if (original.type != binding.type) original.type = binding.type;
                return original;
            }
            return isFloat
                ? EditorCurveBinding.FloatCurve(getVirtualPath(binding.target), binding.type, binding.propertyName)
                : EditorCurveBinding.PPtrCurve(getVirtualPath(binding.target), binding.type, binding.propertyName);
        }
    }
}
