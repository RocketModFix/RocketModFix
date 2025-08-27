param(
    [string]$RepoFile = "api-suppressions.xml",
    [string]$GeneratedFile = "generated-api-suppressions.xml"
)

Write-Host "Comparing suppression files..."

# Check files exist
if (!(Test-Path $RepoFile)) {
    Write-Host "❌ Repository file not found: $RepoFile" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $GeneratedFile)) {
    Write-Host "❌ Generated file not found: $GeneratedFile" -ForegroundColor Red
    exit 1
}

# Simple file comparison
$repoContent = Get-Content $RepoFile -Raw
$generatedContent = Get-Content $GeneratedFile -Raw

if ($repoContent.Trim() -ne $generatedContent.Trim()) {
    Write-Host "❌ Files are different!" -ForegroundColor Red
    Write-Host "Please either fix breaking change or update $RepoFile with the content from $GeneratedFile if this is not important breaking change" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✅ Files match!" -ForegroundColor Green
    exit 0
}