# 스테이지 1 기믹 수정 반영 (2026-11-03)

사용자가 제공한 `기믹 수정사항.pdf` 6쪽을 기준으로 A안(스테이지 1 전용 설정/컴포넌트)을 승인받아 적용했다. 협동기믹 4의 대체 기믹은 확정하지 않았으며 이번 작업에서 새로 만들지 않았다.

## 씬과 설정

- 대상은 `Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch.unity`다. MainMenu 연결과 Stage_02_Mazes 전환은 유지한다.
- `StageLevel.abilityRules`는 전용 `Data/Stages/Stage_01/SO_StageOneGimmickAbilityRules.asset`을 참조한다. 잎 설정은 `SO_StageOneTimedLeaf.asset`이며 `permanentInstallation=false`, `lifetime=5`다. 다른 씬의 공용 설정은 변경하지 않았다.
- 새 배치는 `Stage1_Gameplay/GimmickRevision_20261103` 아래에 모았다. 수정 전 씬은 로컬 `Temp/GimmickRevisionReview/Before/Stage1.unity`에 보관했다.

## 잎과 나무

- 기존 일반 상승용 버섯 발판 24개를 비활성화하고 해당 외형 상단에 잎 설치 구역을 배치했다. 수액 주입구와 연결된 점프대 17개는 유지했다.
- 협동 동선의 연결 지점 6개를 포함해 새 설치 구역은 30개다. 초록 고리와 트리거만 있으며 표식 자체를 고정 발판처럼 밟을 수 없다. 설치한 잎이 실제 발판이다.
- 잎의 비행/조준 쿼리만 `StageOneLeafZone` 트리거를 포함한다. 다른 트리거는 무시하고, 앞에 있는 일반 충돌면의 가림 판정은 유지한다. 일반 `LeafInstallTarget`과 기존 점프대 소켓도 계속 동작한다.
- 설치 직후부터 5초를 센다. 두 플레이어의 탑승 여부나 사용 횟수는 타이머에 영향을 주지 않는다. 수동 회수와 용량 제한도 유지한다.
- `StageOneTraversal`은 메인 나무와 가지 충돌체 15개에 접촉하면 해당 플레이어를 마지막 일반 발판/이끼 지형 또는 시작점으로 돌려보낸다. 임시 잎은 체크포인트에 포함하지 않으며 배치된 능력 오브젝트와 다른 플레이어의 진행은 초기화하지 않는다.
- 이끼 안전 구역과 잎 설치 허용은 별도 판정이다. 이끼 구역은 회전 가능한 BoxCollider로 편집한다. 기존 가지 및 점프대 인근 나무 충돌면을 따라 이끼 외형을 구웠고 최종 가지 점프대 앞에 안전 착지용 가지를 추가했다. 소스 씬의 다른 누락 모델을 일괄 대체하지는 않았다.

## 천과 협동 동선

- 기존 3개 HangingCloth 컨트롤러의 빈 외형 참조를 실제 `movefabric (2)/fabric`, `Tree_playobject/movefabric (1)/fabric`, `Tree_playobject/movefabric (4)/fabric`에 대응시켰다. 원본 모델 프리팹의 외형은 숨기고 동일 메쉬/머티리얼 및 기존 Cloth가 있는 경우 그 설정을 독립 복제하여 컨트롤러 아래 연결했다. 잡는 지점은 실제 천 하단에 맞췄다.
- 해당 컨트롤러의 `_waterDriven`만 켰다. 수액의 실제 명중 방향을 기존 각속도 계산에 공급하고 탑승자의 방향키 구동은 차단한다. 빈 천도 밀 수 있다. 기존 탑승 중 능력 잠금과 잡기/놓기는 유지한다.
- 공유 `SO_HangingClothTuning`의 가속도, 최대 각속도, 스프링, 감쇠, 최대 각도와 방출 속도 값은 바꾸지 않았다. 천은 명중이 끝나도 기존 스프링/관성으로 계속 움직인다.
- 복제한 Unity Cloth 외형 두 개는 각각 2,004정점의 별도 읽기 가능한 메쉬를 사용한다. 재임포트 후 `isReadable=true`를 확인했다. 원본 모델의 임포트 설정은 바꾸지 않았다. 실제 Windows 빌드는 이번 작업에서 실행하지 않았다.
- `Coop_1_WaterClothAndLeaves`, `Coop_2_WaterClothAndLeaves`, `Coop_5_WaterClothAndLeaves`에 시작/도착 이끼 가지와 잎 연결 지점을 배치했다. `StageOneCoopRoute`에는 대응 천과 의도한 역할 순서를 Inspector 메모로 남겼다. 천과 잎의 실제 이동 조작을 이용하며 자동 이동이나 강제 착지는 추가하지 않았다.
- 문서의 '기존 유지' 항목인 분출구 차단/버섯 크기 변화에 대한 새 게임플레이 시스템은 이번 변경에 추가하지 않았다. 기존 점프대와 현재 씬의 다른 기믹은 보존했다.

## 검사 결과와 남은 플레이 검증

- 컴파일 및 최종 Play Console 오류/경고 0개.
- 저장 씬: Missing Script 0개, 활성 잎 타깃 ID 중복 0개, 활성 점프대 17개, 잎 설치 구역 30개, 수액 구동 천 3개.
- `Herbalist.Editor.StageOneChecks.Run()` 및 `SapHoseChecks.Run()` 통과.
- 스테이지 1 오프라인 Play에서 `Herbalist.Editor.StageOneGimmickRevisionChecks.RunPlay()` 통과: 같은 잎에 실제 두 캐릭터 캡슐을 배치하여 지지 확인, 4.99초 유지/5.01초 회수, 설치 표식 트리거 조준, 비고체 표식, 세 천의 좌/우 구동, 실제 탄도 수액 명중과 이동 방향.
- 별도 실제 씬 검사 통과: 탑승자 WASD로 구동되지 않음, 탑승 중 수액 발사 차단, 실제 나무 MeshCollider 및 CharacterController.Move로 정지한 접촉 감지, 접촉 시 복귀, 이끼 영역 안의 실제 나무 접촉 허용. 시작/도착 가지 6개의 중심에서 실제 메쉬 지지면도 확인했다.
- 이 결과는 구성요소/충돌 검증이다. 새 동선의 실제 Photon Host/Client 전체 완주, 역할별 사격 위치의 접근성, 모든 잎 구간의 점프 가능 거리 및 두 사람이 5초 안에 통과하는지는 추가 플레이 테스트가 필요하다. 수치 자동 보정이나 착지 고정은 하지 않았다.

배치 작업 코드: `Scripts/Editor/StageOneGimmickRevisionAuthoring.cs`. `Apply()`는 미적용 씬에 한 번만 실행하도록 보호한다. 이미 적용한 씬은 Hierarchy/Inspector에서 수정한다. 이 도구는 자동 실행되지 않는다.
