using UnityEngine;
using VF.Component;
using VF.Utils;

namespace VF {
    internal static class VRCFurySpsGreenScreenFixEditor {
        [VFInit]
        private static void Init() {
            VRCFurySpsGreenScreenFix.onCreated = SaveMaterials;
        }

        private static void SaveMaterials(MeshRenderer renderer) {
            if (renderer == null) return;
            // SPS-NDMF: TmpDirPath is only set while an SPS NDMF build runs; without it GetPath() would throw
            // (e.g. entering play mode with an already-baked avatar and no SPS build in this session)
            if (TmpFilePackage.GetPathNullable() == null) return;

            var materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0) return;

            var dir = VRCFuryAssetDatabase.GetUniquePath(
                TmpFilePackage.GetPath() + "/Builds",
                "SpsGreenScreenFix"
            );
            VRCFuryAssetDatabase.CreateFolder(dir);

            for (var i = 0; i < materials.Length; i++) {
                var mat = materials[i];
                if (mat == null) continue;
                VRCFuryAssetDatabase.SaveAsset(mat, dir, $"{i}_{mat.shader.name}");
            }
        }
    }
}
