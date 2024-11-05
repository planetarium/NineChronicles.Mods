# BepInEx로 모딩 시작하기

## 모딩 준비하기

### 나인 크로니클 설치하기

- 나인 크로니클 웹사이트에서 설치 파일을 다운로드 합니다: https://nine-chronicles.com/start
- 다운로드한 설치 파일을 실행합니다.

### 나인 크로니클 실행하기

- 설치한 나인 크로니클 런처를 실행해서 나인 크로니클을 실행하여 잘 동작하는지 확인합니다.
- 확인 후에 나인 크로니클 클라이언트를 종료합니다. 런처는 종료하지 않아도 됩니다.

### BepInEx 5 LTS 설치하기

- OS와 CPU 아키텍쳐에 맞는 BepInEx 5 LTS 버전을 다운로드 합니다.
  - https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2
- 다운로드한 BepInEx 압축 파일을 나인 크로니클 클라이언트가 설치되어 있는 폴더에 해제합니다.
  - 윈도우즈: `C:\Users\{username}\Roaming\Nine Chronicles\player\main\`
- 처음에는 압축 해제한 `BepInEx` 폴더 안에 `core` 폴더만 있습니다.

<img width="327" alt="Untitled" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/1a089551-cfa4-44a6-9e47-7d4934bf9a8d">
<img width="335" alt="Untitled (1)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/a0bb3e6e-5e4a-4233-96c5-191adeac60d2">

### 나인 크로니클 동작 확인하기

- BepInEx 기본 설치 환경에서 나인 크로니클 클라이언트가 잘 동작하는지 확인합니다.
- 확인 후에 나인 크로니클 클라이언트를 종료합니다.

### 자동 생성된 BepInEx 구조 확인하고 설정하기

- 압축 해제한 `BepInEx` 폴더 안의 구성을 확인합니다.
  
  <img width="377" alt="Untitled (2)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/65b673e7-7427-45b6-baf3-a344f73c660b">

- `config` 폴더 안의 `BepInEx.cfg` 파일을 열어서 일부 설정을 수정합니다.
  
  ```
  [Chainloader]
    HideManagerGameObject = true
  [Logging.Console]
    Enabled = true
  [Preloader.Entrypoint]
    Type = Camera
  ```

### 나인 크로니클 동작 확인하기

- BepInEx 설정 수정 후, 나인 크로니클 클라이언트가 잘 동작하는지 확인합니다.
- BepInEx 로그 터미널이 열리는 것을 확인합니다.
- 확인 후에 나인 크로니클 클라이언트를 종료합니다.

## 모딩

### 새 프로젝트 만들기

- Target Framework: .NET Standard 2.1
- Lang Version: 9.0

### 기본 종속성 추가하기

**나인 크로니클 종속성**

위에서 설치한 나인 크로니클 클라이언트의 바이너리 파일을 참조합니다.

바이너리 파일 위치:

- 윈도우즈: `C:\Users{username}\Roaming\Nine Chronicles\player\main\NineChronicles_Data\Managed\`

바이너리 파일 목록:

- UnityEngine.CoreModule.dll
- UnityEngine.dll
- Lib9c.dll
- Nekoyume.dll

**BepInEx 종속성**

BepInEx의 핵심 바이너리 파일을 참조합니다.

바이너리 파일 위치:

- 윈도우즈: `C:\Users{username}\Roaming\Nine Chronicles\player\main\BepInEx\core\`

바이너리 파일 목록:

- 0Harmony.dll
- BepInEx.dll
  
### 플러그인 만들기

You can check example code here [https://github.com/planetarium/NineChronicles.Mods/tree/sample-dotnet-code/Sample]

**예제 플러그인 구현을 위해서 추가 종속성 추가하기**

예제에서 사용할 Unity 엔진의 InputLegacyModule 파일을 참조합니다.

바이너리 파일 위치:

- 윈도우즈: `C:\Users{username}\Roaming\Nine Chronicles\player\main\NineChronicles_Data\Managed\`

바이너리 파일 목록:

- UnityEngine.InputLegacyModule.dll

**예제 코드**

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

### 모드 적용하기

**모드 빌드하기**

- TestMod.dll

**나인 크로니클 클라이언트 경로에 모드 바이너리 파일 복사하기**

- Windows: `C:\Users{username}\Roaming\Nine Chronicles\player\main\BepInEx\plugins\`

### 모드 동작 확인하기

- 모드 바이너리 파일 적용 후, 나인 크로니클 클라이언트가 잘 동작하는지 확인합니다.
- BepInEx 로그 터미널에서 모드가 잘 로드되는 것을 확인합니다.
  - Loading [Test Mod 0.1.0]
  - Test Mod is loaded!
    
    <img width="500" alt="Untitled (3)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/fc0419d0-1ef9-468a-a7b5-ffec65240ddc">
- 모드가 잘 동작하는 것을 확인합니다.
  - e.g., 스페이스 키를 입력하면, 나인 크로니클 클라이언트의 `NotificationSystem.Push()` 기능이 동작합니다.
    
    <img width="419" alt="Untitled (4)" src="https://github.com/Atralupus/NineChronicles.Mods/assets/30599098/a64fc4e4-ba85-483d-90df-24557b741955">
- 확인 후에 나인크로니클 플레이어를 종료합니다.

> [!NOTE]
> 축하합니다! 테스트 모드가 성공적으로 만들어졌습니다. 이제 다른 모드를 만들어 보거나 기존 모드를 개선해 보세요.
