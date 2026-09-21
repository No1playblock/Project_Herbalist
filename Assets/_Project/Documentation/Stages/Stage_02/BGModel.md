# Stage 02 BGModel 실행 구성

현재 2스테이지는 `Assets/_Project/Scenes/Stages/Stage_02/Stage_02_BGModel.unity`입니다.
Stage_01_Exterior의 StageLevel.nextScenePath와 EditorBuildSettings가 이 씬을 가리킵니다.
Stage_02_Interior는 이전 프로토타입으로 보존하며 현재 빌드 목록에서는 제외합니다.
StageLevelLayout은 이전 프로토타입 생성기이므로 새 배경 씬을 생성하거나 덮어쓰는 용도로 사용하지 않습니다.

## 실행 구성
- StageTwo_Runtime: 기존 Player, PlayerSpawnLayout, PlayScreen, EventSystem, GameOverlayUI, StageTwoGoal과 StageTwoHUD를 함께 옮겨 내부 참조를 보존했습니다.
- 스폰: (8, -0.65, 18), (10, -0.65, 18), Y 회전 180도. 나무 사이 북쪽 틈에서 내부 중앙으로 진입합니다.
- 기존 메인 카메라는 비활성화하며 플레이어 카메라가 사용됩니다.
- 기존 Fusion 씬 전환의 슬롯·복용 능력 유지와 Host 스폰 처리를 사용합니다.
- Ground 1의 실제 메시를 바닥 충돌로 사용합니다. 추락 시 능력 오브젝트를 초기화하지 않습니다.
- Root 계열 메시에는 MeshCollider, LeafInstallTarget, SapReceiver, SapSource를 연결했습니다. 설치 대상과 수액 수신자의 ID는 201부터 쌍으로 부여합니다.
- Lattice 변형 메시의 충돌 사본은 Art/Environments/Stage_02/CollisionMeshes에 저장했습니다. 이후 메시 형상을 편집하면 충돌 메시도 다시 베이크해야 합니다.
- 상단 도착 영역은 RootEndpoint 위 (0, 33.5, 6), 크기 (5, 4, 4)입니다. Host가 두 명의 도착을 확인하는 기존 목표 처리를 사용합니다.

## 플레이어별 입구 차단 (A)
Hierarchy의 StageTwo_EntranceGate를 편집합니다.
- 위치 (9, 24.3, 16), Y 회전 180도. 로컬 +Z가 내부 방향입니다.
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
