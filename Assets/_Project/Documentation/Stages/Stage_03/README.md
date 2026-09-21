# 2 → 3스테이지 공동 출구

## 배치와 동작
- 출발 씬: Assets/_Project/Scenes/Stages/Stage_02/Stage_02_BGModel.unity
- Hierarchy: StageThree_Exit. 위치 (0, 33.6, 6), Y 회전 90도.
- 상단 RootEndpoint의 열린 구간에 출구 발판과 문틀이 있습니다. 청록색 바닥 안쪽이 공동 진입 영역입니다.
- BothPlayersTransitionVolume은 두 명이 동시에 내부에 있을 때만 이동합니다.
- 먼저 도착하면 기존 UGUI HUD에 '다른 플레이어를 기다리는 중 · 1/2'를 표시합니다. 밖으로 나가면 인원을 다시 계산합니다.
- Host가 인원과 전환을 확정합니다. 중복 요청은 출구 상태와 기존 세션 전환 가드로 차단합니다.
- 별도의 런타임 UI 생성 없이 기존 GameOverlayHud를 사용합니다.
- 기존 BGModel의 StageTwoGoal/StageTwoHUD는 이 출구로 대체했습니다. 이전 Stage_02_Interior 프로토타입의 목표는 보존합니다.

## 다음 스테이지
- 씬: Assets/_Project/Scenes/Stages/Stage_03/Stage_03_Prototype.unity
- 현재는 바닥, 경계, 조명, 플레이어/HUD를 갖춘 도착 테스트 공간입니다. 3스테이지 기믹은 아직 없습니다.
- PlayerSpawnLayout.EntryId = FromStage02
- 스폰: 슬롯 0 (-1.25, 0.05, -8), 슬롯 1 (1.25, 0.05, -8).
- Host의 기존 상태 저장/복원 경로를 통해 선택한 캐릭터와 실제 복용한 능력을 유지합니다.
- StageLevel.requireEarnedAbilities = true. 멀티 진입 시 캐릭터별 테스트 능력으로 덮어쓰지 않습니다.
- 배치한 나뭇잎/수액 오브젝트는 이전 씬과 함께 종료되며, 복용한 능력 종류를 유지합니다.
- 오프라인 직접 실행은 기존 능력 테스트 방식입니다. A 방식의 출구에는 두 슬롯이 필요하므로 혼자서는 전환되지 않습니다. 목적지 씬을 직접 열어 오프라인 테스트할 수 있습니다.

## 재사용과 편집
- StageArrivalZone: BoxCollider, 인원수, 플레이어 판정 기준점 설정. 같은 슬롯은 한 번만 집계.
- CooperativeStageExit: 영역, StageLevel, 목적지 입구 ID, 안내 문구 연결. Fusion NetworkObject와 함께 씬에 배치.
- StageLevel.nextScenePath: 목적지 경로. 씬을 Build Settings에도 등록.
- PlayerSpawnLayout.EntryId: 해당 목적지의 입구 이름. 출구의 Destination Entry Id와 일치시킬 것.
- 입구 ID가 없는 기존 전환은 이전처럼 기본 스폰 레이아웃을 사용합니다. 지정한 ID가 없으면 Host가 오류를 보고하며 임의의 다른 입구에 스폰시키지 않습니다.
- 실제 3스테이지로 교체할 때 출구의 StageLevel 목적지와 도착 씬의 스폰 레이아웃을 연결하면 됩니다.

## 검증
- 컴파일 및 Play 모드 Console 오류/경고 없음.
- 실제 StageActor와 복제한 두 번째 배우를 사용해 인원 0/1/2, 중복 슬롯 제외, 이탈 시 감소 확인.
- 실제 CharacterController로 가지에서 출구 영역까지 보행 확인.
- 도착 씬 두 스폰의 지형 충돌 없음, 카메라/UGUI 실행 확인.
- StageOneNetworkCheck의 level-test 경로는 두 명 진입/한 명 이탈/3스테이지 도착/복용 능력 유지 검증으로 갱신했습니다.
- 이번 작업에서는 실제 Photon Host/Client 두 프로세스 전환 검사는 실행하지 않았습니다.
