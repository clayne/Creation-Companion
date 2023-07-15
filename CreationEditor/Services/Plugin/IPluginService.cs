﻿namespace CreationEditor.Services.Plugin;

public interface IPluginService {
    /// <summary>
    /// Plugins that are currently loaded.
    /// </summary>
    IReadOnlyList<IPluginDefinition> Plugins { get; }
}