namespace VF {
    /**
     * SPS-NDMF: patched shaders are content-addressed by their input hash, so they are a
     * project-wide cache rather than a per-build artifact. NDMF gives every build its own
     * asset container directory, so keeping them there would recompile every shader on
     * every build. They live in a fixed directory instead, next to NDMF's own bake output
     * so that clearing that folder clears the shaders along with the avatars using them.
     */
    internal static class SpsShaderCache {
        public const string Dir = "Assets/ZZZ_GeneratedAssets/spsndmf-shaders/SPS";
    }
}
