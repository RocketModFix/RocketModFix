using System;
using System.Collections;
using System.IO;
using SDG.Unturned;
using UnityEngine.Networking;

namespace Rocket.AutoInstaller.Installation
{
    public static class GenericUriDownloader
    {
        public static bool IsUri(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
                
            return Uri.TryCreate(path, UriKind.Absolute, out _);
        }
        
        public static IEnumerator DownloadFromUri(string uri, Action<byte[]> onSuccess, Action<string> onError)
        {
            if (!IsUri(uri))
            {
                onError?.Invoke("Invalid URI format.");
                yield break;
            }
            
            CommandWindow.Log($"Downloading: {uri}");
            
            var request = UnityWebRequest.Get(uri);
            request.SetRequestHeader("User-Agent", "RocketModFix");
            request.redirectLimit = 5;
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to download from URI: {request.error} (Status: {request.responseCode})");
                yield break;
            }
            
            var data = request.downloadHandler.data;
            
            onSuccess?.Invoke(data);
        }
    }
}
