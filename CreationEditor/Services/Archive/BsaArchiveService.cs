﻿using System.IO.Abstractions;
using System.Reactive.Linq;
using CreationEditor.Resources.Comparer;
using CreationEditor.Services.Asset;
using CreationEditor.Services.Environment;
using Mutagen.Bethesda.Archives;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
namespace CreationEditor.Services.Archive;

public sealed class BsaArchiveService : IArchiveService {
    private sealed record ArchiveDirectory(string Name, IList<ArchiveDirectory> Subdirectories);

    private readonly IGameReleaseContext _gameReleaseContext;
    private readonly IFileSystem _fileSystem;
    private readonly IDisposableDropoff _disposables = new DisposableBucket();
    private readonly IFileSystemWatcher _watcher;

    private readonly Dictionary<string, IArchiveReader> _archives = new();
    private readonly Dictionary<string, List<ArchiveDirectory>> _archiveDirectories = new();

    private const string BsaFilter = "*.bsa";

    private readonly string _dataDirectory;

    private string? _archiveExtension;
    public string GetExtension() => _archiveExtension ?? LoadExtension();
    private string LoadExtension() => _archiveExtension = global::Mutagen.Bethesda.Archives.Archive.GetExtension(_gameReleaseContext.Release);

    public IReadOnlyList<string> Archives { get; }

