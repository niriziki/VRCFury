using System.Collections.Generic;
using VF.Injector;

namespace VF.Plugin {
    /// <summary>
    /// Shared state across NDMF passes during SPS build.
    /// Stored in BuildContext.GetState&lt;SpsContext&gt;().
    /// </summary>
    internal class SpsContext {
        public VRCFuryInjector Injector { get; set; }
        public NdmfAvatarOutput AvatarOutput { get; set; }

        /// <summary>
        /// Animator parameter renames that Modular Avatar will apply to the SPS output, captured
        /// before MA runs because it destroys the components they come from. Passes running after
        /// MA have to look up SPS parameter names through this to reach their final names.
        /// </summary>
        public IReadOnlyDictionary<string, string> ParameterRenames { get; set; }
    }
}
