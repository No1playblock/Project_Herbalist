# 3스테이지 — 수액 점프대 (260919 기획)

## 적용 범위
- 승인된 A: 발사 때 CharacterController 모터가 두 플레이어를 지정 착지점으로 이동시키고, 나뭇잎 상승은 별도 연출로 표시합니다.
- 이동하는 플랫폼 위에서 두 캐릭터를 운반하는 구조는 아닙니다.
- Stage_03_Prototype은 기획의 수직 협동 경로를 검증하는 임시 지형입니다. 최종 배경 모델/유체 아트가 아닙니다.
- 기존 Stage_02_BGModel의 공동 출구 → Stage_03_Prototype → Stage_04_Arrival로 연결됩니다.
- 일반 방 시작 목적지는 기존처럼 Stage_02_BGModel입니다. 4스테이지는 도착 공간만 있으며 퍼즐은 구현 범위 밖입니다.

## 플레이 순서
1. 초록색 LeafSocket에 **발판 모드** 나뭇잎을 던집니다. 한 슬롯에는 하나만 설치됩니다.
2. 두 플레이어가 설치된 나뭇잎 위에 올라섭니다. 단순히 근처에 있는 플레이어는 인원에 포함하지 않습니다.
3. 뒤쪽 SapSupply에서 수액을 추출하고 청록색 SapInlet에 분사합니다.
4. 두 명이 올라선 상태에서 설정된 양을 주입하면 두 사람을 동시에 발사합니다.
5. 사용한 나뭇잎은 기존 회수 경로로 돌아갑니다. 쿨다운 후 다시 나뭇잎을 설치해 사용할 수 있습니다.
6. 점프대 사이의 가는 기둥에는 일반 나뭇잎/수액을 붙여 올라갑니다. 마지막 상단 영역에 둘이 모이면 다음 씬으로 이동합니다.

나뭇잎의 기존 5초 수명은 유지합니다. 두 사람이 준비한 후 설치하는 협동 흐름입니다.
일반 기둥은 기존 **수액 먼저 → 나뭇잎 결합 → 회수할 때까지 유지** 규칙입니다.
점프대는 별도 장치 주입구이므로 **나뭇잎 먼저 → 두 명 탑승 → 수액 주입**으로 동작합니다.
주입구에 맞은 수액은 일반 바닥 흔적을 만들지 않습니다.

## 구조
| 파일/컴포넌트 | 역할 |
| --- | --- |
| SapInjectionPort | 권한 있는 분사/설치 경로에서 받은 수액량 전달 |
| LeafInstallTarget | 단일 설치 슬롯, 정확한 소켓 위치, 설치 나뭇잎 조회 |
| SapJumpPad | 탑승 인원·충전·발사·쿨다운, Host 상태 동기화 |
| SapJumpPadSettings | 인원, 충전량, 유예, 비행 시간/중력, 쿨다운, 연출 |
| PlayerMotor / CharacterControllerMotor | 외부 발사 속도 유지, 착지 후 일반 이동 복귀 |
| NetworkPlayer | 발사 상태/중력의 예측 복원 및 Host 텔레포트 |
| StageFallRecovery | 시작점 아래로 떨어진 플레이어만 해당 슬롯의 시작점으로 복귀 |
| StageThreeProgress | 발견·설치·첫 착지·나뭇잎 구간 튜토리얼의 Host 진행 상태 |
| SapJumpPadStatus / GameOverlayHud | 미리 배치된 UGUI 상태 안내와 기획 대사 |
| CooperativeStageExit | 상단 두 명 진입 판정 및 기존 능력 보존 씬 전환 |

키를 새로 하드코딩하지 않습니다. 기존 능력 입력을 그대로 사용합니다.
UI는 프리팹/씬에 미리 있으며 실행 중 생성하지 않습니다.
기존 영구 결합·회수·용량 규칙은 변경하지 않습니다.

## 주요 에셋
- 씬: Assets/_Project/Scenes/Stages/Stage_03/Stage_03_Prototype.unity
- 도착 공간: Assets/_Project/Scenes/Stages/Stage_04/Stage_04_Arrival.unity
- 점프대: Assets/_Project/Prefabs/InteractiveObjects/Stages/PF_SapJumpPad.prefab
- 설정: Assets/_Project/Data/Stages/Stage_03/SO_SapJumpPadSettings.asset
- Hierarchy: StageThree_Layout/SapJumpPad_1~3
- 착지점: StageThree_Layout/Landing_1~3/Slot_0, Slot_1
- 경로 안내: StageThree_Layout의 LevelRouteGuide (Scene Gizmos)

