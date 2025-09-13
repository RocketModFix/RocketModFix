# Rocket.AutoInstaller

Auto-installer for RocketModFix. Downloads and installs the latest version from GitHub, or installs from local builds for development.

## Quick Start

Just drop the module in your `Modules` folder and start your server. It will automatically download and install the latest RocketModFix.

## Configuration

Edit `config.json` to customize installation behavior:

```json
{
	"EnableCustomInstall": false,
	"CustomInstallPath": "",
	"BlockIfRocketInstalled": true,
	"AutoInstallRocketFromExtras": false,
	"EnableRetry": true
}
```

### Options

**For regular users:**
- `BlockIfRocketInstalled` - Prevent installation if Rocket is already installed
- `EnableRetry` - Enable retry mechanism for failed downloads (5 attempts, 5s delay)

**For developers:**
- `EnableCustomInstall` - Enable local installation instead of GitHub
- `CustomInstallPath` - Path to your local build (see examples below)
- `AutoInstallRocketFromExtras` - Auto-install from Extras folder

### Local Installation Examples

For developers who want to test their own builds without manually copying files every time. The `CustomInstallPath` can point to:

1. A zip file - Direct path to Rocket.Unturned.Module.zip
   ```
   "CustomInstallPath": "C:\\Builds\\Rocket.Unturned.Module.zip"
   ```

2. An unzipped folder - Directory containing the module files
   ```
   "CustomInstallPath": "C:\\Builds\\RocketModFix\\Rocket.Unturned"
   ```
   
   The folder should contain either:
   - Rocket.Unturned.dll directly, or
   - Rocket.Unturned.Module.zip file, or
   - Rocket.Unturned.dll in any subdirectory

3. GitHub Actions URL - Direct link to a GitHub Actions run
   ```
   "CustomInstallPath": "https://github.com/RocketModFix/RocketModFix/actions/runs/17277595277"
   ```

4. Generic URI - Any HTTP/HTTPS URL pointing to a zip file
   ```
   "CustomInstallPath": "https://example.com/downloads/Rocket.Unturned.Module.zip"
   ```

## Features

- Auto-install from GitHub - Downloads latest release automatically
- Local build support - Install from zip files, directories, or URLs
- Smart caching - Only downloads when newer versions are available
- Retry mechanism - Handles network issues with automatic retries
- Extras integration - Auto-install from server's Extras folder
- Safety checks - Prevents conflicts with existing installations

## Plans

If you want to implement one of the plans or have better ideas, feel free to let us know!

- [x] Auto-Install from GitHub Releases
- [x] Auto-Install Local Build (no need to manually install RocketModFix every time you test/update it, 1 click to build, restart server, and you're testing!)
	- [x] Json config with options:
  		- [x] `EnableCustomInstall`: false/true (default is false)
    	- [x] `CustomInstallPath`: path to a build of RocketModFix
    - [x] `AutoInstallRocketFromExtras`: false/true (default is false) - Auto-install Rocket (LDM) from Extras
    - [x] Fixed self-directory exclusion in Module code to prevent blocking the load process (see `BlockIfRocketInstalled`)!
- [x] Caching
	- [x] Check if GitHub release is newer than current cached version and ONLY then install new version, also use Retry (5 seconds, 5 attempts or so) in case of GitHub's down or problems with internet
	- [x] For safe usage without Internet Connection
- [x] ([Details](https://github.com/RocketModFix/RocketModFix/issues/119)) Block installation if Rocket already installed (`BlockIfRocketInstalled` in config)