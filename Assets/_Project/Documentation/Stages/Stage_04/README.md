# Stage Four — 수액 미로 협동 프로토타입 (260927 수정)

## 씬과 진행
- 씬: Assets/_Project/Scenes/Stages/Stage_04/Stage_04_Arrival.unity.
- 기존 3스테이지 → 이 씬 → Stage_05_Arrival 임시 도착 씬의 연결과 GUID를 유지한다.
- 새 기획서의 전체 스테이지 통합/번호 재분류는 이번 변경 범위에 포함하지 않았다.
- 문서 일부에 남은 상승/주입 설명보다 사용자가 전달한 최신 규칙(수액 미리 배치, R 전환, 하강)을 우선한다.
- 역할은 복용한 능력으로 결정한다. 캐릭터/슬롯에 고정하지 않는다.
- 상층에서 시작해 두 명이 앞/뒤 발판에 각각 탑승하면 첫 미로로 내려간다. 완료한 미로마다 두 명 탑승 후 다음 층으로 하강한다.
- 직접 혼자 실행하는 Editor/Development 테스트에서는 아래의 StageFourSoloTest 옵션으로 첫 미로를 바로 활성화한다. 옵션을 끄거나 멀티로 실행하면 기존 두 명 탑승 조건을 적용한다.

## 수액 조종
1. 각 미로의 시작 지점에 설정된 양의 수액이 이미 있다. 나무에서 추출하거나 주입하는 과정은 없다.
2. 수액 역할이 현재 미로 앞면 제어 구역에서 R(재설정 가능한 기존 Cycle 입력)을 누르면 수액 조종을 시작한다.
3. WASD는 미로 평면의 상하좌우로 수액을 움직인다. 카메라 방향과 무관하다. 조종 중 캐릭터 이동과 점프는 잠긴다.
4. 다시 R을 누르면 캐릭터 이동으로 돌아온다. 수액의 위치/잔량은 유지된다. 조종을 끈 동안에도 막히지 않은 누수 구간에 수액이 있으면 계속 감소한다.
5. 제어 구역을 벗어나거나 능력을 바꾸거나 미로가 완료/실패하면 조종을 해제한다. 제어 구역의 R 입력은 일반 호스 능력을 동시에 켜지 않는다.
6. 수액이 목표에 도달하면 해당 미로 완료. 도착 전 전부 소진하면 재시도 대기 후 시작 위치/시작량으로 복원하고 현재 미로의 설치된 나뭇잎만 제거한다. 자동으로 조종에 재진입하지 않고 R을 다시 눌러야 한다.
7. 이전 미로의 완료 상태/발판 위치를 보존한다. 개인 낙하 복귀는 진행 중인 층으로 되돌린다.
8. 세 미로와 마지막 하강을 완료하고 두 명이 하단 출구에 모이면 5스테이지 임시 씬으로 이동한다.

## 시점 선택 (A 승인)
각 SapMaze_1~3의 SapMazeBoard 컴포넌트에서 Control View를 설정한다.
- Keep Player Camera: 기본값. 조종 중에도 기존 카메라를 유지하고 마우스로 회전 가능.
- Maze Camera: 조종 중 Front Camera에 할당한 미로 전용 앵커 시점으로 전환. R로 조종을 끄면 원래 카메라로 복귀.
- 이 설정은 게임플레이/네트워크 상태와 분리되어 있다. Tab 개인/분할 화면 전환을 유지한다.

## 나뭇잎과 누수
- 구멍은 **발판형(Platform)**만 받는다. 고정형(Pin)은 거부한다.
- 수액이 누수 반경 내에 있는 동안만 설정된 초당 양만큼 감소한다. 구간을 벗어나면 감소하지 않는다.
- 젖은 구멍에 나중에 설치한 발판은 수액 우선 결합 규칙으로 회수 전까지 유지된다.
- 먼저 설치한 발판에는 소급 결합하지 않는다. 기본 5초 수명을 유지한다.
- 기존 최대 3개, 가장 오래된 것 회수 후 다음 클릭 투척 규칙을 유지한다.

## 구성과 조정값
- SapMazeDefinition: 저장된 노드/연결/누수 위치, Initial Volume(미로별 시작량), Speed, Leak Radius, Loss Per Second, Retry Delay.
- SapMazeSimulation: 이동/누수/도착/재시도 계산.
- SapMazeBoard: 조종 상태와 역할별 입력 연결, 카메라/이동 잠금, Fusion 복제.
- IAbilityCycleReceiver: 기존 능력 Cycle 입력의 대상별 전달. 키를 코드에서 직접 검사하지 않는다.
- MazeLeakSocket: 발판 설치, 결합, 초기화.
- StagePulleyLift: 출발/도착 앵커 사이의 두 명 이동 운반. 기존 serialized _lower/_upper 이름을 유지하되 각각 출발/도착이며 높이 순서를 강제하지 않는다.
- StageFourFlow: 미로 순서, 낙하 복귀, 최종 출구.
- Host가 조종 전환, 수액 이동/누수/완료, 승강 진행을 결정한다.
- UI는 미리 작성된 UGUI를 사용한다. 런타임 UI 생성은 없다.

