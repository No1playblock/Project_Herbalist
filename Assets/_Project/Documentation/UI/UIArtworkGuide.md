# 원화 이미지를 실제 게임에 넣기

## 담당자가 하는 작업

1. Play 모드를 종료한다.
2. PNG를 `Assets/_Project/Art/UI/Textures/ArtistDrop/`에 넣는다. 새 파일은 UI용 Sprite로 자동 설정된다.
3. 상단 `Herbalist > UI Art` 메뉴에서 수정할 UI를 연다.
4. Hierarchy에서 `Art_`를 검색하고 대상 오브젝트를 선택한다.
5. Inspector의 **Image > Source Image**에 그림을 넣는다.
6. 그 오브젝트의 **RectTransform** 또는 Rect 도구(T)로 위치·크기를 조정한다.
7. 프리팹을 저장하고 프리팹 편집을 종료한다. `MainMenu` 씬에서 Play를 눌러 실제 게임을 진행하며 확인한다.

Play 중 변경은 종료하면 사라진다. 수정은 Play를 종료한 다음 저장한다. Auto Save를 끈 프리팹은 별도로 Save한다.

## 어느 자리에 넣는지

아래 경로는 각 프리팹 내부 경로다. 기능을 담당하는 부모 대신 **마지막 Art_ 오브젝트**를 선택한다.

| 화면 | 프리팹 / 이미지 자리 |
|---|---|
| 타이틀 로고 | `PF_MainMenuScreen / Home / MenuPaper / Logo / Art_Logo` |
| 타이틀 메뉴판 | `PF_MainMenuScreen / Home / MenuPaper / Art_MenuPaper` |
| 방 만들기 버튼 | `PF_MainMenuScreen / Home / MenuPaper / Create / Art_Create` |
| 방 참가·옵션·종료 버튼 | 같은 위치의 `Join / Art_Join`, `Options / Art_Options`, `Quit / Art_Quit` |
| 방 코드 입력 배경 | `PF_MainMenuScreen / RoomEntry / RoomCode / Art_RoomCode` |
| 두영 선택 카드 배경 | `PF_MainMenuScreen / CharacterRoom / DuyeongCard / Art_DuyeongCard` |
| 두영 일러스트 | `PF_MainMenuScreen / CharacterRoom / DuyeongCard / Art_DuyeongPortrait` |
| 소담 일러스트 | `PF_MainMenuScreen / CharacterRoom / SodamCard / Art_SodamPortrait` |
| 조준점 | `PF_PlayerHud / Reticle / Art_Reticle` |
| 상호작용 배경 | `PF_PlayerHud / Interaction / Art_Interaction` |
| 천·수액 안내 배경 | `ClothInteraction / Art_ClothInteraction`, `SapInjectionInteraction / Art_SapInjectionInteraction` |
| 도움말 배경 | `PF_PlayerHud / HelpPanel / Art_HelpPanel` |
| 잎 상태 아이콘 | `PF_PlayerHud / LeafStatus / Art_StateTint_LeafSlot1Dot` 등 3개 |
| 제조 화면 배경·솥 | `PF_PlayerHud / BrewingHud / Art_BrewingHud`, `BrewingPotAndFire / Art_BrewingPotAndFire` |
| 온도계 테두리 | `PF_PlayerHud / BrewingHud / ThermometerFrame / Art_ThermometerFrame` |
| 스테이지 목표 배경 | `PF_GameOverlayHud / StageHeader / Art_StageHeader` |
| 일시정지 배경 | `PF_GameOverlayHud / Pause / Art_Pause` |
| 재개·옵션·메뉴 버튼 | `PF_GameOverlayHud / Pause / OwnerMenu` 아래 각 버튼의 `Art_…` |
| 옵션 창·슬라이더 | `PF_OptionsPanel / Card / Art_Card`, `Volume`·`Sensitivity` 아래 `Art_Track`·`Art_Handle` |
| 로딩 안내 카드 | `PF_LoadingPanel / ProgressCard / Art_ProgressCard` |

