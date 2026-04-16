using VF.Injector;

namespace VF.Plugin {
    /// <summary>
    /// Shared state across NDMF passes during SPS build.
    /// Stored in BuildContext.GetState&lt;SpsContext&gt;().
    /// </summary>
    internal class SpsContext {
        public VRCFuryInjector Injector { get; set; }
        public NdmfAvatarOutput AvatarOutput { get; set; }
    }
}
