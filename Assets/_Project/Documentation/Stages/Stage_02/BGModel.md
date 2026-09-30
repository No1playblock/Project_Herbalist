> 2026-09-22: feat/BGModel 7246c48의 Assets/1.unity 배치를 반영했습니다. 실행 경로와 씬 GUID는 유지하며, 공동 출구 설정은 [3스테이지 전환 문서](../Stage_03/README.md)를 참조하세요.

# Stage 02 BGModel 실행 구성

현재 2스테이지는 `Assets/_Project/Scenes/Stages/Stage_02/Stage_02_BGModel.unity`입니다.
Stage_01_Exterior의 StageLevel.nextScenePath와 EditorBuildSettings가 이 씬을 가리킵니다.
Stage_02_Interior는 이전 프로토타입으로 보존하며 현재 빌드 목록에서는 제외합니다.
StageLevelLayout은 이전 프로토타입 생성기이므로 새 배경 씬을 생성하거나 덮어쓰는 용도로 사용하지 않습니다.

## 실행 구성
- StageTwo_Runtime: 기존 Player, PlayerSpawnLayout, PlayScreen, EventSystem, GameOverlayUI를 옮겨 내부 참조를 보존했습니다. StageThree_Exit와 StageTwo_EntranceGate도 새 맵 좌표로 이동했습니다.
- 스폰: (170.4447, 0.1928, -4.3508), (172.4447, 0.1928, -4.3508), Y 회전 180도. 나무 사이 북쪽 틈에서 내부 중앙으로 진입합니다.
- 기존 메인 카메라는 비활성화하며 플레이어 카메라가 사용됩니다.
- 기존 Fusion 씬 전환의 슬롯·복용 능력 유지와 Host 스폰 처리를 사용합니다.
- Ground 1의 실제 메시를 바닥 충돌로 사용합니다. 추락 시 능력 오브젝트를 초기화하지 않습니다.
- Root 계열 메시 40개에는 MeshCollider, LeafInstallTarget, SapReceiver, SapSource를 연결했습니다. 설치 대상과 수액 수신자의 ID는 201부터 쌍으로 부여합니다.
- Lattice 변형 메시의 충돌 사본은 Art/Environments/Stage_02/CollisionMeshes에 저장했습니다. 이후 메시 형상을 편집하면 충돌 메시도 다시 베이크해야 합니다.
- StageThree_Exit는 (162.4447, 34.7, -16.3508)에 있습니다. 두 명이 영역에 함께 들어오면 기존 Host 전환 처리로 Stage_03_Prototype의 FromStage02 입구로 이동합니다.

## 플레이어별 입구 차단 (A)
Hierarchy의 StageTwo_EntranceGate를 편집합니다.
- 위치 (171.4447, 25.1628, -6.3508), Y 회전 180도. 로컬 +Z가 내부 방향입니다.
- EntryTrigger: 크기 (22, 50, 4), Is Trigger 활성화.
- ReturnBarrier: 크기 (22, 50, 0.3), 일반 BoxCollider.
- 두 오브젝트 모두 Renderer가 없어 게임에서는 보이지 않습니다.
- 입구 통과 전에는 해당 캐릭터와 ReturnBarrier 사이의 충돌만 무시합니다.
- 캐릭터 몸체가 내부 방향으로 차단벽을 완전히 넘으면 그 캐릭터의 통과 비트를 저장하고 이후 되돌아오는 이동을 차단합니다. 다른 플레이어의 진입에는 영향을 주지 않습니다.
- 판정은 CharacterController 이동 구간을 검사하므로 빠르게 트리거를 지나쳐도 작동합니다. 물리 OnTriggerEnter 콜백에 의존하지 않습니다.
- MotorState와 NetworkPlayer.EnteredGateMask가 통과 상태를 보존합니다. Host가 확정하고 로컬 예측 및 재시뮬레이션에서 같은 판정을 수행합니다.
- 일반 Teleport는 통과 상태를 유지합니다. 새 씬의 새 플레이어는 초기 상태로 시작합니다.
- 여러 입구를 추가할 때 같은 씬에서는 Gate Index(0~31)를 중복하지 마세요. 트리거와 차단벽의 방향을 맞추고 깊이는 캐릭터 반경보다 충분히 크게 유지하세요. 벽의 끝은 지형과 겹쳐 우회할 틈을 없애야 합니다.

## 이번 검증
- StageEntranceGateChecks.Run(): 회전된 입구, 두 캐릭터별 독립 충돌, 역방향 차단, 빠른 통과, 영역 밖 배제, 예측 상태 복원, 텔레포트 상태 유지 통과.
- 실제 BGModel 씬에서 오프라인 캐릭터 이동: 입구 통과 비트 설정 및 역방향 이동 차단 확인.
- PlayerInteractionChecks.Run(): 나뭇잎 위로 통과/착지, 예측 복원, 짧은 점프, 회수, 조준 레이, 머리 방향, Q/R 도움말 회귀 검사 통과.
- Play 모드 Console 오류·경고 없음.
- 이번 변경 후 실제 Photon Host/Client 두 프로세스 접속 검증과 새 맵 전체 퍼즐 수동 완주는 아직 수행하지 않았습니다.

## 최신 배치 반영 검증
- 원본 브랜치와 이전 반영본의 차이는 Assets/1.unity 하나입니다. 새 패키지나 모델 변경은 없으며, 씬이 참조하는 모든 에셋이 현재 프로젝트에서 해결됩니다.
- 기존 나무 26개의 상호작용 ID를 유지하고 추가 뿌리 14개에 새 ID를 부여했습니다. 총 40개 ID가 중복 없이 연결되어 있습니다.
- Lattice 변형 뿌리 28개의 충돌 메시를 현재 형상으로 베이크했습니다.
- 실제 새 맵에서 입구 진입/복귀 차단, 가지에서 출구로 보행, 공동 출구 인원 0→1→2→1 갱신 통과.
- StageEntranceGateChecks.Run()의 두 캐릭터 독립 충돌과 예측 복원 검사 통과. Console 오류 없음.
- 실제 Photon 2인 접속 전환과 전체 레벨 수동 완주는 이번 변경에서 재실행하지 않았습니다.

## RootNormalKit 및 로비 직접 시작
- RootNormalKit 4개 묶음의 하위 뿌리 28개 모두 고정/발판 나뭇잎 설치와 수액 부착·추출을 허용합니다.
- 28개 각각 실제 MeshCollider 표면 레이 적중, SapReceiver.TryPlacement, SapSource.TryClosestSurface를 확인했습니다. 설치 대상 ID는 씬 전체에서 중복이 없습니다.
- SO_LobbySettings.playScenePath는 Stage_02_BGModel입니다. 방에서 캐릭터 선택 후 시작하면 이 씬으로 이동합니다.
- Grant Prototype Abilities On Direct Start를 활성화하여 로비에서 바로 시작하는 첫 스테이지에서만 기존 프리팹 슬롯 설정(두영 수액/소담 나뭇잎)을 적용합니다.
- 스테이지 전환으로 저장된 능력이 있으면 이 직접 시작 예외를 적용하지 않습니다. 3스테이지에서도 보유 능력을 유지합니다.
- 정상 1스테이지 진행으로 복원하려면 로비의 Play Scene Path를 Stage_01_Exterior로 되돌리고 직접 시작 테스트 능력 지급 옵션을 끕니다.
