# Nine Chronicles Mods

Welcome to the [Nine Chronicles](https://github.com/planetarium/NineChronicles) repository for mod development and distribution.

## BepInEx

This project uses [BepInEx](https://github.com/BepInEx/BepInEx), a framework for Unity game mods, to develop mods. See the [BepInEx Tutorial](./BEPINEX_TUTORIAL.md) documentation to learn how to use BepInEx.

## Tagging and Deployment

### Tagging

In order to distribute your mods, you need to create a version tag for each mod. The tags are created in the format `{PluginName}-{PluginVersion}`.

- PluginName: The name of the `BepInEx` plugin, the value you use for the `BepInPlugin` attribute of BepInEx.
- PluginVersion: The version of the `BepInEx` plugin, the value used for the `BepInPlugin` attribute of BepInEx.

For example, the tag for the `0.1.0` version of the `Athena` mode is `Athena-0.1.0`.

### Deployment

Once you're ready to deploy, you can deploy via GitHub Releases. This is where you upload the binary file along with the release notes so that users can easily download it.
The release notes detail the changes, bug fixes, new features, etc. so that users can easily understand what's in the update.

In the future, we plan to build an automated deployment process to streamline the deployment process.

## Install and Run

We'll use Athena Mod as an example to illustrate the installation and launch process.

### Installation: Windows

1. Download the latest version of Athena Mod from [GitHub release](https://github.com/planetarium/NineChronicles.Mods/releases): `Athena-x.y.z.zip`.
2. Press the `Windows + R` keys simultaneously to open the Run dialog box, type `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main` and press `Enter`.
3. In the folder that opens, unzip the downloaded file and paste the contents inside the `Athena-x.y.z` folder.

### Run

Each mod has different launch conditions or methods, and will either run automatically or need to be launched manually.
For Athena, you can activate it by launching the game and pressing the `Space` key after the loading screen.

> [!TIP]
> If the game does not run normally or the mod does not activate, double-check the installation process and make sure all files are in the correct locations.

## Mods

- [Athena](./NineChronicles.Mods.Athena): Enhances or changes your avatar's equipment, and simulates arena combat.
- [Illusionist](./NineChronicles.Mods.Illusionist): Changes your avatar's appearance and more.

## Modules

- [BlockSimulation](./NineChronicles.Modules.BlockSimulation): Modules that provide blockchain simulation functionality.

## Ad-Hoc

- [BurnAsset](./AdHoc.BurnAsset): A special purpose mod that uses the [BurnAsset](https://github.com/planetarium/lib9c/blob/main/Lib9c/Action/BurnAsset.cs) action.
- [RetrieveAvatarAssets](./AdHoc.RetrieveAvatarAssets): A special-purpose mode that uses the [RetrieveAvatarAssets](https://github.com/planetarium/lib9c/blob/main/Lib9c/Action/RetrieveAvatarAssets.cs) action.
