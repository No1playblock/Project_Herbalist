# Shared UGUI

The active menu is the authored `PF_MainMenuScreen` in MainMenu. Game overlay and player HUD prefabs are authored before play and toggled with SetActive. The active route is MainMenu -> `Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference` -> `Stage_02_Mazes` -> `Stage_03_BranchRide`. `Sandbox_Abilities` remains for separate ability testing.

The current Stage 1 includes `PlayScreenController`, `GameplayCursor`, the shared `PF_GameOverlayHud`, one `PF_PlayerHud` per spawn slot, and an `EventSystem` with the Input System UI module. The cursor captures and hides while the Game view has focus, and releases for UI modes or when the release action is used. HUD roots are authored in the scene/prefabs and remain presentation-only.

## 원화 담당자용 실제 게임 UI 작업 (2026-11-04)

별도 미리보기 씬 없이 실제 플레이 UI에 이미지를 넣는다. [원화 적용 가이드](UIArtworkGuide.md)를 담당자에게 전달한다.

- `Art_…` 오브젝트의 `Image > Source Image`를 교체하고 해당 오브젝트의 RectTransform을 조정한다.
- 새 PNG는 `Assets/_Project/Art/UI/Textures/ArtistDrop/`에 넣으면 Sprite / Single / 투명도 / Bilinear / Clamp가 자동 설정된다. 기존 파일의 재가져오기 설정은 덮어쓰지 않는다.
- 고정 배경의 기존 색은 placeholder Sprite에 옮겼고, 교체용 Image는 흰색이다. 원래 Graphic 컴포넌트와 기능 오브젝트는 보존하며 기존 Graphic은 비활성화한다. Button/Slider/InputField의 Target Graphic은 새 이미지에 연결했다.
- `Art_StateTint_…`는 기존 상태 색상 제어를 유지한다. `Art_Fill_…`는 Filled 설정과 코드의 게이지 참조를 유지한다.
- 캐릭터 선택 카드에는 `Art_DuyeongPortrait`, `Art_SodamPortrait`를 마련했다. 이전 임시 도형은 비활성으로 보존했다.
- 1스테이지는 공통 프리팹을 사용한다. 2·3스테이지는 씬 내부 UI이므로 해당 씬의 `Art_…`를 수정한다. 공통 프리팹 수정이 이 두 씬에 자동 전파되지는 않는다.
- `Herbalist > UI Art` 메뉴에서 공통 UI 프리팹을 바로 연다. `Prepare Artwork Slots`는 개발자용 재설정 명령이며 원화 담당자가 실행할 필요는 없다. 편집 중인 2·3스테이지가 미저장 상태라면 해당 씬은 저장하지 않고 중단한다.
