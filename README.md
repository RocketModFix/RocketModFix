# Legally Distinct Missile

<<<<<<< Updated upstream
The **Legally Distinct Missile** (or LDM) is a fork of Rocket for Unturned maintained by the game developers (SDG) after the resignation of its original community team. Using this fork is recommended because it preserves compatibility, and has fixes for important legacy Rocket issues like multithreading exceptions and teleportation exploits.
=======
## RocketModFix

The **RocketModFix** is a fork of RocketMod for Unturned maintained by the Unturned plugin devs, this fork don't have plans for any major changes to the RocketMod, only fixes and new features that doesn't break any backward compatibility with API.

## Compatibility

You can still use old RocketMod plugins without any changes/recompilation/updates, however if you want to use new features and bug fixes we recommend to install updated Module and new Rocket. API/Core/Unturned Redistributables (libraries).

## Our plan

- [x] Create Discord Server Community.
- [x] UnityEngine NuGet Package redist.
- [x] Unturned NuGet Package redist.
- [x] Update MSBuild to the `Microsoft.NET.Sdk`, because current MSBuild in RocketMod is outdated and its hard to support and understand what's going on inside.
- [x] RocketMod NuGet Package containing all required libraries for RockeMod API usage.
- [x] CI/CD and nightly builds with RocketMod .dlls.
- [x] Automatic Release on Tag creation (with RocketMod Module).
- [x] Rocket.Unturned NuGet Package.
- [x] Reset changelog.
- [x] For changelog use [Keep a Changelog standard][keep_a_changelog_url].
- [x] For versioning use [SemVer][semver_url].
- [x] Installation guides inside of the Rocket Unturned Module.
- [ ] Keep backward compatibility.
	- [ ] Test with RocketMod plugins that uses old RocketMod libraries, and make sure current changes doesn't break anything.
	- [ ] Test with most used Modules:
		- [ ] AviRockets.
		- [ ] uScript.
		- [ ] OpenMod.
- [ ] RocketMod Fixes:
	- [ ] Fix UnturnedPlayer.SteamProfile (cause so many lags). 
	- [x] Fix UnturnedPlayerComponent is not being added and removed automatically.
	- [ ] Assembly Resolve fixes (don't spam with not found library or make a option to disable it, load all libraries at rocketmod start instead of searching for them only on OnAssemblyResolve)
	- [ ] Commands fixes:
		- [ ] Fix /vanish.
		- [ ] Fix /god.
		- [ ] Fix /p (not readable at all).
	- [ ] Perfomance.
- [x] New Features:
	- [x] Commands:
		- [x] /position /pos (current position of the player).
		- [ ] /tpall (teleport everyone to self or Vector3 point)
- [ ] Gather a Team with a direct access to the repo edit without admins help.
- [ ] RocketModFix Video Installation Guide (could be uploaded on YouTube).

After plan is finished -> Add new plans, keep coding, and don't forget to accept PR or issues.
>>>>>>> Stashed changes

## Installation

The dedicated server includes the latest version, so an external download is not necessary:
1. Copy the Rocket.Unturned module from the game's Extras directory.
2. Paste it into the game's Modules directory.

## Contributing

The goals of this repository are to maintain compatibility with Unturned, maintain backwards compatibility with plugins, and fix bugs. Changes outside that scope will be made to the Unturned API rather than reworking the Rocket API. New plugins should ideally be using the game API where possible.

Issues are monitored and will be discussed, but pull requests will not be directly merged.

## Resources

fr34kyn01535 has listed all of the original plugins in a post to the /r/RocketMod subreddit: [List of plugins from the old repository](https://www.reddit.com/r/rocketmod/comments/ek4i7b/)

Following closure of the original forum the recommended sites for developer discussion are the [/r/UnturnedLDM](https://www.reddit.com/r/UnturnedLDM/) subreddit, [SDG Forum](https://forum.smartlydressedgames.com/c/modding/ldm), or the [Steam Discussions](https://steamcommunity.com/app/304930/discussions/17/).

The RocketMod organization on GitHub hosts several related archived projects: [RocketMod (Abandoned)](https://github.com/RocketMod)

## History

On the 20th of December 2019 Sven Mawby "fr34kyn01535" and Enes Sadık Özbek "Trojaner" officially ceased maintenance of Rocket. They kindly released the source code under the MIT license. [Read their full farewell statement here.](https://github.com/RocketMod/Rocket/blob/master/Farewell.md)

Following their resignation SDG forked the repository to continue maintenance in sync with the game.

On the 2nd of June 2020 fr34kyn01535 requested the fork be rebranded to help distance himself from the project.
