using System;
using System.Collections;
using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace Rocket.AutoInstaller.Installation
{
    public static class RetryHelper
    {
        public static IEnumerator DownloadWithRetry(
            string url,
            Action<byte[]>? onSuccess,
            Action<string>? onError,
            int maxRetries = 5,
            float delaySeconds = 5.0f)
        {
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

        public static IEnumerator DownloadSingleAttempt(
            string url,
            Action<byte[]>? onSuccess,
            Action<string>? onError)
        {
            var request = UnityEngine.Networking.UnityWebRequest.Get(url);
            request.SetRequestHeader("User-Agent", "RocketModFix");
            request.redirectLimit = 5;

            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
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
