# CI/CD 준비 점검 (2026-10-08)

## 현재 저장소

- 원격 저장소: GitHub `No1playblock/Project_Herbalist`; 기존 CI 워크플로 없음.
- Unity Editor: `6000.6.0f1` (`ProjectSettings/ProjectVersion.txt`).
- 활성 빌드 씬: 7개 (`ProjectSettings/EditorBuildSettings.asset`), 시작 씬은 `MainMenu`.
- `Packages/manifest.json`은 Git URL의 `com.coplaydev.unity-mcp`와 네 개의 embedded package를 사용한다. CI에는 UPM과 Git 네트워크 접근이 필요하다.
- 프로젝트 자체의 Editor 검증은 주로 `Assets/_Project/Tests/Editor`의 정적 `Run()` 함수다. 현재 Unity Test Runner의 `[Test]` 메서드로 등록되지 않아 `-runTests`만 실행하는 CI는 이 검증을 수행하지 않는다. 런타임 네트워크 smoke test도 별도 실행 절차가 필요하다.

## 깨끗한 체크아웃에서 막힐 수 있는 항목

2026-10-08 로컬 에디터의 7개 활성 씬에 대해 `AssetDatabase.GetDependencies`를 조회한 결과, 프로젝트 `Assets/` 의존성 553개 중 110개가 Git 인덱스에 없다. 이 중 99개는 현재 스테이지 1 아트 경로에 있고, 2개는 게임 스크립트다. 모든 미추적 의존성에는 로컬 `.meta` 파일이 존재한다. 씬과 코드 변경을 커밋할 때 실제 의존 에셋 및 `.meta`도 함께 포함해야 한다. 이 점검은 Git 인덱스를 수정하지 않았다.

`Assets/_Project/Data/Stages/Stage_01/Stage1_ModelingTerrain.asset`은 작업 파일이 약 126 MB이다. 작업 중인 `.gitattributes`에는 이 파일의 LFS 규칙이 있지만, 현재 Git 인덱스의 본문은 약 90 MB의 일반 Git blob이다. 다음 버전을 추가하기 전에 LFS 포인터로 실제 변환됐는지 확인해야 한다. GitHub는 일반 Git 파일 100 MiB 초과를 차단한다: https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github

## 아직 결정할 사항

1. 이번 범위: 저장소 검증·로컬 빌드 진입점까지만 준비할지, GitHub Actions 워크플로도 작성할지.
2. 첫 빌드 대상: Windows x64 권장. 이후 다른 플랫폼은 별도 Unity 빌드 모듈과 작업으로 확장.
3. CI 실행 방식과 라이선스: GitHub Actions 자체 Windows 러너, GitHub 호스팅 러너, 또는 Unity Build Automation 중 선택. Unity Personal은 Unity Hub로 라이선스를 활성화해야 하므로 CI 러너 방식에 영향을 준다: https://docs.unity.com/en-us/engine/6000.0/manual/get-started/install-and-upgrade/licenses-and-activation/license-activation-methods
4. CD 대상: 우선 빌드 아티팩트만 보관할지, 릴리스/스토어 배포까지 자동화할지.

Unity 커맨드라인 빌드는 명시적 `-buildTarget` 또는 빌드 프로필, `-batchmode`, `-quit`, 로그 경로를 사용하도록 준비한다: https://docs.unity3d.com/6000.0/Documentation/Manual/build-command-line.html
