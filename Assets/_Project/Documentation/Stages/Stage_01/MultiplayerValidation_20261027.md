# 멀티 충돌·스테이지 전환 검증 — 2026-10-27

## 수정

- `ProjectSettings/DynamicsManager.asset`: Player(8) ↔ StageTreeCamera(22)의 충돌을 활성화했다. 카메라용 레이어, 카메라 마스크와 기존 나무 메시 콜라이더는 유지한다. 멀티 캡슐은 Player 레이어, 오프라인 프리팹은 Ignore Raycast 레이어여서 이전에는 솔로만 나무와 충돌했다.
- `Stage_02_Mazes.unity`: PlayerSpawnLayout.playerPrefab이 PF_Stage02MazesNetworkPlayer의 존재하지 않는 NetworkObject fileID 5399665031343075763을 참조했다. 실제 variant NetworkObject fileID 8100419551143118037로 복구했다. 수정 전 실제 Host가 1→2 씬 로드 후 SpawnPlayers에서 ArgumentNullException(prefab)을 발생시키며 Loading에 남는 것을 재현했다.

## 검증 결과

- 실제 크기 2.7의 Player 캡슐로 Fusion RestoreState처럼 매 이동마다 컨트롤러를 껐다 켜며 148개 지점 착지 통과: 이동용 콜라이더 146, 나무 1, Terrain 1. 위에서 밟을 면이 없는 콜라이더 8개는 착지 대상에서 제외했다.
- 나무 상부 (352.17, 177.54, 256.50)에서 동일 캡슐 비교: 이전 충돌 무시 상태는 y174.44까지 관통, 수정 상태는 y177.66에 착지. 단순 레이캐스트가 아닌 CharacterController.Move로 재현했다.
- 별도 Windows Development Host + Editor Client의 실제 Photon 연결: 두 플레이어 나무 착지, 천 3개 Fusion 부착 확인과 한 천에 두 명 동시 잡기/놓기, 점프대 6개 네트워크/포트/소켓 참조 확인과 한 점프대의 잎 설치·두 명 지지·주입 후 동시 상승 발사 통과.
- 현재 MainMenu의 역할 선택과 실제 스테이지 로더 사용. 1→2 전환, 역할/능력 유지, 두 역할별 뗏목 진입 확인.
- 3개 미로 각각 Client의 가상 키보드 R → 실제 Input System → Fusion 입력 → Host 제어 진입, 가상 마우스의 좌클릭 목표 이동, R 종료 통과. 각 완료 후 같은 뗏목의 다음 구간 하강과 2→3 전환, 두 능력 유지 통과. 양쪽 ALL PASS 및 Client 콘솔 오류 0건.
- `MazePassageChecks.Run()`: 폭 0.03 및 0.003 틈 연결과 점 이동, 닫힌 벽/표본보다 얇은 벽 차단, authored 좌표 보존 통과.
- `MazePointerViewChecks.Run()`: 실제 세 미로 경로 연결, 시작/도착 변경, 클릭 좌표, 카메라 프레이밍, 클릭 유지 이동/해제 정지, R 종료, 잠금 및 HUD 복원 통과.
- `OneWayLandingChecks.Run()`: 크기 1 및 2.7, 평면/경사면의 착지 30회와 아래에서 통과 후 재착지 6회 통과.
- 저장된 CharacterBranch, Stage_02_Mazes, Stage_03_Altar, Stage_03_BranchRide의 모든 PlayerSpawnLayout 프리팹 참조가 유효한 것을 확인했다.

## 검사 범위와 재실행

네트워크 검증은 제작 완료 약제를 테스트 상태로 준비한 뒤 일반 Drink 명령으로 능력을 얻는다. 이동 지점 접근은 Host의 TeleportAuthoritatively로 단축한다. 미로의 전체 수집·누수 퍼즐을 사람이 끝까지 플레이한 검사가 아니다. 각 미로의 원격 조작 검증 뒤 Solved 상태를 주입해 뗏목/씬 전환을 분리 검증한다. 천 전체의 스윙 경로나 모든 점프대에서의 전체 등반 동선은 수동 완주하지 않았다. 숨김 자동화 Client에서만 GameplayCursor를 비활성화해 OS 포커스 제한을 우회하며 게임의 포커스 정책은 바꾸지 않는다.

검증 코드:

- `Assets/_Project/Tests/Editor/MultiplayerCollisionChecks.cs` — Stage 1 Play에서 `Run()`.
- `Assets/_Project/Tests/Runtime/Networking/MultiplayerTraversalCheck.cs` — Editor/Development에서만 명시적 실행. 일반 플레이에서는 실행하지 않는다.
- MainMenu Play에서 `MultiplayerTraversalCheck.Run(host, room, absoluteCheckpointFolder)` 또는 빌드 인자 `-herbalist-traversal-check host|client room absoluteCheckpointFolder`.
- Development 빌드는 EditorBuildSettings의 활성 씬 목록/순서를 전부 그대로 포함해야 한다. Fusion은 씬의 Build Index를 전송한다.
- 마지막 네트워크 증거는 로컬 `Temp/TraversalRegression/Run4/host-result.txt`, `client-result.txt`(둘 다 PASS), `host.log` 및 에디터 콘솔에 있다. Temp/Builds 출력은 재생성 가능하며 버전 관리 대상이 아니다.