    public BsaArchiveService(
        IEditorEnvironment editorEnvironment,
        IGameReleaseContext gameReleaseContext,
        IDataDirectoryProvider dataDirectoryProvider,
        IFileSystem fileSystem) {
        _gameReleaseContext = gameReleaseContext;
        _fileSystem = fileSystem;
        _dataDirectory = dataDirectoryProvider.Path;

        // Collect bsa files in the data directory and sort them based on the load order
        IEnumerable<string> GetModBSAFiles(IModListingGetter<IModGetter> modListing) {
            var extension = GetExtension();

            yield return modListing.ModKey.Name + extension;
            yield return modListing.ModKey.Name + " - Textures" + extension;
        }

        var bsaNameOrder = editorEnvironment.GameEnvironment.LoadOrder.ListedOrder
            .SelectMany(GetModBSAFiles)
            .ToArray();

        // todo potentially filter out bsa files that are not in the load order or referenced in the ini file
        Archives = fileSystem.Directory
            .EnumerateFiles(_dataDirectory, BsaFilter)
            .Order(new FuncComparer<string>((x, y) => {
                var indexOfX = bsaNameOrder.IndexOf(x);
                var indexOfY = bsaNameOrder.IndexOf(y);

                if (indexOfX == -1) {
                    if (indexOfY == -1) return 0;

                    return -1;
                }

                if (indexOfY == -1) return 1;

                // Files that are higher in the load order will have a higher index and be prioritized 
                return indexOfX.CompareTo(indexOfY);
            }))
            .ToArray();

        foreach (var bsa in Archives) Add(bsa);

        // Create file system watcher in the data directory to check for new BSA files
        _watcher = fileSystem.FileSystemWatcher.New(_dataDirectory, BsaFilter);
        _watcher.EnableRaisingEvents = true;
        _watcher.IncludeSubdirectories = false;

        Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _watcher.Changed += h,
                h => _watcher.Changed -= h)
            .Subscribe(e => {
                Remove(e.EventArgs.FullPath);
                Add(e.EventArgs.FullPath);
            })
            .DisposeWith(_disposables);

        Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _watcher.Created += h,
                h => _watcher.Created -= h)
            .Subscribe(e => Add(e.EventArgs.FullPath))
            .DisposeWith(_disposables);

        Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _watcher.Deleted += h,
                h => _watcher.Deleted -= h)
            .Subscribe(e => Remove(e.EventArgs.FullPath))
            .DisposeWith(_disposables);

        Observable
            .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                h => _watcher.Renamed += h,
                h => _watcher.Renamed -= h)
            .Subscribe(e => {
                Remove(e.EventArgs.OldFullPath);
                Add(e.EventArgs.FullPath);
            })
            .DisposeWith(_disposables);
    }

    public IArchiveReader GetReader(string path) {
        if (_archives.TryGetValue(path, out var reader)) return reader;

        return global::Mutagen.Bethesda.Archives.Archive.CreateReader(_gameReleaseContext.Release, path);
    }

    public string? TryGetFileAsTempFile(string filePath) {
        // Make the path relative to the data folder
        filePath = _fileSystem.Path.GetRelativePath(_dataDirectory, filePath);

        var directoryPath = _fileSystem.Path.GetDirectoryName(filePath);
        if (directoryPath == null) return null;

        // Search for file in archives
        foreach (var reader in _archives.Values) {
            if (!reader.TryGetFolder(directoryPath, out var archiveFolder)) continue;

            var archiveFile = archiveFolder.Files.FirstOrDefault(f => f.Path == filePath);
            if (archiveFile == null) continue;

            // Copy file to temp file
            var tempFilePath = _fileSystem.Path.GetTempFileName() + _fileSystem.Path.GetExtension(filePath);
            using var tempFileStream = _fileSystem.File.Create(tempFilePath);
            using var archiveFileStream = archiveFile.AsStream();
            archiveFileStream.CopyTo(tempFileStream);

            return tempFilePath;
        }

        return null;
    }

    public IEnumerable<string> GetFilesInFolder(string path) {
        var relativePath = _fileSystem.Path.GetRelativePath(_dataDirectory, path);

        foreach (var reader in _archives.Values) {
            if (!reader.TryGetFolder(relativePath, out var archiveFolder)) continue;

            foreach (var archiveFile in archiveFolder.Files) {
                yield return archiveFile.Path;
            }
        }
    }

    public IEnumerable<string> GetSubdirectories(string path) {
        var relativePath = _fileSystem.Path.GetRelativePath(_dataDirectory, path);
        var isRoot = string.Equals(path, _dataDirectory, AssetCompare.PathComparison);

        foreach (var (archivePath, reader) in _archives) {
            // Compile directories in archive
            if (!_archiveDirectories.TryGetValue(archivePath, out var directories)) {
                directories = new List<ArchiveDirectory>();

                foreach (var file in reader.Files) {
                    var dirName = _fileSystem.Path.GetDirectoryName(file.Path);
                    if (dirName == null) continue;

                    var directoryNames = dirName.Split(_fileSystem.Path.DirectorySeparatorChar);
                    if (directoryNames.Length == 0) continue;

                    IList<ArchiveDirectory> subDirectories = directories;
                    foreach (var dir in directoryNames) {
                        var currentDir = subDirectories.FirstOrDefault(d => d.Name == dir);
                        if (currentDir == null) {
                            currentDir = new ArchiveDirectory(dir, new List<ArchiveDirectory>());
                            subDirectories.Add(currentDir);
                        }
                        subDirectories = currentDir.Subdirectories;
                    }
                }

                _archiveDirectories.Add(archivePath, directories);
            }

            var invalid = false;
            IList<ArchiveDirectory> currentDirectories = directories;
            if (!isRoot) {
                var relativePathNames = relativePath.Split(_fileSystem.Path.DirectorySeparatorChar);
                foreach (var pathName in relativePathNames) {
                    var dir = currentDirectories.FirstOrDefault(d => string.Equals(d.Name, pathName, AssetCompare.PathComparison));
                    if (dir == null) {
                        invalid = true;
                        break;
                    }

                    currentDirectories = dir.Subdirectories;
                }
            }

            if (isRoot || !invalid) {
                foreach (var currentDirectory in currentDirectories) {
                    yield return _fileSystem.Path.Combine(path, currentDirectory.Name);
                }
            }
        }
    }

    private void Add(string fullPath) {
        if (_archives.ContainsKey(fullPath)) return;

        var reader = global::Mutagen.Bethesda.Archives.Archive.CreateReader(_gameReleaseContext.Release, fullPath);
        _archives.Add(fullPath, reader);
    }

    private void Remove(string fullPath) {
        _archives.Remove(fullPath);
    }

    public void Dispose() {
        _disposables.Dispose();
        _watcher.Dispose();
    }
}
