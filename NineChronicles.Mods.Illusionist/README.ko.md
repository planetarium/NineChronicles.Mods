# 일루셔니스트

일루셔니스트는 이미지나 오디오 등의 에셋을 원하는데로 바꿀 수 있게 도와줍니다.

## 주요 기능

- 무기 이미지 교체: 일루셔니스트를 통해 플레이어는 무기의 외형을 원하는 스타일로 변경할 수 있습니다.

## 설치와 실행

### 설치: 윈도우즈

1. [깃헙 릴리즈](https://github.com/planetarium/NineChronicles.Mods/releases)에서 최신 버전의 일루셔니스트 모드 압축 파일을 다운로드합니다: `Illusionist-x.y.z.zip`
2. `윈도우즈 + R` 키를 동시에 눌러 실행 대화 상자를 열고 `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main`을 입력한 후 `엔터` 키를 누릅니다.
3. 열리는 폴더에 다운로드한 파일의 압축을 풀고 `Illusionist-x.y.z` 폴더 안에 있는 내용을 붙여넣습니다.

### 실행

일루셔니스트는 별도의 조건 없이 자동 실행 됩니다.

> [!TIP]
> 게임이 정상적으로 실행되지 않거나 모드가 활성화되지 않는 경우, 설치 과정을 다시 확인하고 모든 파일이 올바른 위치에 있는지 확인하세요.

## 무기 이미지 교체

### 대상 무기 선정

무기 교체를 위해서는 무기의 시트 ID가 필요합니다. 이를 알아보기 위해서 9c-board 서비스를 사용할 수 있습니다:
- https://9c-board.nine-chronicles.dev/{planet-name}/avatar/{address}
    - `planet-name`에는 아래의 값을 사용할 수 있습니다:
        - odin
        - heimdall
    - `address`에는 아바타의 주소를 입력합니다.

예를 들어서 `odin` 플래닛의 `0xe56d432da2032f6C851943b76C4a41815baaBB54` 주소에 있는 아바타는 다음과 같은 URL을 사용할 수 있습니다:
- `https://9c-board.nine-chronicles.dev/odin/avatar/0xe56d432da2032f6C851943b76C4a41815baaBB54`

원하는 무기 이미지에 마우스를 올려 무기의 시트 ID를 확인하세요.

![image](https://github.com/user-attachments/assets/48f471a2-4b24-43f7-baac-91ee21781da2)

### 무기 이미지 변경

무기 이미지 파일을 교체하려면, 아래 경로에 원하는 무기 이미지 파일을 저장하세요:
- `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main\BepInEx\plugins\Illusionist\CharacterTextures\Weapons\{무기의 시트 ID}.png`

본 예시에서는 무기를 벗었을 때 사용하는 나무 막대기의 시트 ID인 `10100000`을 사용해보겠습니다. 아래 경로에 원하는 무기 이미지 파일을 `10100000.png`로 저장하면 게임 내에서 해당 무기의 외형이 변경됩니다:
- `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main\BepInEx\plugins\Illusionist\CharacterTextures\Weapons\10100000.png`

제가 사용한 무기 이미지입니다.

![image](https://github.com/user-attachments/assets/66a2ff15-ca9c-4e91-98ca-c285058f1499)

이제 게임을 실행하고, 무기를 벗으면 기본 무기인 나무 막대기 대신 새로운 이미지로 변경된 외형을 확인할 수 있습니다.

<img width="273" alt="image" src="https://github.com/user-attachments/assets/a635b79b-296f-4c2d-8b99-6d5a39861564">
<img width="670" alt="image" src="https://github.com/user-attachments/assets/fa5bb6a9-b6cd-46bd-a9b2-b97a5ac0e220">
