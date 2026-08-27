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

            foreach (var binding in clip.GetFloatCurveBindings().ToArray()) {
                if (!targets.TryGetValue(binding.path, out var target)) continue;
                if (binding.type == null || binding.propertyName == null) continue;
                bridge.SetCurve(target, binding.type, binding.propertyName, clip.GetFloatCurve(binding));
                importedFloat.Add(binding);
            }
            foreach (var binding in clip.GetObjectCurveBindings().ToArray()) {
                if (!targets.TryGetValue(binding.path, out var target)) continue;
                if (binding.type == null || binding.propertyName == null) continue;
                bridge.SetCurve(target, binding.type, binding.propertyName, clip.GetObjectCurve(binding));
                importedObject.Add(binding);
            }
            if (importedFloat.Count == 0 && importedObject.Count == 0) return;

            rewriteVfClip(bridge);

            foreach (var binding in importedFloat) clip.SetFloatCurve(binding, null);
            foreach (var binding in importedObject) clip.SetObjectCurve(binding, null);
            foreach (var (binding, curve) in bridge.GetAllCurves()) {
                if (binding.target == null) continue;
                var path = getVirtualPath(binding.target);
                if (curve.IsFloat) {
                    clip.SetFloatCurve(
                        EditorCurveBinding.FloatCurve(path, binding.type, binding.propertyName),
                        curve.FloatCurve
                    );
                } else {
                    clip.SetObjectCurve(
                        EditorCurveBinding.PPtrCurve(path, binding.type, binding.propertyName),
                        curve.ObjectCurve
                    );
                }
            }
        }
    }
}
