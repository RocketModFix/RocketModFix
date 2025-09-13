using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Rocket.AutoInstaller;
using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine.Networking;

namespace Rocket.AutoInstaller.Installation
{
    public class Installer
    {
        public static IEnumerator Install(Config config)
        {
            if (config.EnableCustomInstall == true && !string.IsNullOrWhiteSpace(config.CustomInstallPath))
            {
                CommandWindow.Log("Installing from local path...");
                yield return LocalInstaller.InstallFromLocalPath(config.CustomInstallPath);
                yield break;
            }

            if (config.AutoInstallRocketFromExtras)
            {
                CommandWindow.Log("Installing from Extras...");
                yield return ExtrasInstaller.InstallRocketFromExtras();
                yield break;
            }

            yield return InstallFromGitHub(config);
        }

        public static IEnumerator InstallFromGitHub(Config config)
        {
            var modulesDirectory = Path.Combine(ReadWrite.PATH, "Modules");
            var releaseCache = new ReleaseCache(modulesDirectory);

            if (releaseCache.IsRocketInstalled())
            {
                var cachedEntry = releaseCache.GetCachedEntry();
                if (cachedEntry != null)
                {
                    CommandWindow.Log($"Already installed: {cachedEntry.TagName}");
                    yield break;
                }
            }

            yield return FetchAndInstallLatestRelease(releaseCache, config);
        }

        private static IEnumerator FetchAndInstallLatestRelease(ReleaseCache releaseCache, Config config)
        {
            var request = UnityWebRequest.Get("https://api.github.com/repos/RocketModFix/RocketModFix/releases");
            request.SetRequestHeader("User-Agent", "RocketModFix");
            request.redirectLimit = 5;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorMessage = $"Failed to fetch releases: {request.error} (Status: {request.responseCode})";
                CommandWindow.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var responseContent = request.downloadHandler.text;
            var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(responseContent);
            var latestRelease = releases!.FirstOrDefault();
            if (latestRelease == null)
            {
                CommandWindow.LogError("No release found");
                throw new Exception("No releases found");
            }
            if (string.IsNullOrWhiteSpace(latestRelease.TagName))
            {
                CommandWindow.LogError("Invalid release tag");
                throw new Exception("Invalid release tag name");
            }
            var moduleAsset = latestRelease.Assets.FirstOrDefault(IsRocketModFixModule);
            if (moduleAsset == null)
            {
                CommandWindow.LogError("Module not found");
                throw new Exception("Module asset not found");
            }

            if (!releaseCache.IsNewerReleaseAvailable(latestRelease.TagName, latestRelease.PublishedAt))
            {
                CommandWindow.Log("No newer release available");
                yield break;
            }

            CommandWindow.LogWarning($"Installing RocketModFix {latestRelease.TagName} ({latestRelease.PublishedAt:yyyy-MM-dd})");

            byte[]? rawData = null;
            if (config.EnableRetry)
            {
                yield return RetryHelper.DownloadWithRetry(
                    moduleAsset.BrowserDownloadUrl,
                    data => rawData = data,
                    error => throw new Exception(error),
                    maxRetries: 5,
                    delaySeconds: 5.0f);
            }
            else
            {
                yield return RetryHelper.DownloadSingleAttempt(
                    moduleAsset.BrowserDownloadUrl,
                    data => rawData = data,
                    error => throw new Exception(error));
            }

            if (rawData == null)
            {
                throw new Exception("Failed to download module data");
            }

            var releaseEntries = GetReleaseEntries(rawData);
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
                CommandWindow.LogError($"{rocketEntryPointFileName} not found");
                yield break;
            }

            foreach (var rocketLibrary in rocketLibraries)
            {
                Assembly.Load(rocketLibrary);
            }

            var rocketModule = Assembly.Load(rocketModuleData);

            var types = GetLoadableTypes(rocketModule);
            var moduleType = types.FirstOrDefault(
                x => x.IsAbstract == false && typeof(IModuleNexus).IsAssignableFrom(x));
            if (moduleType == null)
            {
                CommandWindow.LogError("Rocket module type not found");
                yield break;
            }

            try
            {
                if (Activator.CreateInstance(moduleType) is not IModuleNexus plugin)
                {
                    CommandWindow.LogError("Failed to create module");
                    yield break;
                }

                plugin.initialize();
            }
            catch (Exception ex)
            {
                CommandWindow.LogError($"Error creating module: {ex}");
                throw ex;
            }

            var cacheEntry = new CacheEntry
            {
                TagName = latestRelease.TagName,
                Name = latestRelease.Name,
                PublishedAt = latestRelease.PublishedAt,
                DownloadUrl = moduleAsset.BrowserDownloadUrl,
                FileSize = moduleAsset.Size
            };
            releaseCache.SaveCacheEntry(cacheEntry);

            CommandWindow.LogWarning($"Installed RocketModFix v{latestRelease.TagName}");
        }

        private static bool IsRocketModFixModule(GitHubAsset asset)
        {
            return asset.Name.Equals("Rocket.Unturned.Module.zip", StringComparison.Ordinal);
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