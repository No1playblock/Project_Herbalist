# 솔로 테스트 스테이지 바로 시작

스테이지 1·2·3 및 Sandbox_Abilities에서 오프라인 Play 중 숫자 키패드로 스테이지를 바로 시작한다.

| 기본 입력 | 대상 씬 |
| --- | --- |
| 키패드 1 | Stage_01_ModelingReference_CharacterBranch |
| 키패드 2 | Stage_02_Mazes |
| 키패드 3 | Stage_03_BranchRide |

키보드 상단 숫자열은 해당하지 않는다. 같은 스테이지의 번호를 누르면 다시 로드한다. 현재 진행/소지품/설치 오브젝트를 이어받지 않고 해당 씬에 작성된 시작 상태를 사용한다. 일시정지 중 사용해도 새 스테이지에서는 정지를 해제한다. 능력 자동 해금 등 기존 솔로 테스트 규칙은 변경하지 않는다.

각 씬의 `SoloStageShortcuts` 오브젝트에 `Scripts/Development/OfflineStageShortcuts.cs`를 배치했다. Inspector의 Shortcuts 배열에 InputAction 바인딩과 씬 경로를 저장하며, 키 판정이나 목적지 경로를 게임플레이 코드에 넣지 않았다. 테스트 기능을 제거하려면 이 오브젝트를 비활성화하거나 삭제한다. Photon 연결 중이거나 세션이 존재하면 입력을 거부한다.

검증(2026-11-03): Input System의 실제 키패드 입력 이벤트로 1→2→3→1 씬 전환과 같은 씬 재시작을 확인했다. 각 대상은 Build Settings에 활성 등록되어 있다. Play 검사 중 Console 오류/경고 0개. 실제 Photon 접속 상태에서의 키 입력 검사는 실행하지 않았다.
