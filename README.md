# ![RocketModFix][rocketmodfix_logo]

## RocketModFix

The **RocketModFix** is a fork of [LDM][ldm_github_repository] for Unturned maintained by the Unturned plugin devs, this fork don't have plans for any major changes to the RocketMod, only fixes and new features that doesn't break any backward compatibility, so you don't need to update your plugins.

## Compatibility

You can still use old plugins without any changes/recompilation/updates.
We're not planning to make any breaking changes with API.

## Our plan and what we're done

- [x] Create Discord Server Community.
- [x] UnityEngine NuGet Package redist.
- [x] Unturned NuGet Package redist.
- [x] Update MSBuild to the `Microsoft.NET.Sdk`, because current MSBuild in RocketMod is outdated and its hard to support and understand what's going on inside.
- [x] RocketMod NuGet Package containing all required libraries for RockeMod API usage.
- [x] CI/CD and nightly builds with RocketMod .dlls.
- [x] Breaking changes detector ([see #126](https://github.com/RocketModFix/RocketModFix/issues/126)).
- [x] Automatic Release on Tag creation (with RocketMod Module).
- [x] Rocket.Unturned.Module Artifacts on PR.
- [x] Rocket.Unturned NuGet Package.
- [x] Reset changelog.
- [x] For versioning use [SemVer][semver_url].
- [x] Installation guides inside the Rocket Unturned Module.
- [x] Rocket.AutoInstaller to automatically install Rocket.
- [x] Keep backward compatibility.
	- [x] Test with RocketMod plugins that uses old RocketMod libraries, and make sure current changes doesn't break anything.
	- [x] Test with most used Modules:
		- [x] AviRockets.
		- [x] uScript.
		- [x] OpenMod.
- [ ] RocketMod Fixes:
	- [x] Fix UnturnedPlayer.SteamProfile, current implementation cause so many lags (fixed, but still requires fixes).
	- [x] Fix UnturnedPlayerComponent is not being added and removed automatically.
	- [x] /admin /unadmin doesn't work when use offline player (now it possible to use steam id of the offline player).
	- [ ] Assembly Resolve fixes (don't spam with not found library or make a option to disable it, load all libraries at rocketmod start instead of searching for them only on OnAssemblyResolve)
	- [x] Fix problem when TaskDispatcher is not calling an action (example: when some plugins queue a player which connected to the server the action might not be called = bypassed checks/bans etc).
	- [ ] Commands fixes:
		- [ ] Fix /vanish.
		- [x] Fix /god. (oxygen isn't fixed)
		- [ ] Fix /p (not readable at all).
	- [x] Make permission group operations case-insensitive ([see #107](https://github.com/RocketModFix/RocketModFix/issues/107))
	- [ ] Performance.
- [x] New Features:
	- [x] JSON file support (before it was only XML).
	- [x] Commands:
		- [x] /position /pos (current position of the player).
		- [x] /tpwp (improved version of /tp wp).
		- [x] /savelogs (a fast way for sending logs to plugin developer or whatever).
- [ ] Remove Features:
	- [x] Command /compass
- [x] Gather a Team with a direct access to the repo edit without admins help. (We still gather a team)
- [ ] RocketModFix Video Installation Guide (could be uploaded on YouTube).

After plan is finished -> Add new plans, keep coding, and don't forget to approve PR or issues.

## Installation

Now we have 2 different ways you can install Rocket, either `Standard Way` or `Auto-Installer`, select the one you like more, but we highly recommend to use `Auto-Installer`.

### Standard Way

1. **Stop the Server**: If your server is running, stop it.
2. **Remove Old Rocket**: Delete the entire `Rocket.Unturned` folder located in `Modules` (if it exists).
3. **Download the Latest RocketModFix**: Go to the [RocketModFix releases page](https://github.com/RocketModFix/RocketModFix/releases).
4. **Access the Assets**: Open the "Assets" section if it's not already expanded.
5. **Download the Module**: Click `Rocket.Unturned.Module.zip` to download the latest module.
6. **Final**: Extract the downloaded archive, open the `Rocket.Unturned.Module` folder, and copy the `Rocket.Unturned` folder to `Modules` (copy the folder, not it's content, and if its asks to Replace the existing files then press to replace them).

### Auto-Installer (new way)

It's same as installing Rocket manually (standard way), however if we make an update you will receive it automatically after you restart the server, so you don't need to remove old Rocket, replace/delete files, etc.

See more info [here](https://github.com/RocketModFix/RocketModFix/blob/master/Rocket.AutoInstaller/README.md) about it if you're interested how it work and what we're planning to do next with it.

1. **Stop the Server**: If your server is running, stop it.
2. **Remove Rocket** (if you still have it): Delete the entire `Rocket.Unturned` folder located in `Modules` (if it exists).
3. **Download the Latest Rocket.AutoInstaller**: Go to the [RocketModFix releases page](https://github.com/RocketModFix/RocketModFix/releases).
4. **Access the Assets**: Open the "Assets" section if it's not already expanded.
5. **Download the Rocket.AutoInstaller**: Click `Rocket.AutoInstaller.zip` to download the latest module.
6. **Final**: Extract the downloaded archive, open the `Rocket.AutoInstaller` folder, and copy the `Rocket.AutoInstaller` folder to `Modules` (copy the folder, not it's content, and if its asks to Replace the existing files then press to replace them).

Contact in our discord if you have any problems. Just in case you can also read `Readme_EN.txt` or `Readme_RU.txt` inside of the installed Module.

## Discord

Feel free to join our [Discord Server][discordserver_url], ask questions, talk, and have fun!

## We're used by

- [ALKAD Hosting][hosting_alkad]

If you also use RocketModFix, contact us, we will add a link to you!

## How to Contribute

See here details [how to contribute][contributing].

## NuGet Packages

### Redist

[![RocketModFix.Unturned.Redist.Client][badge_RocketModFix.Unturned.Redist.Client]][nuget_package_RocketModFix.Unturned.Redist.Client]

[![RocketModFix.Unturned.Redist.Server][badge_RocketModFix.Unturned.Redist.Server]][nuget_package_RocketModFix.Unturned.Redist.Server]

[![RocketModFix.UnityEngine.Redist][badge_RocketModFix.UnityEngine.Redist]][nuget_package_RocketModFix.UnityEngine.Redist]

### RocketModFix

[![RocketModFix.Rocket.API][badge_RocketModFix.Rocket.API]][nuget_package_RocketModFix.Rocket.API]

[![RocketModFix.Rocket.Core][badge_RocketModFix.Rocket.Core]][nuget_package_RocketModFix.Rocket.Core]

[![RocketModFix.Rocket.Unturned][badge_RocketModFix.Rocket.Unturned]][nuget_package_RocketModFix.Rocket.Unturned]

## Resources

fr34kyn01535 has listed all of the original plugins in a post to the /r/RocketMod subreddit: [List of plugins from the old repository](https://www.reddit.com/r/rocketmod/comments/ek4i7b/)

The RocketMod organization on GitHub hosts several related archived projects: [RocketMod (Abandoned)](https://github.com/RocketMod)

## History

On the 20th of December 2019 Sven Mawby "fr34kyn01535" and Enes Sadık Özbek "Trojaner" officially ceased maintenance of Rocket. They kindly released the source code under the MIT license. [Read their full farewell statement here.](https://github.com/RocketMod/Rocket/blob/master/Farewell.md)

Following their resignation SDG forked the repository to continue maintenance in sync with the game.

On the 2nd of June 2020 fr34kyn01535 requested the fork be rebranded to help distance himself from the project.

## Credits

[OpenMod][openmod_github_repository] for nuget packages ready-to-go actions and workflows.

[discordserver_url]: https://discord.gg/z6VM7taWeG
[contributing]: https://github.com/RocketModFix/RocketModFix/blob/master/CONTRIBUTING.md
[keep_a_changelog_url]: https://keepachangelog.com/en/1.1.0/
[semver_url]: https://semver.org/
[rocketmodfix_logo]: https://raw.githubusercontent.com/RocketModFix/RocketModFix/master/resources/RocketModFix.png
[hosting_alkad]: https://hosting.alkad.org/
[openmod_github_repository]: https://github.com/openmod/openmod
[ldm_github_repository]: https://github.com/SmartlyDressedGames/Legally-Distinct-Missile

[nuget_package_RocketModFix.Unturned.Redist.Client]: https://www.nuget.org/packages/RocketModFix.Unturned.Redist.Client
[badge_RocketModFix.Unturned.Redist.Client]: https://img.shields.io/nuget/v/RocketModFix.Unturned.Redist.Client?label=RocketModFix.Unturned.Redist.Client&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRocketModFix.Unturned.Redist.Client
[nuget_package_RocketModFix.Unturned.Redist.Server]: https://www.nuget.org/packages/RocketModFix.Unturned.Redist.Server
[badge_RocketModFix.Unturned.Redist.Server]: https://img.shields.io/nuget/v/RocketModFix.Unturned.Redist.Server?label=RocketModFix.Unturned.Redist.Server&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRocketModFix.Unturned.Redist.Server
[nuget_package_RocketModFix.UnityEngine.Redist]: https://www.nuget.org/packages/RocketModFix.UnityEngine.Redist
[badge_RocketModFix.UnityEngine.Redist]: https://img.shields.io/nuget/v/RocketModFix.UnityEngine.Redist?label=RocketModFix.UnityEngine.Redist&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRocketModFix.UnityEngine.Redist
[nuget_package_RocketModFix.Rocket.API]: https://www.nuget.org/packages/RocketModFix.Rocket.API
[badge_RocketModFix.Rocket.API]: https://img.shields.io/nuget/v/RocketModFix.Rocket.API?label=RocketModFix.Rocket.API&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRocketModFix.Rocket.API
[nuget_package_RocketModFix.Rocket.Core]: https://www.nuget.org/packages/RocketModFix.Rocket.Core
[badge_RocketModFix.Rocket.Core]: https://img.shields.io/nuget/v/RocketModFix.Rocket.Core?label=RocketModFix.Rocket.Core&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRocketModFix.Rocket.Core
[nuget_package_RocketModFix.Rocket.Unturned]: https://www.nuget.org/packages/RocketModFix.Rocket.Unturned
[badge_RocketModFix.Rocket.Unturned]: https://img.shields.io/nuget/v/RocketModFix.Rocket.Unturned?label=RocketModFix.Rocket.Unturned&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRocketModFix.Rocket.Unturned
