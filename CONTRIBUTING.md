First of all, thank you for your contributions in any form. In this article, we will briefly explain how to use and develop mods using the Athena mod (hereafter Athena) as an example.

# Before development

## Play NineChronicles

Install NineChronicles from the path below and verify that the game runs smoothly.
- https://nine-chronicles.com/start

## Try Athena

1. Download the latest version of Athena mod and apply it to NineChronicles.
   - [Releases](https://github.com/planetarium/NineChronicles.Mods/releases)
2. Launch NineChronicles, and from the game's main lobby screen, press the space bar to launch and try Athena.

## (optional) Familiarize yourself with BepInEx

Open the file [BEPINEX_TUTORIAL.md](./BEPINEX_TUTORIAL.md) and go through a short tutorial.

# Develop

## Prepare your development environment

1. Prepare a .NET development environment that includes an appropriate IDE.
2. Clone the this repository to your development environment.
3. Install the NineChronicles you want to test.

## Run the solution

1. Run the `NineChronicles.Mods.sln` solution.
2. Set up the `.env.xml` file.
   - Refer to the [.env.macOS.xml](./.env.macOS.xml) file and the [.env.Windows.xml](./.env.Windows.xml) file to create and configure the `.env.xml` file.

## Build the Athena project and apply it to NineChronicles

1. Build the project `NineChronicles.Mods.Athena`.
2. Duplicate the BepInEx files and the files required for Athena to the NineChronicles client path.
   - [NineChronicles.Mods.Athena.dll](./NineChronicles.Mods.Athena/bin/Release/netstandard2.1/NineChronicles.Mods.Athena.dll)
   - [NineChronicles.Modules.BlockSimulation.dll](NineChronicles.Mods.Athena/bin/Release/netstandard2.1/NineChronicles.Modules.BlockSimulation.dll)

### Using the script

We've prepared a script to build the Athena project and apply it to NineChronicles.

- [inject-plugins.ps1](./scripts/inject-plugins.ps1)
- [inject-plugins.sh](./scripts/inject-plugins.sh)

Run the script from the root path of your repository, as shown below.

```bash
./scripts/inject-plugins.sh
```

## Test Athena

After running your NineChronicles installation, test the functionality of your newly applied Athena.
