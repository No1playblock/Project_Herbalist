# Stage 2 / Stage 3 환경 모델링 통합 (2026-11-03)

출처는 `origin/feat-environmentModeling`, `b797e0d9`이다. 관련 커밋은 `b54f703b`, `42b5fe78`, `30806f7e`, `1b7f3ca7`, `6bdb9ad5`, `19b67d5c`, `10b6a676`, `b797e0d9`이다.

## 비교용 원본 씬

- `Stage_02_EnvironmentModeling_Reference`: 원본 `Stage_02/00_Stage_2.unity` 복제.
- `Stage_03_EnvironmentModeling_Reference`: 원본 `Stage_03/03_Stage.unity` 복제.
- 에셋 참조 GUID만 격리된 사본에 연결했다. 두 씬은 빌드 경로나 스테이지 진행에 등록하지 않는다.
- 원본 브랜치의 `Stage_02_Mazes` / `Stage_03_BranchRide`는 현재 게임플레이보다 이전 구성이라 가져오지 않았다.

## 에셋 및 패키지

`Art/Environments/EnvironmentModelingImport_20261103/ImportManifest.json`에 원본 경로, 원본/사본 GUID, Git blob과 재사용 여부를 기록했다. 119개 에셋은 새 GUID와 원본 임포트 설정으로 격리했으며, 동일한 65개는 재사용했다. 소스 FBX의 첫 메시를 임의로 연결하거나 공유 모델 임포트 배율을 변경하지 않는다.

원본 조명 의존성 `com.cqf.urpvolumetricfog`는 Package Manager API로 `354ebc5b6ed3c2c40dc9797c813b56d5da213c4e`에 고정해 설치했다. PC_Renderer에는 해당 renderer feature만 추가하고 기존 SSAO/렌더러 설정은 유지했다. 기존 Stage 1의 환경/플레이어 설정은 수정하지 않았다.

Lattice로 변형된 장식 메시 4개는 `BakedMeshes`에 저장하고, 복제한 장식에서 편집용 Lattice 컴포넌트를 제거했다. 비교 씬의 원본 Lattice는 그대로다.

## Stage 2 적용

- 적용 씬: `Stage_02_Mazes`.
- 기존 `Stage2_EnvironmentModeling`의 렌더러는 숨기고 기존 콜라이더와 참조를 보존했다. 새 외형은 그 아래 `EnvironmentModeling_20261103`에 배치했다.
- 원본의 나무 군집, 배경 나무, 버섯과 조명을 기존 스테이지의 `(-85,0,-85)` / 2.7 배율 기준에 맞춰 옮겼다.
- 원본과 계층·메시 이름·정점 수·바운드가 일치하는 기어/로프/기구 102개의 메시와 재질을 교체했다. 별도 제작된 Platform003/004는 현재 이동 플랫폼 형상과 콜라이더를 유지했다.
- 안개용 Quad 2개와 GlobalVolume는 시야를 가리므로 플레이 씬에서 기본 비활성화했다. 비교 씬에는 원본 설정을 그대로 보존했다.
- 미로의 벽/Plane/WaterWalls, 물 계산, 이동 속도, 누수/보충/재시도, 두 하강 플랫폼 및 UI/씬 전환 연결은 유지했다.

## Stage 3 적용

- 적용 씬: `Stage_03_BranchRide`.
- 트랙 39개, 분기점 피벗 10개, 두 낙하, 노드와 출구, 과녁/돌/운송 조건을 유지했다.
- 원본의 굵은 `1027_Branch_Fbx`를 65개 가지 외형에 적용했다. 각 외형은 기존 메시 바운드와 축에 맞추고 원래 렌더러만 숨겼으며, 기존 게임플레이 콜라이더는 유지했다. 가느다란 가지 군집 `S3_B`를 이동 트랙 전체에 강제로 끼워 넣지 않는다.
- 부적의 Paper 외형은 원본 `Fabric_unwarp` 메시와 원본 Flag/Rope 재질로 교체했다. 탑승/하차 위치와 충돌체는 유지했다.
- 원본 동굴/뿌리 모델을 기존 +X 진행 경로에 맞게 회전하고 네 구간으로 배치했다. 원본 대비 2.7 배율로 캐릭터·카메라 공간을 확보했고, 각 구간에 편집 가능한 보조 조명을 배치했다. 새 배경의 콜라이더는 비활성화했다.
- 아직 전용 모델이 없는 과녁/돌은 기존 표현과 판정을 유지한다. 원본 모델링의 전체 배치로 트랙을 다시 설계한 작업은 아니다.

## 편집 도구

`EnvironmentModelingSceneSync`의 `Herbalist/Stages/Apply Environment Modeling Stage 2/3` 메뉴가 적용 절차를 기록한다. 이미 적용된 씬에는 중복 생성하지 않는다. 세부 배치와 조명은 씬의 새 루트에서 수정할 수 있다.

## 검증

- Stage 2: 기존 113개 게임플레이 컴포넌트와 32개 충돌체의 동작 설정 보존. 기존 SapOrbitPresentation의 두 기본 필드가 저장된 것 외에는 변경하지 않았다.
- Stage 3: 기존 231개 게임플레이 컴포넌트와 90개 충돌체 보존. SapOrbitPresentation의 기본 필드 저장 외에는 동작 설정 변경이 없다.
- 자산 GUID 중복 0개, 격리한 사본 GUID 불일치 0개. 두 씬 스크립트 누락 0개. Stage 2의 FluidSurface/LeakStream은 원래 런타임에 메시를 만드는 구성이다.
- 유체 이동/보존/반전/누수/보충, 좁은 통로, 최근접 인덱스 회귀 검사 통과.
- 실제 미로 경로: Maze1 1,898점/926 Tick, Maze2 1,491점/864 Tick, Maze3 1,572점/636 Tick, 도착·물량 보존·벽 차단 검사 통과.
- 가지 타기 편집 검사 및 Play 시나리오 검사 통과: 한 명 대기, 두 명 탑승, A 경로 추락/초기화, 수액 재무장, 잎 정지/회수, 전체 경로와 두 낙하/완주/하차, 돌 충돌/잎 제거, 일시정지.
- 렌더링 화면 검수와 스크립트 컴파일 확인. Play 검사 이후 스크립트 재로드에서 Fusion SDK의 `FusionPluginProjectSettings.ScriptsReloaded`가 `AssertException: attribute != null`을 1건 기록했다(`Fusion.Unity.cs:890`). 이번 외형 작업에서 Photon SDK 파일은 수정하지 않았으며, 이 에디터 콜백 예외는 미해결로 남긴다. 실제 두 프로세스 Host/Client 접속 검증은 이번 작업에서 수행하지 않았다.

변경 전 두 메인 씬, 상세 비교·검사 결과와 화면은 `Temp/Stage23EnvironmentSync`에 보관했다.