| 설정 | 초기값 |
| --- | --- |
| Required Riders | 2 |
| Charge Units | 1 |
| SapInjectionPort Units Per Second | 1 |
| Injection Grace | 0.2초 |
| Flight Duration | 1.4초 |
| Gravity | 18 |
| Cooldown | 1.5초 |
| FallThreshold | Y=-5 |
| LowerCatchFloor | 상단 Y=-8 |

시작 스폰은 (-1.25, 0.04, -2.5), (1.25, 0.04, -2.5)입니다.
3스테이지 입구 ID는 FromStage02, 4스테이지는 FromStage03입니다.

## 추가 배치
1. 점프대 프리팹을 배치하고 활성화합니다.
2. LeafSocket의 Target Id, SapInlet의 Receiver Id를 씬 내 고유 양수로 지정합니다.
3. Landings에 각 슬롯의 착지 Transform을 연결합니다. 경로에 천장/기둥이 있으면 실제 충돌로 비행이 막힙니다.
4. 필요하면 별도 SapJumpPadSettings 에셋을 만들어 충전량·비행 시간을 조절합니다.
5. 프리팹 자식 UGUI는 그대로 재사용합니다. 장치별 문구는 SapJumpPadStatus에서 변경합니다.
6. NetworkObject를 포함한 씬을 저장/베이크합니다. 기존 Editor 생성 도구가 최종 베이크를 수행합니다.

StageThreeBuilder.Build는 최초 임시 레벨 생성용이며, 기존 StageThree_Layout 또는 Stage_04_Arrival이 있으면 덮어쓰지 않습니다.
후속 맵 작업에서는 프리팹/씬을 직접 편집합니다.

## 검증
- StageThreeChecks.Run(): Stage_03_Prototype Play 모드에서 실행.
  - 실제 LeafProjectile 충돌 설치, 단일 슬롯 거절, 중복 슬롯 제외
  - 한 명만 탑승 시 충전 금지, 분사 중단/나뭇잎 회수 시 충전 초기화
  - 실제 hose hit 경로로 충전, 세 점프대에서 두 명 발사·착지
  - 매 틱 MotorState 복원 시 발사 궤적 유지, 발사 중 일반 점프 차단
  - 쿨다운 후 재준비, 개인별 낙하 복귀, 기존 설치물/파트너 유지
  - 상단 공동 진입/이탈 인원 확인
- StageEntranceGateChecks.Run(): 기존 입구 역주행 차단/예측 복원 회귀 검사 통과.
- SapHoseChecks.Run(): 일반 흔적, 거리 갱신, 성장, 수명, 결합 순서, 영구 유지 및 회수 회귀 검사 통과.
- 실제 두 프로세스 검사용 플래그:
  - -herbalist-stage-three-check host ROOM
  - -herbalist-stage-three-check client ROOM
  - Development Build에서만 활성화됩니다. 런타임 복제 설정으로 3스테이지 직접 진입하며 실제 프로젝트 로비 설정은 변경하지 않습니다.
  - Host의 실제 나뭇잎 생성/충돌과 수액 hit 경로를 사용합니다. 키보드/마우스 조작 자동화 검사는 아닙니다.

### 최종 실행 결과 — 2026-09-22
- Windows Development Build 성공, 빌드 오류 0개.
- 독립 Photon Host/Client 프로세스 모두 세 점프대의 발사 및 착지 PASS.
- Host 개인 낙하 복귀 및 양쪽 Stage_04_Arrival 전환·능력 보존 PASS.
- 착지 중 출구가 먼저 동작하지 않도록 최종 착지점과 출구를 분리했습니다.
- 나뭇잎 아래 바닥에 서 있는 캐릭터를 탑승자로 계산하지 않는 회귀 검사 PASS.
- Play 모드 Console 오류/경고 없음. 실제 키보드 조작감과 최종 배경 아트는 별도 플레이 테스트/레벨 작업 대상입니다.
