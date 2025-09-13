using System;
using System.IO;
using SDG.Unturned;

namespace Rocket.AutoInstaller.Installation
{
    public static class PathValidator
    {
        public static bool ValidateLocalBuildPath(string path, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(path))
            {
                errorMessage = "Path is null or empty.";
                return false;
            }

            if (GenericUriDownloader.IsUri(path))
            {
                return ValidateUri(path, out errorMessage);
            }

            if (File.Exists(path) && path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return ValidateZipFile(path, out errorMessage);
            }

            if (Directory.Exists(path))
            {
                return ValidateDirectory(path, out errorMessage);
            }

            errorMessage = $"Path does not exist (neither file nor directory): {path}";
            return false;
        }

        private static bool ValidateUri(string uri, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
            {
                errorMessage = "Invalid URI format.";
                return false;
            }

            if (parsedUri.Scheme != "http" && parsedUri.Scheme != "https")
            {
                errorMessage = "Only HTTP and HTTPS URIs are supported.";
                return false;
            }

            // Note: We can't validate the actual content without making a network request
            // This validation will be done during the download process
            return true;
        }

        private static bool ValidateZipFile(string zipPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var fileInfo = new FileInfo(zipPath);
                if (fileInfo.Length == 0)
                {
                    errorMessage = "Zip file is empty.";
                    return false;
                }

                using var zipArchive = System.IO.Compression.ZipFile.OpenRead(zipPath);
                var hasRocketDll = false;
                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.Name.Equals("Rocket.Unturned.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        hasRocketDll = true;
                        break;
                    }
                }

                if (!hasRocketDll)
                {
                    errorMessage = "Rocket.Unturned.dll not found in the zip file.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Invalid or corrupted zip file: {ex}";
                return false;
            }

            return true;
        }

        private static bool ValidateDirectory(string directoryPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            var rocketDllPath = Path.Combine(directoryPath, "Rocket.Unturned.dll");
            if (File.Exists(rocketDllPath))
            {
                return true;
            }

            var moduleZipPath = Path.Combine(directoryPath, "Rocket.Unturned.Module.zip");
            if (File.Exists(moduleZipPath))
            {
                return ValidateZipFile(moduleZipPath, out errorMessage);
            }

            var subdirectories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);
            foreach (var subdir in subdirectories)
            {
                var subdirRocketDll = Path.Combine(subdir, "Rocket.Unturned.dll");
                if (File.Exists(subdirRocketDll))
                {
                    return true;
                }
            }

            errorMessage = $"Neither Rocket.Unturned.dll nor Rocket.Unturned.Module.zip found in: {directoryPath}";
            return false;
        }

        public static void LogValidationResult(string path, bool isValid, string errorMessage = "")
        {
            if (isValid)
            {
                CommandWindow.Log($"Path validated: {path}");
            }
            else
            {
                CommandWindow.LogError($"Path validation failed: {errorMessage}");
            }
        }
    }
}
