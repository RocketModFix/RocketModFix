using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SDG.Unturned;
using UnityEngine.Networking;

namespace Rocket.AutoInstaller.Installation
{
    public static class ExtrasInstaller
    {
        public static IEnumerator InstallRocketFromExtras()
        {
            CommandWindow.Log("Installing from Extras...");

            var extrasPath = Path.Combine(ReadWrite.PATH, "Extras");
            if (!Directory.Exists(extrasPath))
            {
                CommandWindow.LogError("Extras directory not found");
                yield break;
            }

            var moduleZipPath = Path.Combine(extrasPath, "Rocket.Unturned.Module.zip");
            if (!File.Exists(moduleZipPath))
            {
                CommandWindow.LogError("Rocket.Unturned.Module.zip not found in Extras");
                yield break;
            }

            CommandWindow.Log($"Found module: {moduleZipPath}");

            var modulesPath = Path.Combine(ReadWrite.PATH, "Modules");
            var targetPath = Path.Combine(modulesPath, "Rocket.Unturned.Module.zip");

            try
            {
                File.Copy(moduleZipPath, targetPath, true);
                CommandWindow.Log("Copied module to Modules directory");
            }
            catch (Exception ex)
            {
                CommandWindow.LogError($"Failed to copy module: {ex}");
                yield break;
            }

            ExtractModule(targetPath, modulesPath);
        }

        private static void ExtractModule(string zipPath, string extractPath)
        {
            try
            {
                using var archive = ZipFile.OpenRead(zipPath);
                var extractDir = Path.Combine(extractPath, "Rocket.Unturned");

                if (Directory.Exists(extractDir))
                {
                    Directory.Delete(extractDir, true);
                }
                Directory.CreateDirectory(extractDir);
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    var entryPath = Path.Combine(extractDir, entry.FullName);
                    var entryDir = Path.GetDirectoryName(entryPath);

                    if (!string.IsNullOrEmpty(entryDir) && !Directory.Exists(entryDir))
                    {
                        Directory.CreateDirectory(entryDir);
                    }

                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        entry.ExtractToFile(entryPath, true);
                    }
                }

                File.Delete(zipPath);
            }
            catch (Exception ex)
            {
                CommandWindow.LogError($"Failed to extract: {ex}");
            }
        }
    }
}
