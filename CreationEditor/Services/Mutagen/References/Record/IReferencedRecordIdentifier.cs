﻿using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
namespace CreationEditor.Services.Mutagen.References.Record;

public interface IReferencedRecordIdentifier : IReferenced, IFormLinkIdentifier {
    public IMajorRecordIdentifierGetter Record { get; set; }
}
