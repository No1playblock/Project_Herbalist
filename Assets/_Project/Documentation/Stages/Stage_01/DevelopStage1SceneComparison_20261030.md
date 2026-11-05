# develop_stage1 스테이지 1 씬 복제 및 캐릭터 비교

- 원본: `origin/develop_stage1` / `1e7d7c57` (2026-10-30 07:25 +0900, `chore: stage1: path`)
- 현재 브랜치: `design-stage1`
- 기존 메인: `Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch.unity`
- 비교용 복사본: `Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1.unity`
- 최종 결정: 사용자 승인으로 복제본을 기존 `Stage_01_ModelingReference_CharacterBranch.unity`에 저장해 Stage1 메인으로 사용한다. 기존 GUID `830e461bec2a49adaaa7dcdceebf21aa`를 유지하여 MainMenu/빌드 연결은 유지했다. 비교용 임시 씬은 제거하고 수정 전 사본을 Logs에 보관했다.

## 차이

| 대상 | 현재 위치 X/Y/Z | develop_stage1 위치 X/Y/Z |
| --- | --- | --- |
| Player (두영 오프라인 플레이어) | 690.37060 / 0.56184 / 39.64227 | 281.51000 / 122.80000 / 256.89000 |
| Player_Sodam (소담 오프라인 플레이어) | 692.77063 / 0.57807 / 39.64227 | 373.00000 / 0.57807 / 279.00000 |

- 캐릭터 루트 스케일 `(2.7, 2.7, 2.7)`, 두 모델의 Visual 스케일 `(1,1,1)`과 회전은 같다.
- `SO_Stage01ModelingPlayerTuning` 참조와 원본 에셋 내용이 양 브랜치에서 같다. 이동/점프/중력/카메라 튜닝에 차이가 없다.
- 씬에 적용된 캐릭터 모델/뼈대, Animator/Avatar/Controller 참조, CharacterController와 콜라이더, 능력/입력/표시 컴포넌트 직렬화 설정을 비교했으며 위 두 루트 위치 외에는 같다.
- PlayerSpawnLayout 계층/연결/멀티플레이 스폰 마커와 `player_position`은 같다. 달라진 위치는 씬에 직접 놓인 두 오프라인 플레이어다. 카메라의 로컬 설정은 같지만 부모 캐릭터가 옮겨져 실제 월드 위치는 따라 바뀐다.
- Preview Scene 두 개에서 Player, Player_Sodam, Main Camera, PlayerSpawnLayout, player_position, PlayScreen, StageLevel의 설정을 수집했다. Unity의 일시적인 내부 m_FileID/m_PathID를 제외하고 실제 값의 차이는 위 두 Transform 위치뿐이다.

## 복제 범위와 에셋 격리

- 원본 씬의 누락 환경 의존 에셋 17개(모델/재질/텍스처)를 GUID를 보존해 가져왔다.
- 기존과 내용이 다른 공유 환경 에셋 19개(Terrain, 환경 재질, 가지 모델, Volume Profile)는 새 GUID로 별도 복사했다. 기존 메인 씬의 외형에 영향을 주지 않도록 `Art/Environments/Stage_01/DevelopStage1Import` 아래에 배치했다.
- 복사본의 GUID 참조만 새 환경 사본에 맞게 변경했다. 이 GUID 매핑을 되돌린 씬 바이트는 원격 원본과 완전히 일치함을 확인했다. 이후 사용자 승인으로 복사본을 기존 메인 씬 경로에 승격했다. 메타 GUID와 빌드 설정은 유지했다.
- 복사본의 TerrainData가 새 사본을 참조하며 Terrain Layer 5개 모두 Missing 없이 연결됨을 MCP에서 확인했다.
- 공용 C# 코드, 캐릭터 튜닝/모델, SFX 및 UI 설정/프리팹은 현재 브랜치 것을 유지했다. 원본 브랜치와 공용 파일 전체를 덮어쓰는 작업은 하지 않았다. 공용 참조 파일의 브랜치 차이 목록은 검증 로그에 별도 보관했다.

## 원본 참조와 후속 복구

- `BranchVisual` 18개와 별도 `Branch1_Low` 1개의 누락 메쉬를 기존 설치 포즈에 맞는 `ImportedFromLatest/Branch/Branch1_Low.fbx` 메쉬로 복구했다. BranchVisual의 누락 재질은 같은 세트의 `Branch.mat`로 연결했으며 MeshCollider도 메쉬에 맞췄다. 최신 FBX의 메쉬 내부 ID와 기존 씬 참조 ID가 달랐던 것이 메쉬 누락 원인이다.
- `TSI_Springleaf_Tree_02A_LOD0`의 Missing Prefab 및 `TSI_Water_Lily_Flower_01A_LOD0`의 Missing Material은 기존 씬과 복사본 양쪽에 공통으로 존재한다.
- Play에서 두 허브 모두 상호작용 힌트와 채집 미니게임 시작을 확인했다. 점프대 17쌍은 테스트 코드로 나뭇잎 설치, 탑승 판정, 주입 충전, 지연 후 분출과 캐릭터 수직 발사를 확인했다. 이 검증은 MCP를 통한 컴포넌트 호출 검사이며 키보드로 전체 경로를 완주하거나 실제 Host/Client로 접속한 검사는 아니다. 검증 중 콘솔 오류는 없었다.

검증 스냅샷/차이/에셋 매핑은 로컬 `Logs/Stage1DevelopImport_20261030`에 있다.

## 허브 및 점프대 연결 복구