데이터: Assets/_Project/Data/Stages/Stage_04/SO_SapMaze_1~3.asset.

| 항목 | 현재 테스트값 |
|---|---|
| 미로 형상 | 260927 PDF 28 / 29 / 30쪽 원본, 너비 10, 원본 비율 유지 |
| 누수 구멍 | 3 / 4 / 5 |
| 시작 수액량 | 12 / 16 / 20 |
| 수액 속도 | 1.2 |
| 누수 반경 / 초당 감소 | 도면 14픽셀에 해당하는 월드 거리(약 0.177 / 0.174 / 0.124) / 6 |
| 재시도 대기 | 2초 |
| 발판 이동 시간 | 3초 |
| 진행 높이 | 39 → 36 → 24 → 12 → 0 |
| 나뭇잎 타깃 ID | 501–503 / 511–514 / 521–525 |

시작 수액량은 확정 밸런스가 아닌 테스트값이다. 난이도마다 Inspector에서 독립적으로 변경한다.

## 에디터와 검증
StageFourBuilder는 에디터에서만 씬을 작성한다. ApplyDescendingRevision은 기존 씬/GUID와 미로 경로를 유지하며 하강 배치로 갱신하는 명시적 도구다. 기존 주입구/공급 나무를 제거하고 조종 시점과 안내 문구를 갱신한다. 사용자 변경이 있는 씬은 저장 후 실행해야 한다.

현재 세 미로는 서낭당_레벨_기획서_260927.pdf의 28·29·30쪽 도면을 사용한다. 흰 통로를 수액 경로로, 검정 선을 벽으로 옮겼고 녹색 시작·빨간 도착·파란 누수 표식 위치를 유지한다. 비균일 벽 간격을 임의 격자로 대체하지 않는다.

- 원본 이미지: Assets/_Project/Art/Environments/Stage_04/Textures/TEX_SapMaze_1~3.jpeg. 문서에서 원본 이미지를 추출한 것이며 새 UI가 아니다.
- 저장된 작성 데이터: Assets/_Project/Data/Stages/Stage_04/Layouts/MazeLayout_1~3.json. 원본 픽셀 좌표, 상하좌우 이동 연결, 벽 영역과 표식 위치를 보관한다.
- 검정 벽: MESH_SapMazeWalls_1~3.asset + 씬의 ReferenceWalls MeshCollider. 앞면 ReferenceFace는 원본 도면을 표시한다. 분리된 벽도 유지하고 인쇄 글자는 벽에서 제외한다.
- Tools/trace_stage_four_mazes.py는 에디터 작성용 도구다. PyMuPDF/Pillow/NumPy/SciPy로 PDF를 읽고, 모든 이동 선분이 원본 흰 영역 안에 있으며 상하좌우인지를 검사한다. 런타임 Python 의존성은 없다.
- 갱신: 위 도구에 새 PDF 경로를 전달하고 Unity에서 **Herbalist > Stages > Apply PDF Stage Four Maze Layouts**를 실행한다. Play를 종료하고 사용자 씬 수정을 저장한 뒤 실행한다. 기존 씬·데이터 GUID와 능력/누수 타깃 ID는 보존한다.
- StageFourBuilder도 같은 도면 데이터를 사용한다. 이전 무작위 경로 생성은 제거했다.
- 원본 29쪽의 **누수1은 시작 통로와 완전히 분리**되어 수액이 도달할 수 없다. 요청한 원본 벽을 유지했으며, 시작→도착 경로는 연결되어 미로 완료는 가능하다. 이 부분을 개방하려면 기획 확인 후 도면/경로/벽을 함께 수정한다.
- 작성 검증: StageFourReferenceChecks.Run은 도면별 누수 수, 상하좌우 입력만으로 도착 가능 여부, 원본의 분리된 누수1 상태를 검사한다.

- StageFourChecks.Model: 미리 배치된 시작량, 통로/벽, 누수/차단/이탈, 재시도 복원, 도착 유지.
- StageFourChecks.Scene: StageFourSoloTest의 Enabled For Offline Testing을 끈 새 Play에서 실행. R 진입/해제/재진입, 카메라 두 옵션, 조종 해제 중 누수, 실제 발판형 설치/결합, 실패 초기화 범위, 네 번의 두 명 하강, 최종 출구.
- StageFourNetworkCheck: Development 두 프로세스에서 Client의 실제 R/WASD 입력 → Host 처리, 발판형 차단, 하강, Stage Five 전환. 자동 검사만 속도 2/잔량 120의 메모리 설정을 사용한다. 기본 저장된 설정은 바꾸지 않는다.
- 실행 인자: -herbalist-stage-four-check host 방이름 또는 client 방이름. 일반 실행은 검사 코드를 사용하지 않는다.
- 최종 아트와 실제 두 사람의 난이도/조작 느낌은 별도 튜닝이 필요하다.

