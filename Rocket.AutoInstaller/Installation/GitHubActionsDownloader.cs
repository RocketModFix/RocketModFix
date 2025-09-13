using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SDG.Unturned;
using UnityEngine.Networking;

namespace Rocket.AutoInstaller.Installation
{
    public static class GitHubActionsDownloader
    {
        private const string GitHubApiBase = "https://api.github.com";
        private const string GitHubActionsUrlPattern = @"https://github\.com/([^/]+)/([^/]+)/actions/runs/(\d+)";

        public static bool IsGitHubActionsUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Regex.IsMatch(url, GitHubActionsUrlPattern);
        }

        public static IEnumerator DownloadArtifactFromRun(string runUrl, Action<byte[]>? onSuccess, Action<string> onError)
        {
            if (!IsGitHubActionsUrl(runUrl))
            {
                onError?.Invoke("Invalid GitHub Actions run URL format.");
                yield break;
            }

            var match = Regex.Match(runUrl, GitHubActionsUrlPattern);
            if (!match.Success)
            {
                onError?.Invoke("Could not parse GitHub Actions run URL.");
                yield break;
            }

            var owner = match.Groups[1].Value;
            var repo = match.Groups[2].Value;
            var runId = match.Groups[3].Value;

            var artifactsUrl = $"{GitHubApiBase}/repos/{owner}/{repo}/actions/runs/{runId}/artifacts";
            var request = UnityWebRequest.Get(artifactsUrl);
            request.SetRequestHeader("User-Agent", "RocketModFix");
            request.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to fetch artifacts: {request.error} (Status: {request.responseCode})");
                yield break;
            }

            var artifactsResponse = request.downloadHandler.text;
            var artifacts = JsonConvert.DeserializeObject<GitHubArtifactsResponse>(artifactsResponse);

            if (artifacts?.Artifacts == null || artifacts.Artifacts.Length == 0)
            {
                onError?.Invoke("No artifacts found for this run.");
                yield break;
            }

            var moduleArtifact = artifacts.Artifacts.FirstOrDefault(a => a.Name.Equals("Rocket.Unturned.Module.zip", StringComparison.OrdinalIgnoreCase));

            if (moduleArtifact == null)
            {
                moduleArtifact = artifacts.Artifacts.FirstOrDefault(a =>a.Name.ToLowerInvariant().Contains("module"));
            }

            if (moduleArtifact == null)
            {
                onError?.Invoke($"No suitable module artifact found. Available artifacts: {string.Join(", ", artifacts.Artifacts.Select(a => a.Name))}");
                yield break;
            }

            var downloadUrl = $"{GitHubApiBase}/repos/{owner}/{repo}/actions/artifacts/{moduleArtifact.Id}/zip";
            var downloadRequest = UnityWebRequest.Get(downloadUrl);
            downloadRequest.SetRequestHeader("User-Agent", "RocketModFix");
            downloadRequest.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            yield return downloadRequest.SendWebRequest();

            if (downloadRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to download artifact: {downloadRequest.error} (Status: {downloadRequest.responseCode})");
                yield break;
            }

            var artifactData = downloadRequest.downloadHandler.data;

            onSuccess?.Invoke(artifactData);
        }
    }
}
