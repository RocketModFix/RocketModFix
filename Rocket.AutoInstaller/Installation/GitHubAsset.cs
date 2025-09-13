using System;
using Newtonsoft.Json;

namespace Rocket.AutoInstaller.Installation
{
    public class GitHubAsset
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    public class GitHubArtifactsResponse
    {
        [Newtonsoft.Json.JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [Newtonsoft.Json.JsonProperty("artifacts")]
        public GitHubArtifact[] Artifacts { get; set; }
    }

    public class GitHubArtifact
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public long Id { get; set; }

        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("size_in_bytes")]
        public long SizeInBytes { get; set; }

        [Newtonsoft.Json.JsonProperty("url")]
        public string Url { get; set; }

        [Newtonsoft.Json.JsonProperty("archive_download_url")]
        public string ArchiveDownloadUrl { get; set; }

        [Newtonsoft.Json.JsonProperty("expired")]
        public bool Expired { get; set; }

        [Newtonsoft.Json.JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [Newtonsoft.Json.JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

        public class GitHubRelease
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonProperty("assets")]
        public GitHubAsset[] Assets { get; set; }
    }
}