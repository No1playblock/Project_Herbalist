# Project Herbalist — 폴더 및 네이밍 가이드

> 팀 공용 작업 규칙 · 2026-09-18 기준
>
> 새로 만드는 파일과 코드에 적용합니다. 기존 직렬화 필드·입력 식별자와 외부 에셋의 예외는 아래 호환성 규칙을 따릅니다.

## 1. 기본 원칙

- 자체 제작 파일은 `Assets/_Project`에 넣습니다.
- 파일은 **종류 → 기능 또는 대상** 순서로 분류합니다.
- 폴더·파일·코드 식별자는 영문을 사용하고, 사용자에게 보이는 문구와 설명은 한글을 사용할 수 있습니다.
- 일반 단어는 PascalCase로 작성합니다. 예: `InteractiveObjects`, `LeafThrowAbility`.
- 이름은 대상과 역할을 드러내도록 작성합니다. `New`, `Temp2`, `FinalFinal`, 자동 생성된 `(1)`을 최종 이름으로 사용하지 않습니다.
- 캐릭터 이름은 **Sodam / Duyeong**, 능력 이름은 **Leaf / Sap**으로 통일합니다.
- 스테이지 번호는 **Stage_01 / Stage_02**처럼 두 자리로 표기합니다.

## 2. 파일을 넣는 위치

아래 경로는 `Assets/_Project`를 기준으로 합니다.

| 파일 또는 역할 | 위치 | 설명 |
|---|---|---|
| 캐릭터 모델·텍스처 | Art/Characters/캐릭터명 | Models, Textures로 구분 |
| 환경 모델·텍스처 | Art/Environments/Stage_번호 | 스테이지별 원본 리소스 |
| UI 이미지·셰이더 | Art/UI | Textures, Shaders로 구분 |
| VFX 원본 리소스 | Art/VFX | 배치용 프리팹은 Prefabs/VFX |
| 머티리얼 | Art/Materials/용도 | Characters, Environments, Abilities, UI 등 |
| 음원 | Audio/BGM, SFX, Voice | 사용 목적별 분류 |
| 애니메이션·Animator Controller | Animations/Characters 또는 Objects | 캐릭터별 하위 폴더 사용 |
| 배치용 프리팹 | Prefabs/용도 | Characters, InteractiveObjects, UI, VFX, Networking 등 |
| 씬 | Scenes/역할 | 타이틀, 스테이지, 개발용 씬 분리 |
| 게임플레이 코드 | Scripts/Gameplay/기능 | Character, Interaction, Item, Abilities, Crafting, Stage |
| 카메라·화면 분할 코드 | Scripts/Camera | 시점과 화면 표시 제어 |
| UI 코드 | Scripts/UI | 화면·패널·HUD |
| 네트워크 코드 | Scripts/Networking | Fusion 연결·스폰·동기화 |
| 공통 실행 상태 | Scripts/Core | 여러 기능이 함께 사용하는 최소 기능 |
| 임시 테스트 기능 | Scripts/Development | 솔로 능력 전환 등 |
| 에디터 작성 도구 | Scripts/Editor | UnityEditor 전용 코드 |
| 게임 데이터·조정 값 | Data/용도 | Characters, Abilities, Items, Recipes, Stages, UI 등 |
| 입력·렌더링 설정 | Settings/Input 또는 Rendering | 프로젝트 공통 설정 |
| 폰트와 라이선스 | Fonts/폰트명 | 라이선스 파일도 함께 보관 |
| 검증 코드 | Tests/Editor 또는 Runtime | 실행 환경에 따라 분리 |
| 기획·개발 문서 | Documentation | 기능별 설명과 작업 규칙 |

`Managers` 폴더는 만들지 않습니다. 방 관리는 Networking, 스테이지 진행은 Gameplay/Stage처럼 실제 역할에 배치합니다.
`Data/ScriptableObjects`도 만들지 않습니다. ScriptableObject는 저장 형식이므로 게임에서의 용도에 따라 분류합니다.
Boot와 Main 씬은 실제 기능이 분리될 때 추가합니다.

## 3. 에셋 이름 규칙

