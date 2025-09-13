using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SDG.Unturned;

namespace Rocket.AutoInstaller.Installation
{
    public class ReleaseCache
    {
        private const string CacheFileName = "rocket_cache.json";
        private readonly string _cacheFilePath;
        private readonly string _modulesDirectory;

        public ReleaseCache(string modulesDirectory)
        {
            _modulesDirectory = modulesDirectory;
            _cacheFilePath = Path.Combine(modulesDirectory, CacheFileName);
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
                    CommandWindow.Log("Cache cleared");
                }
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
    }

    public class CacheEntry
    {
        public string TagName { get; set; }
        public string Name { get; set; }
        public DateTime PublishedAt { get; set; }
        public string DownloadUrl { get; set; }
        public long FileSize { get; set; }
        public DateTime CachedAt { get; set; }

        public CacheEntry()
        {
            CachedAt = DateTime.UtcNow;
        }
    }
}
