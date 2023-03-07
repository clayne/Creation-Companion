﻿using System.IO.Abstractions;
using CreationEditor.Avalonia.Services.Record.Browser.Filter;
using CreationEditor.Services.Environment;
using Mutagen.Bethesda.Skyrim;
namespace CreationEditor.Skyrim.Avalonia.Services.Record.Browser.Filter;

public class SoundDescriptorFilter : AssetFilter<ISoundDescriptorGetter> {
    public SoundDescriptorFilter(
        IEditorEnvironment editorEnvironment,
        IFileSystem fileSystem)
        : base(editorEnvironment, fileSystem) {}
}