| 종류 | 형식 | 예시 |
|---|---|---|
| 프리팹 | PF_대상 | PF_NetworkPlayer, PF_MainMenuScreen |
| 게임 데이터 에셋 | SO_대상 | SO_SapAbilitySettings, SO_LeafPotion |
| 머티리얼 | MAT_대상 | MAT_Sap, MAT_PauseBlur |
| 텍스처 | T_대상_용도 | T_RootBark_BaseColor, T_RootBark_Normal |
| UI 이미지 | T_역할 | T_HomeBackground |
| 독립 애니메이션 클립 | AN_캐릭터_동작 | AN_Sodam_Walk |
| Animator Controller | AC_대상 | AC_SodamLocomotion |
| 스테이지 씬 | Stage_번호_역할 | Stage_01_Exterior, Stage_02_Interior |
| 개발용 씬 | Sandbox_목적 | Sandbox_Abilities |
| 그 외 씬 | 역할 | MainMenu |

- 표의 이름은 확장자를 제외한 이름입니다.
- 독립 애니메이션의 동작 구분에는 `_`를 사용할 수 있습니다. 현재 `AN_SodamIdlePose`처럼 PascalCase로 작성된 이름도 허용합니다.
- 폰트·입력 액션 참조·렌더 파이프라인 에셋에는 `SO_`를 강제하지 않습니다.
- 입력 액션 참조는 `Move`, `Jump`, `Use`처럼 실제 액션의 의미를 드러냅니다.
- FBX에 포함된 클립, 본, 노드 이름은 원본과 연결된 식별자이므로 임의로 바꾸지 않습니다.
- 접두사는 파일 종류를 구분하기 위한 것입니다. Hierarchy의 오브젝트 이름에까지 붙일 필요는 없습니다.

## 4. C# 이름 규칙

| 대상 | 규칙 | 예시 |
|---|---|---|
| 파일·클래스·구조체·enum 타입 | PascalCase | LeafThrowAbility.cs, LeafMode |
| 인터페이스 | I + PascalCase | IPlayerMotor |
| 메서드 | PascalCase | TryExtract, BeginReturn |
| 프로퍼티 | PascalCase | IsBound, ActiveCount |
| enum 값 | PascalCase | Off, Pin, Platform |
| 상수 | PascalCase | MaxPlayerCount |
| private 필드 | _camelCase | _moveSpeed, _settings |
| 매개변수·지역 변수 | camelCase | targetPosition, source |

- MonoBehaviour와 ScriptableObject는 파일명과 대표 클래스명을 일치시킵니다.
- 조정 값은 설정 에셋 또는 직렬화 필드로 관리합니다. 코드 안에 게임 밸런스 수치나 키 입력을 흩어 놓지 않습니다.
- 새 내부 상태는 private 필드로, 외부에 필요한 읽기 값은 프로퍼티로 노출하는 방식을 우선합니다.
- 기존 public 직렬화 필드의 camelCase 이름은 호환성 예외로 유지합니다.
- 기존 네임스페이스는 타입 호환성을 위해 유지합니다. 새 타입은 관련 기능의 `Herbalist.*` 네임스페이스를 따릅니다.

## 5. 역할을 드러내는 클래스 이름

| 역할 | 권장 이름 | 예시 |
|---|---|---|
| 조정 가능한 수치·규칙 | Settings 또는 기존 Tuning | SapAbilitySettings, PlayerTuning |
| 아이템 등의 고정 정의 | Definition | StageItemDefinition |
| 입력 수집 | InputReader | PlayerInputReader |
| 이동 처리 | Motor | CharacterControllerMotor |
| 외형·애니메이션·효과 표시 | Presentation | PlayerCharacterPresentation |
| 전체 UI 화면 | Screen | MainMenuScreen |
| UI의 부분 영역 | Panel | OptionsPanel |
| 플레이 중 정보 표시 | Hud | PlayerHud, GameOverlayHud |
| 네트워크 연동 | Network + 대상 | NetworkPlayer, NetworkSapDeposit |
| 에디터 생성 도구 | Builder 또는 기존 Setup | GameUiBuilder, LeafAbilitySetup |
| 검증 코드 | Checks 또는 기존 SmokeTest | SapHoseChecks, NetworkMovementSmokeTest |

