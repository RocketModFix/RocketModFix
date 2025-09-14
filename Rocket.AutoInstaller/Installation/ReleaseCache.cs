using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SDG.Unturned;

namespace Rocket.AutoInstaller.Installation
{
    public class ReleaseCache
    {
        private const string CacheFileName = "rocket_cache.json";
        private const string CacheDirectoryName = "cache";
        private readonly string _cacheFilePath;
        private readonly string _modulesDirectory;
        private readonly string _cacheDirectory;

        public ReleaseCache(string modulesDirectory)
        {
            _modulesDirectory = modulesDirectory;

            var autoInstallerDirectory = FindAutoInstallerDirectory(modulesDirectory);
            if (autoInstallerDirectory == null)
            {
                throw new Exception("Rocket.AutoInstaller module directory not found");
            }

            _cacheFilePath = Path.Combine(autoInstallerDirectory, CacheFileName);
            _cacheDirectory = Path.Combine(autoInstallerDirectory, CacheDirectoryName);

            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        private static string? FindAutoInstallerDirectory(string modulesDirectory)
        {
            try
            {
                var autoInstallerFiles = Directory.GetFiles(modulesDirectory, "Rocket.AutoInstaller.dll", SearchOption.AllDirectories);
                if (autoInstallerFiles.Length > 0)
                {
                    return Path.GetDirectoryName(autoInstallerFiles[0]);
                }
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Failed to find Rocket.AutoInstaller directory: {ex}");
            }
            return null;
        }

        public CacheEntry? GetCachedEntry()
        {
            if (!File.Exists(_cacheFilePath))
                return null;

            try
            {
                var cacheContent = File.ReadAllText(_cacheFilePath);
                var cache = JsonConvert.DeserializeObject<CacheEntry>(cacheContent);
                return cache;
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Cache read failed: {ex}");
                return null;
            }
        }

        public void SaveCacheEntry(CacheEntry entry)
        {
            try
            {
                var cacheContent = JsonConvert.SerializeObject(entry, Formatting.Indented);
                File.WriteAllText(_cacheFilePath, cacheContent);
                CommandWindow.Log($"Cached: {entry.TagName} ({entry.PublishedAt:yyyy-MM-dd})");
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Cache save failed: {ex}");
            }
        }

        public bool IsNewerReleaseAvailable(string latestTagName, DateTime latestPublishedAt)
        {
            var cachedEntry = GetCachedEntry();
            if (cachedEntry == null)
            {
                CommandWindow.Log("No cached release found");
                return true;
            }
            if (latestPublishedAt > cachedEntry.PublishedAt)
            {
                CommandWindow.Log($"Newer release: {latestTagName} vs {cachedEntry.TagName}");
                return true;
            }

            if (latestPublishedAt == cachedEntry.PublishedAt && latestTagName != cachedEntry.TagName)
            {
                CommandWindow.Log($"Different tag: {latestTagName} vs {cachedEntry.TagName}");
                return true;
            }

            CommandWindow.Log($"No newer release: {cachedEntry.TagName}");
            return false;
        }

        public void ClearCache()
        {
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    File.Delete(_cacheFilePath);
                }
                ClearCachedFiles();
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Cache clear failed: {ex}");
            }
        }

        public bool IsRocketInstalled()
        {
            var rocketFiles = Directory.GetFiles(_modulesDirectory, "Rocket.Unturned.dll", SearchOption.AllDirectories);
            return rocketFiles.Any();
        }

        public string GetCachedFilePath(string tagName)
        {
            var fileName = $"Rocket.Unturned.Module.{tagName}.zip";
            return Path.Combine(_cacheDirectory, fileName);
        }

        public bool IsFileCached(string tagName)
        {
            var cachedFilePath = GetCachedFilePath(tagName);
            return File.Exists(cachedFilePath);
        }

        public void SaveCachedFile(string tagName, byte[] fileData)
        {
            try
            {
                var cachedFilePath = GetCachedFilePath(tagName);
                File.WriteAllBytes(cachedFilePath, fileData);
                CommandWindow.Log($"Cached file: {Path.GetFileName(cachedFilePath)}");
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Failed to cache file: {ex}");
            }
        }

        public byte[]? LoadCachedFile(string tagName)
        {
            try
            {
                var cachedFilePath = GetCachedFilePath(tagName);
                if (File.Exists(cachedFilePath))
                {
                    var fileData = File.ReadAllBytes(cachedFilePath);
                    CommandWindow.Log($"Loaded from cache: {Path.GetFileName(cachedFilePath)}");
                    return fileData;
                }
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Failed to load cached file: {ex}");
            }
            return null;
        }

        public void ClearCachedFiles()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    var files = Directory.GetFiles(_cacheDirectory, "*.zip");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    CommandWindow.Log($"Cleared {files.Length} cached files");
                }
            }
            catch (Exception ex)
            {
                CommandWindow.LogWarning($"Failed to clear cached files: {ex}");
            }
        }
    }

    public class CacheEntry
    {
        public string TagName { get; set; }
        public string Name { get; set; }
        public DateTime PublishedAt { get; set; }
        public string DownloadUrl { get; set; }
        public int FileSize { get; set; }
        public DateTime CachedAt { get; set; }

        public CacheEntry()
        {
            CachedAt = DateTime.UtcNow;
        }
    }
}
