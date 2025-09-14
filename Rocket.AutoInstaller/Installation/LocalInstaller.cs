using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using SDG.Framework.Modules;
using SDG.Unturned;

namespace Rocket.AutoInstaller.Installation
{
    public class LocalInstaller
    {
        public static IEnumerator InstallFromLocalPath(string localPath)
        {
            if (!PathValidator.ValidateLocalBuildPath(localPath, out var errorMessage))
            {
                CommandWindow.LogError($"Local install failed: {errorMessage}");
                PathValidator.LogValidationResult(localPath, false, errorMessage);
                yield break;
            }

            PathValidator.LogValidationResult(localPath, true);
            CommandWindow.Log($"Installing from: {localPath}");

            List<ReleaseEntry> releaseEntries = [];

            if (GenericUriDownloader.IsUri(localPath))
            {
                yield return DownloadAndProcessGenericUri(localPath, entries => releaseEntries = entries);
            }
            else
            {
                try
                {
                    if (File.Exists(localPath) && localPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        var zipData = File.ReadAllBytes(localPath);
                        releaseEntries = GetReleaseEntries(zipData);
                    }
                    else
                    {
                        releaseEntries = GetReleaseEntriesFromDirectory(localPath);
                    }
                }
                catch (Exception ex)
                {
                    CommandWindow.LogError($"Failed to read module zip: {ex}");
                    yield break;
                }
            }

            byte[]? rocketModuleData = null;
            List<byte[]> rocketLibraries = [];
            const string rocketEntryPointFileName = "Rocket.Unturned.dll";

            foreach (var releaseEntry in releaseEntries)
            {
                if (releaseEntry.Name == rocketEntryPointFileName)
                {
                    rocketModuleData = releaseEntry.Content;
                    continue;
                }
                if (releaseEntry.Name != rocketEntryPointFileName && releaseEntry.FileExtension == ".dll")
                {
                    rocketLibraries.Add(releaseEntry.Content);
                    continue;
                }
            }

            if (rocketModuleData == null)
            {
                CommandWindow.LogError($"{rocketEntryPointFileName} not found in zip");
                yield break;
            }

            foreach (var rocketLibrary in rocketLibraries)
            {
                try
                {
                    Assembly.Load(rocketLibrary);
                }
                catch (Exception ex)
                {
                    CommandWindow.LogWarning($"Failed to load library: {ex}");
                }
            }

            var rocketModule = Assembly.Load(rocketModuleData);
            var types = GetLoadableTypes(rocketModule);
            var moduleType = types.FirstOrDefault(x => !x.IsAbstract && typeof(IModuleNexus).IsAssignableFrom(x));

            if (moduleType == null)
            {
                CommandWindow.LogError("Rocket module type not found");
                yield break;
            }

            IModuleNexus plugin;
            try
            {
                if (Activator.CreateInstance(moduleType) is not IModuleNexus createdPlugin)
                {
                    CommandWindow.LogError("Failed to create rocket module");
                    yield break;
                }
                plugin = createdPlugin;
            }
            catch (Exception ex)
            {
                CommandWindow.LogError($"Error creating rocket module: {ex}");
                yield break;
            }

            plugin.initialize();
            CommandWindow.LogWarning($"Installed from local build: {localPath}");
        }

        private static List<ReleaseEntry> GetReleaseEntries(byte[] assetData)
        {
            var entries = new List<ReleaseEntry>();
            using var memoryStream = new MemoryStream(assetData);
            using var zipArchive = new ZipArchive(memoryStream);
            foreach (var entry in zipArchive.Entries)
            {
                try
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(entry.FullName);
                    var fileExtension = Path.GetExtension(entry.FullName);
                    var entryDirectoryName = Path.GetDirectoryName(entry.FullName)!;

                    using var stream = entry.Open();
                    var entryData = stream.CopyToArray();

                    entries.Add(new ReleaseEntry(entry.Name, entry.FullName, fileNameWithoutExtension, fileExtension,
                        entryDirectoryName, entryData));
                }
                catch (Exception ex)
                {
                    CommandWindow.LogError($"Error reading {entry.FullName}: {ex}");
                }
            }
            return entries;
        }

        private static List<ReleaseEntry> GetReleaseEntriesFromDirectory(string directoryPath)
        {
            var entries = new List<ReleaseEntry>();

            var allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in allFiles)
            {
                try
                {
                    var relativePath = GetRelativePath(directoryPath, filePath);
                    var fileName = Path.GetFileName(filePath);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    var fileExtension = Path.GetExtension(filePath);
                    var entryDirectoryName = Path.GetDirectoryName(relativePath) ?? string.Empty;

                    var fileData = File.ReadAllBytes(filePath);

                    entries.Add(new ReleaseEntry(fileName, relativePath, fileNameWithoutExtension, fileExtension,
                        entryDirectoryName, fileData));
                }
                catch (Exception ex)
                {
                    CommandWindow.LogError($"Error reading {filePath}: {ex}");
                }
            }

            return entries;
        }

        private static IEnumerator DownloadAndProcessGenericUri(string uri, Action<List<ReleaseEntry>> onComplete)
        {
            var entries = new List<ReleaseEntry>();
            var downloadSucceeded = false;
            var downloadError = string.Empty;

            yield return GenericUriDownloader.DownloadFromUri(uri,
                data => {
                    entries = GetReleaseEntries(data);
                    downloadSucceeded = true;
                },
                error => {
                    downloadError = error;
                    downloadSucceeded = false;
                });

            if (!downloadSucceeded)
            {
                CommandWindow.LogError($"URI download failed: {downloadError}");
                yield break;
            }

            onComplete?.Invoke(entries);
        }

        private static string GetRelativePath(string basePath, string targetPath)
        {
            var baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basePath : basePath + Path.DirectorySeparatorChar);
            var targetUri = new Uri(targetPath);
            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Safely returns the set of loadable types from an assembly.
        /// Algorithm from StackOverflow answer here:
        /// https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> from which to load types.</param>
        /// <returns>
        /// The set of types from the <paramref name="assembly" />, or the subset
        /// of types that could be loaded if there was any error.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assembly" /> is <see langword="null" />.
        /// </exception>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.DefinedTypes.Select(x => x.AsType());
            }
            catch (ReflectionTypeLoadException ex)
            {
                CommandWindow.LogError($"Error getting types from {assembly}: {ex}");
                return ex.Types.Where(x => x is not null);
            }
        }
    }
}
