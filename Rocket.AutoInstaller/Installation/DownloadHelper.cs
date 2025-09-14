using System;
using System.Collections;
using SDG.Unturned;
using UnityEngine;
using UnityEngine.Networking;

namespace Rocket.AutoInstaller.Installation
{
    public static class DownloadHelper
    {
        public static bool IsUri(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return Uri.TryCreate(path, UriKind.Absolute, out _);
        }

        public static IEnumerator Download(
            string url,
            Action<byte[]>? onSuccess,
            Action<string>? onError,
            Config config,
            float delaySeconds = 5.0f)
        {
            if (!IsUri(url))
            {
                onError?.Invoke("Invalid URI format.");
                yield break;
            }

            CommandWindow.Log($"Downloading: {url}");

            var maxRetries = config.EnableRetry ? 5 : 1;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (attempt > 1)
                {
                    yield return new WaitForSeconds(delaySeconds);
                }

                bool downloadSucceeded = false;
                yield return DownloadSingleAttempt(url,
                    data => {
                        onSuccess?.Invoke(data);
                        downloadSucceeded = true;
                    },
                    error => {
                        onError?.Invoke(error);
                        downloadSucceeded = false;
                    });

                if (downloadSucceeded)
                {
                    yield break;
                }
            }

            CommandWindow.LogError($"Download failed after {maxRetries} attempts: {url}");
        }

        private static IEnumerator DownloadSingleAttempt(
            string url,
            Action<byte[]>? onSuccess,
            Action<string>? onError)
        {
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("User-Agent", "RocketModFix");
            request.redirectLimit = 5;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorMessage = $"Download failed: {request.error} (HTTP {request.responseCode})";
                onError?.Invoke(errorMessage);
                throw new Exception(errorMessage);
            }

            var data = request.downloadHandler.data;
            onSuccess?.Invoke(data);
        }
    }
}
