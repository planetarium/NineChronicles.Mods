# Getting Started with Modding using BepInEx

## Preparing for modding

### Install Nine Chronicles

- Installing Nine Chronicles at https://nine-chronicles.com/start

### Running Nine Chronicles

- Run the Nine Chronicles launcher you installed to run Nine Chronicles and verify that it works.
- After confirmation, exit the Nine Chronicles client. You do not need to exit the launcher.

### Installing BepInEx 5 LTS

- Download the BepInEx 5 LTS version for your OS and CPU architecture.
  - https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2
- Unzip the downloaded BepInEx archive to the folder where the Nine Chronicles client is installed.
  - Windows: `C:\Users{username}\Roaming\Nine Chronicles\player\main\`
- Initially, there is only the `core` folder inside the unzipped `BepInEx` folder.

<img width="327" alt="Untitled" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/1a089551-cfa4-44a6-9e47-7d4934bf9a8d">
<img width="335" alt="Untitled (1)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/a0bb3e6e-5e4a-4233-96c5-191adeac60d2">

### Verifying Nine Chronicles Operation

- Verify that the Nine Chronicles client is working well in the BepInEx default installation environment.
- After verification, exit the Nine Chronicles client.

### Reviewing and Configuring the Automatically Generated BepInEx Structure

- Review the structure inside the extracted BepInEx folder.
  
  <img width="377" alt="Untitled (2)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/65b673e7-7427-45b6-baf3-a344f73c660b">

- Open the `BepInEx.cfg` file inside the `config` folder to modify some settings.
  
  ```
  [Chainloader]
    HideManagerGameObject = true
  [Logging.Console]
    Enabled = true
  [Preloader.Entrypoint]
    Type = Camera
  ```

### Verifying Nine Chronicles Operation

- After modifying the BepInEx settings, verify that the Nine Chronicles client is working properly.
- Verify that the BepInEx log terminal opens.
- After verification, exit the Nine Chronicles client.

## Modding

### Create a New dotnet Project

- Target Framework: .NET Standard 2.1
- Lang Version: 9.0

## Add Basic Dependencies

**Nine Chronicle dependencies**

References the binary files for the Nine Chronicles client you installed above.

Binary file location:

- Windows: `C:\Users{username}\Roaming\Nine Chronicles\player\main\NineChronicles_Data\Managed\`

Binary files:

- UnityEngine.CoreModule.dll
- UnityEngine.dll
- Lib9c.dll
- Nekoyume.dll

**BepInEx Dependencies**

References the core binary files of BepInEx.

Binary file location:

- Windows: `C:\Users{username}\Roaming\Nine Chronicles\player\main\BepInEx\core\`

Binary files:

- 0Harmony.dll
- BepInEx.dll
  
### Create a Plugin

You can check example code here [https://github.com/planetarium/NineChronicles.Mods/tree/sample-dotnet-code/Sample]

**Adding additional dependencies to implement the example plugin**

Reference the InputLegacyModule file in the Unity engine that we will use in our example.

Binary file location:

- Windows: `C:\Users{username}\Roaming\Nine Chronicles\player\main\NineChronicles_Data\Managed\`

Binary files:

- UnityEngine.InputLegacyModule.dll

**Example code**

<details>
<summary>TestModPlugin.cs</summary>

```cs
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace NineChronicles.Mods.TestMod
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TestModPlugin : BaseUnityPlugin
    {
        private const string ModGUID = "com.ninechronicles.mods.testmod";
        private const string ModName = "Test Mod";
        private const string ModVersion = "0.1.0";

        private static TestModPlugin Instance;

        private readonly Harmony _harmony = new Harmony(ModGUID);

        internal ManualLogSource _logger;

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
            }

            _logger = Logger.CreateLogSource(ModGUID);
            _logger.LogInfo($"{ModName} is loaded!");

            _harmony.PatchAll(typeof(TestModPlugin));
            _harmony.PatchAll(typeof(Patches.NotifyKeyboardInput));
        }
    }
}
```
</details>

<details>
<summary>NotifyKeyboardInput.cs</summary>

```cs
using HarmonyLib;
using Nekoyume.Game;
using Nekoyume.UI;
using UnityEngine;

namespace NineChronicles.Mods.TestMod.Patches
{
    [HarmonyPatch(typeof(ActionCamera))]
    internal class NotifyKeyboardInput
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void PostfixUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NotificationSystem.Push(
                    Nekoyume.Model.Mail.MailType.System,
                    "Space key is pressed!",
                    Nekoyume.UI.Scroller.NotificationCell.NotificationType.Notification);
            }
        }
    }
}
```
</details>

### Applying Mods

**Building a Mod**

- TestMod.dll

**Copy the mod binary file to the Nine Chronicles client path**

- Windows: `C:\Users{username}\Roaming\Nine Chronicles\player\main\BepInEx\plugins\`

### Check mode behavior

- After applying the mod binary file, verify that the Nine Chronicles client is working well.
- In the BepInEx log terminal, verify that the mod is loading fine.
  - Loading [Test Mod 0.1.0]
  - Test Mod is loaded!
    
    <img width="500" alt="Untitled (3)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/fc0419d0-1ef9-468a-a7b5-ffec65240ddc">
- Verify the mod operates as expected.
  - e.g., Pressing the space key triggers the 9c-unity NotificationSystem.Push() function.
    
    <img width="419" alt="Untitled (4)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/a64fc4e4-ba85-483d-90df-24557b741955">
- Close the Nine Chronicles client after confirmation.

> [!NOTE]
> Congratulations, you've successfully created a test mode, now try creating another mode or improving an existing one.
