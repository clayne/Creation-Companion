﻿using System;
using System.Collections.Generic;
using CreationEditor.Avalonia.Models.Record.Browser;
using CreationEditor.Avalonia.Services.Record.Browser;
using Loqui;
using Mutagen.Bethesda.Skyrim;
namespace CreationEditor.Skyrim.Avalonia.Services.Record.Browser;

public sealed class SkyrimRecordBrowserGroupProvider(
    Func<ILoquiRegistration, string?, RecordTypeListing> recordTypeListingFactory)
    : IRecordBrowserGroupProvider {

    public List<RecordTypeGroup> GetRecordGroups() {
        return [
            new RecordTypeGroup("Actors", [
                recordTypeListingFactory(INpcGetter.StaticRegistration, null),
                recordTypeListingFactory(IActionRecordGetter.StaticRegistration, null),
                recordTypeListingFactory(IBodyPartDataGetter.StaticRegistration, null),
                recordTypeListingFactory(ILeveledNpcGetter.StaticRegistration, null),
                recordTypeListingFactory(IPerkGetter.StaticRegistration, null),
                recordTypeListingFactory(ITalkingActivatorGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("Audio", [
                recordTypeListingFactory(IAcousticSpaceGetter.StaticRegistration, null),
                recordTypeListingFactory(IMusicTrackGetter.StaticRegistration, null),
                recordTypeListingFactory(IMusicTypeGetter.StaticRegistration, null),
                recordTypeListingFactory(IReverbParametersGetter.StaticRegistration, null),
                recordTypeListingFactory(ISoundCategoryGetter.StaticRegistration, null),
                recordTypeListingFactory(ISoundDescriptorGetter.StaticRegistration, null),
                recordTypeListingFactory(ISoundMarkerGetter.StaticRegistration, null),
                recordTypeListingFactory(ISoundOutputModelGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("Character", [
                recordTypeListingFactory(IAssociationTypeGetter.StaticRegistration, null),
                recordTypeListingFactory(IClassGetter.StaticRegistration, null),
                recordTypeListingFactory(IEquipTypeGetter.StaticRegistration, null),
                recordTypeListingFactory(IFactionGetter.StaticRegistration, null),
                recordTypeListingFactory(IHeadPartGetter.StaticRegistration, null),
                recordTypeListingFactory(IMovementTypeGetter.StaticRegistration, null),
                recordTypeListingFactory(IPackageGetter.StaticRegistration, null),
                recordTypeListingFactory(IQuestGetter.StaticRegistration, null),
                recordTypeListingFactory(IRaceGetter.StaticRegistration, null),
                recordTypeListingFactory(IRelationshipGetter.StaticRegistration, null),
                recordTypeListingFactory(IStoryManagerEventNodeGetter.StaticRegistration, "Story Manager"),
                recordTypeListingFactory(IVoiceTypeGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("Items", [
                recordTypeListingFactory(IAmmunitionGetter.StaticRegistration, null),
                recordTypeListingFactory(IArmorGetter.StaticRegistration, null),
                recordTypeListingFactory(IArmorAddonGetter.StaticRegistration, null),
                recordTypeListingFactory(IBookGetter.StaticRegistration, null),
                recordTypeListingFactory(IConstructibleObjectGetter.StaticRegistration, null),
                recordTypeListingFactory(IIngredientGetter.StaticRegistration, null),
                recordTypeListingFactory(IKeyGetter.StaticRegistration, null),
                recordTypeListingFactory(ILeveledItemGetter.StaticRegistration, null),
                recordTypeListingFactory(IMiscItemGetter.StaticRegistration, null),
                recordTypeListingFactory(IOutfitGetter.StaticRegistration, null),
                recordTypeListingFactory(ISoulGemGetter.StaticRegistration, null),
                recordTypeListingFactory(IWeaponGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("Magic", [
                recordTypeListingFactory(IDualCastDataGetter.StaticRegistration, null),
                recordTypeListingFactory(IObjectEffectGetter.StaticRegistration, "Enchantment"),
                recordTypeListingFactory(ILeveledSpellGetter.StaticRegistration, null),
                recordTypeListingFactory(IMagicEffectGetter.StaticRegistration, null),
                recordTypeListingFactory(IIngestibleGetter.StaticRegistration, null),
                recordTypeListingFactory(IScrollGetter.StaticRegistration, null),
                recordTypeListingFactory(IShoutGetter.StaticRegistration, null),
                recordTypeListingFactory(ISpellGetter.StaticRegistration, null),
                recordTypeListingFactory(IWordOfPowerGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("Miscellaneous", [
                recordTypeListingFactory(IAnimatedObjectGetter.StaticRegistration, null),
                recordTypeListingFactory(IArtObjectGetter.StaticRegistration, null),
                recordTypeListingFactory(ICollisionLayerGetter.StaticRegistration, null),
                recordTypeListingFactory(IColorRecordGetter.StaticRegistration, "Color"),
                recordTypeListingFactory(ICombatStyleGetter.StaticRegistration, null),
                recordTypeListingFactory(IFormListGetter.StaticRegistration, null),
                recordTypeListingFactory(IGlobalGetter.StaticRegistration, null),
                recordTypeListingFactory(IKeywordGetter.StaticRegistration, null),
                recordTypeListingFactory(ILandscapeTextureGetter.StaticRegistration, null),
                recordTypeListingFactory(ILoadScreenGetter.StaticRegistration, null),
                recordTypeListingFactory(IMaterialObjectGetter.StaticRegistration, null),
                recordTypeListingFactory(IMessageGetter.StaticRegistration, null),
                recordTypeListingFactory(ITextureSetGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("Special Effects", [
                recordTypeListingFactory(IAddonNodeGetter.StaticRegistration, null),
                recordTypeListingFactory(ICameraShotGetter.StaticRegistration, null),
                recordTypeListingFactory(IDebrisGetter.StaticRegistration, null),
                recordTypeListingFactory(IEffectShaderGetter.StaticRegistration, null),
                recordTypeListingFactory(IExplosionGetter.StaticRegistration, null),
                recordTypeListingFactory(IFootstepGetter.StaticRegistration, null),
                recordTypeListingFactory(IFootstepSetGetter.StaticRegistration, null),
                recordTypeListingFactory(IHazardGetter.StaticRegistration, null),
                recordTypeListingFactory(IImageSpaceGetter.StaticRegistration, "Imagespace"),
                recordTypeListingFactory(IImageSpaceAdapterGetter.StaticRegistration, "Imagespace Modifier"),
                recordTypeListingFactory(IImpactGetter.StaticRegistration, null),
                recordTypeListingFactory(IImpactDataSetGetter.StaticRegistration, null),
                recordTypeListingFactory(IMaterialTypeGetter.StaticRegistration, null),
                recordTypeListingFactory(IProjectileGetter.StaticRegistration, null),
                recordTypeListingFactory(IVolumetricLightingGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("World Data", [
                recordTypeListingFactory(IClimateGetter.StaticRegistration, null),
                recordTypeListingFactory(IEncounterZoneGetter.StaticRegistration, null),
                recordTypeListingFactory(ILightingTemplateGetter.StaticRegistration, null),
                recordTypeListingFactory(ILocationGetter.StaticRegistration, null),
                recordTypeListingFactory(ILocationReferenceTypeGetter.StaticRegistration, null),
                recordTypeListingFactory(IShaderParticleGeometryGetter.StaticRegistration, null),
                recordTypeListingFactory(IVisualEffectGetter.StaticRegistration, null),
                recordTypeListingFactory(IWaterGetter.StaticRegistration, null),
                recordTypeListingFactory(IWeatherGetter.StaticRegistration, null)
            ]),
            new RecordTypeGroup("World Objects", [
                recordTypeListingFactory(IActivatorGetter.StaticRegistration, null),
                recordTypeListingFactory(IContainerGetter.StaticRegistration, null),
                recordTypeListingFactory(IDoorGetter.StaticRegistration, null),
                recordTypeListingFactory(IFloraGetter.StaticRegistration, null),
                recordTypeListingFactory(IFurnitureGetter.StaticRegistration, null),
                recordTypeListingFactory(IIdleMarkerGetter.StaticRegistration, null),
                recordTypeListingFactory(IGrassGetter.StaticRegistration, null),
                recordTypeListingFactory(ILightGetter.StaticRegistration, null),
                recordTypeListingFactory(IMoveableStaticGetter.StaticRegistration, null),
                recordTypeListingFactory(IStaticGetter.StaticRegistration, null),
                recordTypeListingFactory(ITreeGetter.StaticRegistration, null)
            ])

        ];
    }
}
