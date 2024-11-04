# Nine Chronicles Mods

[나인 크로니클](https://github.com/planetarium/NineChronicles) 모드 개발과 배포를 위한 저장소에 오신 것을 환영합니다.

## BepInEx

이 프로젝트는 Unity 게임 모드를 위한 프레임워크인 [BepInEx](https://github.com/BepInEx/BepInEx)를 사용해서 모드를 개발합니다. BepInEx를 사용하는 방법은 [BepInEx 튜토리얼](./BEPINEX_TUTORIAL.md) 문서를 참조하세요.

## 태깅과 배포

### 태깅

모드를 배포하기 위해서는 각 모드의 버전 태그를 생성해줘야 합니다. 태그는 `{PluginName}-{PluginVersion}` 형식으로 생성합니다.

- PluginName: `BepInEx` 플러그인의 이름으로, BepInEx의 `BepInPlugin` 어트리뷰트에 사용하는 값입니다.
- PluginVersion: `BepInEx` 플러그인의 버전으로, BepInEx의 `BepInPlugin` 어트리뷰트에 사용하는 값입니다.

예를 들어서 `Athena` 모드의 `0.1.0` 버전용 태그는 `Athena-0.1.0`입니다.

### 배포

배포 준비가 완료되면, GitHub Releases를 통해 배포할 수 있습니다. 이때, 릴리스 노트와 함께 바이너리 파일을 업로드하여 사용자들이 쉽게 다운로드할 수 있도록 합니다.
릴리스 노트에는 변경 사항, 버그 수정, 새로운 기능 등을 상세히 기재하여 사용자들이 업데이트 내용을 쉽게 이해할 수 있도록 합니다.

추후에는 자동화된 배포 프로세스를 구축하여 배포 과정을 간소화할 계획입니다.

## 설치와 실행

아테나 모드를 예로 설치와 실행 과정을 설명하겠습니다.

### 설치: 윈도우즈

1. [깃헙 릴리즈](https://github.com/planetarium/NineChronicles.Mods/releases)에서 최신 버전의 아테나 모드 압축 파일을 다운로드합니다: `Athena-x.y.z.zip`
2. `윈도우즈 + R` 키를 동시에 눌러 실행 대화 상자를 열고 `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main`을 입력한 후 `엔터` 키를 누릅니다.
3. 열리는 폴더에 다운로드한 파일의 압축을 풀고 `Athena-x.y.z` 폴더 안에 있는 내용을 붙여넣습니다.

### 실행

각 모드는 실행 조건이나 방법이 다르며, 자동 실행되거나 수동으로 실행해야 합니다.
아테나의 경우에는 게임을 실행하고 로딩 화면이 끝나면 `스페이스` 키를 눌러서 활성화할 수 있습니다.

> [!TIP]
> 게임이 정상적으로 실행되지 않거나 모드가 활성화되지 않는 경우, 설치 과정을 다시 확인하고 모든 파일이 올바른 위치에 있는지 확인하세요.

## 모드 목록

- [Athena](./NineChronicles.Mods.Athena): 아바타의 장비를 강화하거나 변경하고, 아레나 전투를 시뮬레이션합니다.
- [Illusionist](./NineChronicles.Mods.Illusionist): 아바타의 외형 등을 변경합니다.

## 모듈 목록

- [BlockSimulation](./NineChronicles.Modules.BlockSimulation): 블록체인 시뮬레이션 기능을 제공하는 모듈입니다.

## 에드혹

- [BurnAsset](./AdHoc.BurnAsset): [BurnAsset](https://github.com/planetarium/lib9c/blob/main/Lib9c/Action/BurnAsset.cs) 액션을 사용하는 특수 목적의 모드입니다.
- [RetrieveAvatarAssets](./AdHoc.RetrieveAvatarAssets): [RetrieveAvatarAssets](https://github.com/planetarium/lib9c/blob/main/Lib9c/Action/RetrieveAvatarAssets.cs) 액션을 사용하는 특수 목적의 모드입니다.