`Art_Texture_…`는 기존 **RawImage**이므로 **Texture** 칸을 교체한다. 일시정지 화면의 `PauseBackdrop`은 게임 화면 캡처용이므로 교체하지 않는다. 타이틀 뒤의 실제 3D 맵과 카메라는 UI 원화 슬롯과 별개다.

## 위치·크기 조정

- 기준 화면은 **1280×720**, Canvas는 Scale With Screen Size / Match 0.5다. Canvas 설정은 이미 맞춰져 있다.
- 로고·일러스트·조준점은 비율을 유지해서 조정한다. 배경과 프레임은 부모 영역을 채우도록 준비되어 있어 Left / Right / Top / Bottom 여백을 조정할 수 있다.
- 그림 크기를 바꿀 때는 Art 오브젝트를 조정한다. 버튼의 클릭 영역과 글자도 함께 옮기고 싶을 때만 부모 버튼의 RectTransform을 조정한다.
- 조준점, 상호작용 안내, 온도 표시의 부모 위치는 게임이 제어한다. 내부 Art 오브젝트를 옮기면 게임 중에도 그 상대 위치가 유지된다.
- 두영·소담 HUD의 최상위 영역은 분할/개인 화면 전환 시 코드가 조정한다. 최상위 HUD의 Anchor는 변경하지 않는다. 두 화면 모드에서 크기와 글자 겹침을 확인한다.

## 그림을 준비할 때

- 버튼·프레임·아이콘·일러스트는 투명 PNG로 각각 준비한다. 버튼 글자는 기존 TMP 텍스트를 사용하므로 그림에 중복해서 넣지 않는다.
- 일반 `Art_…`의 Image Color는 흰색으로 준비되어 있다. 버튼의 마우스 오버·클릭·비활성 색은 기존 Button 설정으로 작동한다.
- `Art_StateTint_…`는 잎의 보유/설치/회수 상태에 따라 색이 바뀐다. 흰색 또는 밝은 단색 아이콘을 넣으면 상태 색이 잘 보인다. 상태 색 자체는 개발 담당자가 `SO_GameUiSettings`에서 조정한다.
- `Art_Fill_…`는 게이지다. **Image Type = Filled**, Fill Method / Origin과 스크립트 연결을 유지한다.
- Raycast Target, Button, Slider, 제어 스크립트는 이미 연결되어 있다. 해당 설정과 비활성인 원래 Graphic은 그대로 둔다.
- `ArtworkPlaceholders` 폴더는 기본 모양을 유지하는 임시 이미지다. 이 파일을 덮어쓰면 여러 슬롯이 함께 바뀔 수 있으므로 새 그림은 `ArtistDrop`에 넣어 슬롯별로 지정한다.

## 실제 씬에 반영되는 범위

- **타이틀:** `Assets/_Project/Scenes/01_Title/MainMenu.unity`. `PF_MainMenuScreen` 수정이 반영된다.
- **1스테이지:** `Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference`. 공통 `PF_PlayerHud`·`PF_GameOverlayHud`를 사용한다. 각 플레이어 HUD의 slot과 분할 화면 설정은 유지한다.
- **2스테이지:** `Stage_02_Mazes`. UI가 씬 내부에 있으므로 Hierarchy의 `PlayerHudUI`·`GameOverlayUI` 안에서 `Art_…`를 찾아 수정하고 씬을 저장한다.
- **3스테이지:** `Stage_03_BranchRide`. 2스테이지와 같이 씬 내부의 `Art_…`를 수정한다.

2·3스테이지의 씬 내부 UI에는 공통 프리팹 수정이 자동 전파되지 않는다. 같은 그림은 ArtistDrop의 동일 Sprite를 연결해 사용한다. 현재 씬에 별도 프리팹 Override가 있다면 그 설정이 우선하므로 화면이 다를 때 개발 담당자에게 확인을 요청한다.
