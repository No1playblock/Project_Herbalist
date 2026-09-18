# 프로젝트 폴더 및 네이밍 규칙

## 배치 기준
자체 제작 파일은 `Assets/_Project` 아래에서 파일 종류별로 관리한다.
기능별 분류는 각 종류 폴더의 하위에 둔다. 캐릭터 영문명은 `Sodam`, `Duyeong`으로 통일한다.

```text
_Project/
  Art/
    Characters/Sodam/{Models,Textures}
    Characters/Duyeong/
    Environments/{Stage_01,Stage_02}/{Models,Textures}
    UI/{Textures,Shaders}
    VFX/
    Materials/{Characters,Environments,Abilities,Items,UI,Development}
  Audio/{BGM,SFX,Voice}
  Animations/{Characters,Objects}
  Prefabs/{Characters,Enemies,InteractiveObjects,UI,VFX,Networking}
  Scenes/
    01_Title/MainMenu.unity
    Stages/Stage_01/Stage_01_Exterior.unity
    Stages/Stage_02/Stage_02_Interior.unity
    Development/{Sandbox_Abilities,Sandbox_Rendering}.unity
  Scripts/
    Core/
    Gameplay/{Character,Interaction,Item,Abilities,Crafting,Stage}
    UI/
    Camera/
    Networking/
    Development/
    Editor/
  Data/{Characters,Abilities,Items,Recipes,Stages,UI,Networking,Development}
  Fonts/
  Settings/{Input,Rendering}
  Tests/{Editor,Runtime}
  Documentation/
```

- `Art`에는 모델·텍스처·시각 효과 원본, `Prefabs`에는 배치 가능한 프리팹을 둔다.
- 독립 애니메이션과 Animator Controller는 `Animations`에 둔다. 모델 FBX 내부 클립은 분리하지 않는다.
- `Data`는 게임 설정/콘텐츠, `Settings`는 입력·렌더링 등 프로젝트 설정이다. `Data/ScriptableObjects`라는 중복 분류는 만들지 않는다.
- `Managers` 폴더 대신 담당 기능으로 분류한다. 세션은 `Networking`, 진행 규칙은 `Gameplay/Stage`에 둔다.
- 테스트용 MonoBehaviour는 `Tests/Runtime`, UnityEditor API를 사용하는 검증은 `Tests/Editor`에 둔다.
- 임시 솔로 능력 전환·메뉴는 `Scripts/Development`, 플레이용 테스트 씬은 `Scenes/Development`에 둔다.
- Boot/Main 씬은 실제 역할이 분리될 때 추가한다. 구조 정리를 위해 빈 씬을 만들지 않는다.
- `Assets/Plugins`, `Assets/ThirdParty`는 외부 코드/리소스 영역이다. Photon과 TextMesh Pro는 패키지 내부 경로 의존성을 보존하기 위해 기존 루트 경로를 유지한다.
- Unity 기본 템플릿 안내 파일은 `Assets/ThirdParty/UnityTemplate`에 보관한다.

## 이름 규칙

| 대상 | 규칙 | 예시 |
|---|---|---|
| 일반 폴더 | PascalCase | InteractiveObjects |
| 스테이지 | 두 자리 번호 | Stage_01 |
| 씬 | 역할이 드러나는 이름 | Stage_01_Exterior, Sandbox_Abilities |
| C# 파일/타입 | PascalCase, 대표 타입과 파일명 일치 | LeafThrowAbility.cs |
| 메서드/프로퍼티/상수 | PascalCase | TryExtract, IsBound, MaxPlayerCount |
| private 필드 | _camelCase | _moveSpeed |
| 인자/지역 변수 | camelCase | targetPosition |
| 인터페이스 | I 접두사 | IPlayerMotor |
| 프리팹 | PF_ | PF_NetworkPlayer |
| 게임 데이터 에셋 | SO_ | SO_SapAbilitySettings |
| 머티리얼 | MAT_ | MAT_Sap |
| 텍스처 | T_대상_용도 | T_HomeBackground |
| 독립 애니메이션 | AN_ | AN_SodamIdlePose |
| Animator Controller | AC_ | AC_SodamLocomotion |
| Hierarchy | 역할 중심 이름 | OptionsPanel, CameraPivot |

폰트, 입력 액션 참조, 렌더 파이프라인 에셋에는 게임 데이터용 SO_ 접두사를 강제하지 않는다.
외부 에셋 내부 이름과 모델의 본/노드 이름은 원본 호환성을 위해 유지한다.

## 직렬화 호환성
이번 정리는 폴더·파일 및 UI 타입 이름을 대상으로 한다. 기존 Inspector 필드명, 입력 액션명,
Animator 파라미터, PlayerPrefs 키와 네트워크 데이터 식별자는 동작 호환성을 위해 유지한다.
새 코드부터 위 코드 네이밍 규칙을 적용한다. 기존 직렬화 필드를 변경할 때는
FormerlySerializedAs와 에디터 FindProperty/리플렉션 호출, 프리팹 오버라이드를 함께 이전한다.
UI 타입 변경에는 MovedFrom을 명시하고 스크립트 GUID를 유지했다.

## 이동 및 참조 규칙
- Unity AssetDatabase로 이동/이름 변경하여 `.meta` GUID를 보존한다.
- 코드에 경로가 필요한 에디터 도구, 빌드 씬 목록, 메뉴/스테이지 전환의 설정 경로를 함께 갱신한다.
- 런타임은 Inspector의 에셋 참조/설정 경로를 사용한다. 런타임에 UI를 생성하지 않는다.
- 프로젝트 구조 정리와 게임플레이 구조 변경을 섞지 않는다.
- 이전/현재 에셋 경로 대응은 `AssetPathMigration.tsv`를 참고한다.

## 현재 실행 씬
- 메인 메뉴: `Assets/_Project/Scenes/01_Title/MainMenu.unity`
- 1스테이지: `Assets/_Project/Scenes/Stages/Stage_01/Stage_01_Exterior.unity`
- 2스테이지: `Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Interior.unity`
- 솔로 능력 테스트: `Assets/_Project/Scenes/Development/Sandbox_Abilities.unity`

## 이번 이전 검증
- 이동한 249개 에셋의 GUID 보존 확인.
- 기존 씬·프리팹·설정 에셋의 외부 GUID 참조 집합 동일함을 확인.
- 프리팹 14개 및 빌드 씬 5개에서 Missing Script 0개.
- 메뉴·솔로·스테이지 전환의 직렬화 경로 및 빌드 씬 목록 갱신.
- SapHoseChecks 회귀 검사 통과.
- Windows Development 빌드 성공: 오류 0개, 경고 35개(API 사용 중단 및 메시 충돌 사전 베이크 등).
- 실제 Host/Client 모두 캐릭터 선택·씬 시작·역할 보존·공통 일시정지·소유자 재개 검사 통과.
- 빌드/로그는 추적하지 않는 Temp/StructureCheck에 저장.