- `Stage1_Gameplay/Herbs` 아래 기존 LeafHerb_Source/SapHerb_Source를 유지하고 교체된 꽃/허브 모델을 연결했다. 새 모델 위치에 InteractionPoint 및 감지용 Collider를 두었고, 떨어져 있던 수액 허브 장식도 SapHerbVisual로 묶었다.
- `Stage1_Gameplay/JumpPads` 아래 17개의 독립적인 점프대를 만들었다. 각 항목은 원래 배치된 Inlet 1개, 실제 플랫폼 모델, LeafOutlet/LeafSocket, SapJet 및 기존 ExteriorJumpPad 기믹을 포함한다. Inlet과 플랫폼 모델은 월드 위치를 보존했다. 발사 기준점은 플랫폼 표면 위에 놓았다.
- 원본의 새 주입구 17개에는 SapInjectionPort/충돌체가 없었고, 기존 점프대에는 오래된 배열 참조가 남아 있었다. 새 주입구에 동작과 충돌체를 연결하고 각 점프대의 포트/소켓/분출 참조를 하나씩 맞췄다. LeafInstallTarget ID 중복도 해소했다.
- 일반 발판은 Platforms 아래에 정리했다. 이전 기믹 그룹은 Legacy_Disabled 아래 비활성 보관했다.
- 상세 모델-주입구 매칭과 Play 검사 결과: `Logs/Stage1Repair_20261030`.

## 2026-11-03 최신 외형 통합

`origin/develop_stage1`의 `183c529e`, `1e7d7c57`, `b6740472`, `f99cee84`, `6a13b390`, `5c51c016` 변경을 현재 `design-stage1`에 통합했다. 지형/바위/나무/동굴/안개/조명/장식 배치와 필요한 외부 모델·텍스처, Terrain Tools/FBX 의존성과 렌더링 설정을 포함한다.

- 최신 원격 씬의 외형 구성을 기준으로 현재 `Stage1_Gameplay`와 플레이어·화면·스폰·진행 설정을 보존했다. 최종 메인 씬 경로와 GUID는 기존 것을 유지한다.
- 약초 2개, 활성 점프대 17개와 일반 발판/Legacy_Disabled 구성을 유지한다. 버섯 발판 04_Musroom (32)의 최신 위치에 맞춰 JumpPad_13_Mushroom의 출구를 보정하고 주입구는 원격 배치를 유지했다.
- 기존 가지 모델의 내부 Mesh 식별자 불일치 19개를 현재 임포트된 Branch1_low 메시 및 해당 MeshCollider로 복구했다.
- 충돌한 신규 에셋 GUID 15개는 현재 브랜치 에셋의 기존 GUID를 유지하고 새 사본을 새 식별자로 연결했다. 자세한 경로와 GUID 매핑은 `DevelopStage1VisualSync_20261103.json`에 기록한다.
- 원격에서 제거했더라도 현재 다른 프리팹이 사용하는 구형 나무 의존 에셋은 호환성을 위해 보존한다.
- 게임플레이 C# 코드, 수액 고리/전방향 분사/매터리얼 두 버전, Stage 2/3 씬과 전환 경로는 현재 브랜치를 유지한다.

검증: 씬 스크립트 누락 0개, 활성 점프대 17개 포트/소켓/분출 참조 검사, StageOneChecks와 SapHoseChecks 통과. 전체 협동 등반 경로의 실제 Host/Client 완주는 별도다. 통합 전 브랜치는 `codex/backup-stage1-before-visual-sync-20261103`에 남겼으며, 상세 비교/진단 파일은 `Temp/Stage1VisualSync`에 있다.
최종 재로드/Play 확인: 두 플레이어 위치와 루트 스케일 2.7 보존, 활성 점프대 17개·약초 2개, 스크립트 누락 0개, 타깃 ID 중복 0개, 에셋 GUID 중복 0개. Stage 1 직접 Play에서 두 캐릭터가 지면에 안정적으로 정지하며 Console 오류 0개를 확인했다. 임시 비교 씬 2개는 제거했다.

## 2026-11-03 비교 씬 배치 재일치 및 가지 크기 정정

사용자 요청으로 `Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference` 비교 씬을 유지하고 메인 씬의 공통 3D Transform을 비교 씬과 대조해 위치·회전·스케일을 맞췄다. 앞선 통합에서 플레이어 위치를 유지했던 규칙은 이 요청으로 대체하며, 오프라인 플레이어도 비교 씬의 위치를 따른다. 메인 씬 경로/GUID, 약초 및 활성 점프대 17개의 연결과 수액 코드는 유지한다.

앞선 메시 참조 복구는 잘못된 수정이었다. 연결한 Branch1_low는 임포트된 메시 크기가 약 (1020,565,2532)여서 기존 배율에서도 약 500 크기의 가지가 생겼다. 이 19개의 메시 및 해당 MeshCollider 연결을 해제해 비교 씬과 같은 누락 상태로 되돌렸다. 원격에는 오래된 메시 내부 식별자 참조가 남아 있으며, 올바른 원본 메시가 확인되기 전까지 다른 메시로 대체하지 않는다.

검증 결과: 공통 3D Transform 4,975개의 위치/회전/스케일 차이 0건, 비교 씬과 동일한 가지 메시 누락 19개, 활성 점프대 17개, 약초 2개의 상호작용점이 실제 비주얼 경계 내부, 스크립트 누락 0개. StageOneChecks 및 SapHoseChecks 통과, Console 오류 0개. 변경 전 씬과 비교 자료는 `Temp/Stage1PlacementMatch`에 보관했다.