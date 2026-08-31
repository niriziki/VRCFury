using System;
using System.Collections.Immutable;
using VF.Feature.Base;
using VF.Service;

namespace VF.Plugin {
    /// <summary>
    /// Allowlist of types whose FeatureBuilderActions may run in NDMF mode.
    /// Types not listed here are skipped to ensure SPS processing does not
    /// modify anything unrelated to SPS on the avatar.
    ///
    /// NdmfAvatarOutput isolation (no Descriptor fallback) ensures these
    /// services operate on fresh controllers/menu/params. This allowlist
    /// additionally prevents non-SPS hierarchy and Descriptor changes.
    /// </summary>
    internal static class SpsServiceFilter {
        private static readonly ImmutableHashSet<Type> AllowedTypes = ImmutableHashSet.Create<Type>(
            // ---- SPS core ----
            typeof(SpsSendersForAllService),
            typeof(BakeHapticPlugsService),
            typeof(BakeHapticSocketsService),
            typeof(BakeHapticVersionsService),
            typeof(OverlappingContactsFixService),

            // ---- SPS infrastructure ----
            // CleanTmpDirService: upstream moved this from a constructor to a
            // FeatureBuilderAction, so it must be allowed to keep cleaning the temp package.
            typeof(CleanTmpDirService),
            // ObjectCacheService: upstream moved the object path / armature capture out of
            // ApplyFuryConfigs into a FeatureBuilderAction, so it must be allowed to keep running.
            typeof(ObjectCacheService),
            typeof(IsObjectEnabledService),
            typeof(NdmfAnimatorSafetyService),
            typeof(RestingStateService),
            typeof(AvatarBindingStateService),
            typeof(SaveAssetsService),
            typeof(NoBadControllerParamsService),
            typeof(FixPartiallyWeightedAapsService),
            typeof(DisableSyncForAapsService),
            typeof(SpsDeclareExternalAapsService),

            // ---- SPS menu ----
            typeof(SpsOptionsService),
            typeof(MenuChangesService),

            // ---- Controller/menu/params finalization ----
            // Safe: these operate through ControllersService/MenuService/ParamsService,
            // which are isolated by NdmfAvatarOutput (SPS-generated data only).
            typeof(FixWriteDefaultsService),
            typeof(CleanupEmptyLayersService),
            typeof(FixMasksService),
            typeof(FixEmptyMotionService),
            typeof(LayerToTreeService),
            typeof(TreeFlatteningService),
            typeof(FixTreeLengthService),
            typeof(MakeControllerNamesUniqueService),
            typeof(MakeAllSyncedDriversLocalService),
            typeof(RemoveVrcGlobalsFromExpressionParamsService),
            typeof(ActionConflictResolverService),
            typeof(TrackingConflictResolverService),
            typeof(FloatToDriverService),
            typeof(AnimatorLayerControlOffsetService),
            typeof(FinalizeMenuService),
            typeof(FixMenuIconTexturesService),

            // ---- Validation (read-only, no side effects) ----
            typeof(FinalValidationService)
        );

        public static bool IsAllowed(object service) {
            if (service is FeatureBuilder) return false;
            return AllowedTypes.Contains(service.GetType());
        }
    }
}
