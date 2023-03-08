﻿using System;
using System.Collections.Generic;
using CreationEditor.Services.Mutagen.References;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
namespace CreationEditor.Skyrim.Avalonia.Models.Record;

public class ReferencedPlacedRecord : ReferencedRecord<IPlacedGetter> {
    public IMajorRecordIdentifier? Base { get; }

    public ReferencedPlacedRecord(IPlacedGetter record, ILinkCache linkCache, IEnumerable<IFormLinkIdentifier>? references = null)
        : base(record, references) {

        Base = record switch {
            IPlacedObjectGetter placedObject => placedObject.Base.TryResolve(linkCache),
            IPlacedNpcGetter placedNpc => placedNpc.Base.TryResolve(linkCache),
            IPlacedArrowGetter placedArrow => placedArrow.Projectile.TryResolve(linkCache),
            IPlacedBarrierGetter placedBarrier => placedBarrier.Projectile.TryResolve(linkCache),
            IPlacedBeamGetter placedBeam => placedBeam.Projectile.TryResolve(linkCache),
            IPlacedConeGetter placedCone => placedCone.Projectile.TryResolve(linkCache),
            IPlacedFlameGetter placedFlame => placedFlame.Projectile.TryResolve(linkCache),
            IPlacedHazardGetter placedHazard => placedHazard.Hazard.TryResolve(linkCache),
            IPlacedMissileGetter placedMissile => placedMissile.Projectile.TryResolve(linkCache),
            IPlacedTrapGetter placedTrap => placedTrap.Projectile.TryResolve(linkCache),
            _ => throw new ArgumentOutOfRangeException(nameof(record))
        };
    }
}