역할이 불분명한 `GameManager`, `CommonManager`, `UtilityManager` 같은 이름을 새로 만들지 않습니다.
`Controller`는 실제로 여러 구성 요소의 동작을 조정하는 클래스에 사용합니다.
약어는 새 UI 타입에서 `Ui`, `Hud`처럼 표기합니다. 폴더 이름 `UI`, `VFX`, 외부 API 이름은 그대로 사용합니다.

## 6. Hierarchy 이름과 UI

오브젝트의 이름만 보고 역할을 알 수 있게 작성합니다.

```text
MainMenuCanvas
├─ HomePanel
│  ├─ CreateRoomButton
│  ├─ JoinRoomButton
│  └─ OptionsButton
├─ LobbyPanel
├─ OptionsPanel
└─ LoadingPanel
```

- 위 트리는 신규 UI 작성 시 사용할 예시입니다. 기존 Hierarchy 전체를 이 이름으로 변경했다는 의미는 아닙니다.
- UI는 UGUI로 작성하고, 플레이 전에 씬 또는 프리팹 Hierarchy에 배치합니다.
- 실행 중에는 준비된 UI를 `SetActive`로 표시·숨김 처리합니다. 런타임 UI 생성 코드는 작성하지 않습니다.
- 오브젝트 이름이 코드 검색이나 애니메이션 경로에 사용되고 있다면 이름 변경 시 해당 참조도 수정합니다.

## 7. 예외 및 안전한 이름 변경

현재 폴더 정리에서는 기존 Inspector 필드명, 입력 액션명, Animator 파라미터,
PlayerPrefs 키, 네트워크 식별자를 유지했습니다. 새 네이밍 규칙을 적용한다는 이유만으로 기존 식별자를 일괄 변경하지 않습니다.

1. Unity Project 창 또는 AssetDatabase로 에셋을 이동·변경합니다.
2. `.meta`를 함께 유지하여 기존 GUID 참조를 보존합니다.
3. 씬 파일명이 바뀌면 Build Settings, 메뉴 진입, 스테이지 전환, 테스트의 경로·이름 비교를 함께 수정합니다.
4. 직렬화 필드명이 바뀌면 `FormerlySerializedAs` 적용 여부와 Inspector 값, 프리팹 오버라이드, `FindProperty` 및 리플렉션 호출을 확인합니다.
5. 타입명이 바뀌면 파일명을 맞추고, 필요한 경우 `MovedFrom`을 적용합니다.
6. 컴파일, Missing Script, 프리팹 참조와 관련 기능을 확인한 뒤 커밋합니다.

외부 파일은 `Assets/Plugins` 또는 `Assets/ThirdParty`에 관리합니다.
Photon과 TextMesh Pro는 패키지 내부 경로 의존성을 보존하기 위해 현재 루트 위치를 유지합니다.
외부 패키지 파일은 자체 네이밍 규칙에 맞추려고 일괄 변경하지 않습니다.

## 8. 현재 사용하는 씬

| 용도 | 경로 |
|---|---|
| 메인 메뉴 | Assets/_Project/Scenes/01_Title/MainMenu.unity |
| 1스테이지 | Assets/_Project/Scenes/Stages/Stage_01/Stage_01_Exterior.unity |
| 2스테이지 | Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Interior.unity |
| 솔로 능력 테스트 | Assets/_Project/Scenes/Development/Sandbox_Abilities.unity |

## 9. 작업 완료 전 확인

- 파일이 용도에 맞는 폴더에 있는가?
- 이름만 보고 대상과 역할을 알 수 있는가?
- 캐릭터·능력·스테이지 표기가 통일되어 있는가?
- 에셋 종류에 맞는 접두사를 사용했는가?
- `.meta`와 기존 참조가 유지되는가?
- 테스트용 파일과 실제 게임 파일이 구분되어 있는가?
- 변경한 기능을 실행하여 확인했는가?