260928 검증 완료: 에디터 기본값으로 세 미로/재시도/R 전환/카메라 두 옵션/네 번의 하강 검사 통과. 기존 SapHoseChecks와 StageThreeChecks 회귀 검사 통과. Development 빌드 errors=0. 실제 Photon Host/Client 두 프로세스에서 Client의 R 진입/해제/재진입과 WASD, 발판형 누수 차단, 세 미로와 네 번의 하강, Stage Five 전환 및 능력 보존 검사 통과. 머리 위 진입 바닥이 투척 경로를 가리던 문제는 상층 진입 동선을 외곽 통로로 변경하여 해결했다.


## 솔로 테스트 A (260928)
씬의 StageFour_Layout에 StageFourSoloTest 컴포넌트를 미리 배치했다.

- Enabled For Offline Testing: 기본 켜짐. Editor 또는 Development Build에서 네트워크 세션 없이 실행할 때만 적용한다.
- 시작 시 첫 미로 앞 제어 구역으로 이동하고 수액 능력을 선택한다. 최초 두 명 승강 단계는 건너뛴다.
- R: 수액 조종 시작/해제. WASD: 조종 중 수액, 해제 중 플레이어 이동.
- Q: 기존 임시 능력 전환. 나뭇잎으로 전환하면 수액 조종/이동 잠금이 해제된다. 뒷면으로 이동해 R로 발판형을 선택하고 좌클릭으로 누수를 막는다. 다시 Q로 수액 능력, 앞면에서 R로 조종을 재개한다.
- 수액을 도착시키면 앞/뒤 어느 쪽 하강 발판이든 한 명이 탑승해서 다음 미로로 진행할 수 있다. 마지막에는 하단 출구에 혼자 진입하면 Stage_05_Arrival로 이동한다.
- Sap Volume Multiplier: 기본 25. 혼자 양쪽을 오가며 능력을 바꾸는 시간을 위한 임시 배율이다. 실제 난이도로 검사하려면 1로 바꾼다.
- 배율은 런타임 ScriptableObject 복제본에만 적용한다. 기본 미로 데이터의 12/16/20 및 Photon 멀티 수액량은 바뀌지 않는다.
- 새 UI를 생성하지 않고 기존 UGUI 목표/미로 안내 문구를 갱신한다. 비활성 미로에는 R 조종 안내 대신 대기 안내를 표시한다.

### 끄기와 제거
1. Play를 종료하고 StageFour_Layout / StageFourSoloTest의 Enabled For Offline Testing을 끄거나 컴포넌트를 제거한다.
2. 영구 제거 시 StageFourBuilder.Build의 해당 컴포넌트 추가/참조 연결 코드와 StageFourSoloTest.cs/.meta, StageFourSoloChecks.cs/.meta를 제거한다.
3. 더 이상 쓰이지 않는 StageFourFlow.BeginOfflineTest/EndOfflineTest 및 솔로 분기, StagePulleyLift의 OfflineForTesting 메서드, SapMazeBoard.ConfigureOfflineTestDefinition을 함께 정리할 수 있다.
4. 기존 Q 스위처와 메인 메뉴 솔로 버튼은 별도 테스트 기능이다. 요청 범위에 포함되지 않으면 유지한다.
5. Inspector 옵션 변경은 Play 종료 후 적용하는 것을 권장한다. 멀티의 두 명 역할/탑승/출구 조건은 그대로 유지한다.

StageFourSoloChecks.Run은 첫 미로 자동 활성화, 실제 입력 액션 R/Q, 실제 플레이어 위치에서 발판 나뭇잎 투척/수액 결합, 세 미로와 한 명 하강, 기본 데이터 불변을 검사한다. 마지막 출구에 플레이어를 놓아 다음 프레임의 실제 Stage Five 씬 이동도 확인한다. 테스트 시 자동 입력은 MCP 실행 중 게임창 포커스를 요구하지 않도록 커서 포커스 제한만 일시 해제한다.

260928 솔로 검사와 옵션 해제 후 협동 회귀 검사 통과. 직접 솔로 Play에서 Stage_05_Arrival 전환 확인.

추가 검증: 솔로 옵션이 씬에 켜진 Development 빌드에서도 실제 Photon Host/Client는 OfflineTestActive=false로 시작하며, 기본 수액/두 명 탑승 조건을 유지한 상태에서 세 미로와 네 번의 하강 및 Stage Five 전환 검사를 통과했다. 빌드 errors=0.


260928 도면 적용 검증: 추출 도구에서 세 미로의 모든 이동 선분이 검정 벽을 통과하지 않음 확인. 원본 비율을 유지한 최종 데이터의 상하좌우 도착 검사와 모델 검사 통과. 솔로 실제 R/Q 및 발판 나뭇잎 결합·세 미로 완료·하강·Stage Five 전환 확인. 솔로 옵션을 끈 두 명 협동 에디터 검사에서 재시도 초기화, 실제 발판 결합, 세 미로 완료와 네 번의 하강 통과. 이번 도면 변경 후 Photon 두 프로세스 검사는 재실행하지 않았다.
