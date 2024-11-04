우선 어떤 형태로든 당신의 기여에 감사합니다. 여기서는 아테나 모드(이하 아테나)를 예시로 모드를 사용하고 개발하는 방법을 간단히 설명합니다.

# 개발에 앞서

## 나인크로니클 플레이해보기

아래의 경로에서 나인크로니클을 설치하고, 게임이 원활하게 동작하는지를 확인합니다.
- https://nine-chronicles.com/start

## 아테나 사용해보기

1. 최신 버전의 아테나를 다운로드하고 나인크로니클에 적용합니다.
   - [Releases](https://github.com/planetarium/NineChronicles.Mods/releases)
2. 나인크로니클을 실행한 후, 게임의 메인 로비 화면에서 스페이스 바를 눌러서 아테나를 실행하고 사용해봅니다.

## (선택사항) BepInEx 익히기

[BEPINEX_TUTORIAL.md](./BEPINEX_TUTORIAL.md) 파일을 열어서 간단한 튜토리얼을 진행해봅니다.

# 개발하기

## 개발환경 준비하기

1. 적절한 IDE를 포함하는 .NET 개발 환경을 준비합니다.
2. 본 저장소를 개발 환경에 클론합니다.
3. 테스트하고자 하는 NineChronicles 게임을 설치합니다.

## 솔루션 실행하기

1. `NineChronicles.Mods.sln` 솔루션을 실행합니다.
2. `.env.xml` 파일을 설정합니다.
   - [.env.macOS.xml](./.env.macOS.xml) 파일과 [.env.Windows.xml](./.env.Windows.xml) 파일을 참고해서 `.env.xml` 파일을 만들어 설정합니다.

## 아테나 프로젝트 빌드하고 나인크로니클에 적용하기

1. `NineChronicles.Mods.Athena` 프로젝트를 빌드합니다.
2. BepInEx 파일들과 아테나에 필요한 파일들을 나인크로니클 클라이언트 경로에 복제합니다.
   - [NineChronicles.Mods.Athena.dll](./NineChronicles.Mods.Athena/bin/Release/netstandard2.1/NineChronicles.Mods.Athena.dll)
   - [NineChronicles.Modules.BlockSimulation.dll](NineChronicles.Mods.Athena/bin/Release/netstandard2.1/NineChronicles.Modules.BlockSimulation.dll)

### 스크립트 사용하기

아테나 프로젝트를 빌드하고 나인크로니클에 적용하는 스크립트가 준비되어 있습니다.

- [inject-plugins.ps1](./scripts/inject-plugins.ps1)
- [inject-plugins.sh](./scripts/inject-plugins.sh)

아래와 같이 저장소 루트 경로에서 스크립트를 실행합니다.

```bash
./scripts/inject-plugins.sh
```

## 아테나 테스트하기

설치한 나인크로니클을 실행한 후, 새롭게 적용한 아테나의 기능을 테스트합니다.
