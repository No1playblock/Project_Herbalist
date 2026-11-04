# 나무 구멍형 수액 주입구

프리팹: `Assets/_Project/Prefabs/InteractiveObjects/PF_TreeSapInlet.prefab`

불규칙한 타원형 나무껍질 테두리, 안쪽 벽, 어두운 뒷면을 가진 배치용 메시다. 기존 나무의 URP Unlit 재질과 텍스처를 복제한 다섯 재질로 깊이에 따라 어두워진다. 원본 나무 재질은 수정하지 않는다. 메시와 재질은 각각 `Art/Environments/Stage_01/Models/TreeSapInlet`, `Art/Materials/Environments/TreeSapInlet`에 있다.

로컬 +Z(파란 축)가 나무 바깥을 향하게 하고 원점을 나무 표면에 맞춘다. 테두리부터 뒷면까지 표면 앞에 두어, 원본 나무의 불투명 메시가 안쪽을 덮지 않게 한다. 실제 나무를 절삭하는 메시나 관통 구멍은 아니다. 프리팹 크기는 확대 후 약 4.2 × 6.2m이며 Transform으로 조절할 수 있다.

프리팹에는 기존 `SapInjectionPort`와 주입 판정 BoxCollider가 들어 있다. 새 기믹은 이 포트를 해당 기믹의 직렬화된 포트 필드에 연결한다. 기존 포트 아래에 외형만 붙이는 경우에는 중복 포트와 콜라이더를 제거하고 기존 수신기를 유지한다. 프리팹 자체에 장애물/점프대 동작은 포함하지 않는다.

## 시범 적용

메인 `Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference` 씬의 `Stage1_DocumentGimmicks/Coop_1_LinkedObstacleEjection/Arrival_LinkedInlet` 한 곳에 적용했다. 허공에 있던 연결 주입구를 오른쪽의 실제 나무 표면 `(287.47, 56.10, 314.92)`으로 옮겼다. 기존 장애물 기믹의 포트 참조는 유지한다. 다른 주입구는 이번 시안으로 교체하지 않았다.

시범 메시 `TreeSapInlet_SurfaceMatched.asset`은 해당 나무 표면에 맞춰 가장자리 위치와 UV를 보정한 전용 메시다. 다른 곳에는 범용 프리팹을 사용한다. 원본 나무와 버섯의 Transform/활성/메시/재질은 변경하지 않았다.

## 2026-11-04 전체 주입구 적용 및 점프대 연결

- 사용자가 키운 시안의 월드 배율 `(2.65, 2.65, 2.65)`을 프리팹 기본 크기에 반영했다. 위치를 올린 시안은 해당 위치를 유지하고 조준점도 외형을 따라간다.
- 기존 `WallWaterInlet` 20곳의 예전 외형/판정은 비활성화하고 같은 크기의 새 프리팹 외형과 입구 부피에 맞는 BoxCollider로 교체했다. 부모 배율을 보정하므로 모두 월드 크기가 같다. 원래 포트 오브젝트와 다른 기믹의 참조는 유지한다.
- 사용자 확인에 따라 정확히 지정한 `Mushroom.fbx`를 쓰는 6개만 점프대다. `MushroomVisual`은 기존 포트 Transform ID `1852367055`, `Mushroom`은 `153831838`에 연결한다. 기존 포트 하나가 다른 버섯 메시 안에 묻혀 있어 외형/조준점을 가려지지 않는 표면으로 보정했다.
- `MushroomVisual (1)/(2)/(3)/(4)`에는 근처 포트를 새로 추가했다. 앞의 두 곳은 나무 표면, 뒤의 두 곳은 가까운 나무가 없어 기존 버섯 표면에 입구를 붙였다. 원본 나무/버섯 모델 배치는 바꾸지 않았다.
- 동작 루트는 `Stage1_DocumentGimmicks/MushroomFbxJumpPads`이다. 예전 17개 점프대 루트와 시뮬레이션은 비활성화해 이중 발사를 방지한다. 연결되지 않은 나머지 WallWaterInlet은 외형만 바뀌며 새 점프대를 임의로 추가하지 않는다. 기존 성장 버섯 주입 연결은 유지한다.
- 기존 Host/offline 수액 충돌 처리로 충전한다. 근처 R 유지 시 `SapInjectionPort._aimPoint`를 향해 자동 조준한다. 링이 나무와 겹치는 근거리에서는 같은 링 위의 가려지지 않는 발사점을 찾는다. 발사점이 주입구 판정 안에 있는 경우도 주입을 받는다. 벽 너머로 직접 주입하는 우회는 추가하지 않았다.
- 1초 주입 후 3초 경고와 분출, 실제 탑승자 발사는 기존 설정을 사용한다. 점프대 잎은 시간 제한 없이 유지하고, 실제 플레이어 발사 직후 회수한다.

`TreeSapInletChecks.Play()`에서 6곳 모두 실제 물줄기 조준 충전, 다른 곳을 보고 가까이 R 유지로 충전, 120초 잎 유지, 캐릭터 발사와 잎 회수를 확인했다. 실제 두 프로세스 Host/Client와 전체 이동 동선은 별도 검증 대상이다. 외형의 주입 중 물빛·패턴 반응은 추가하지 않았다.

생성 도구: `Herbalist/Art/Create Tree Sap Inlet Prefab`. 기존 재질과 메시가 있으면 튜닝을 유지하며 프리팹을 다시 생성한다. 씬 시범 적용은 자동 실행하지 않는다.

## 점프대와 주입구 묶음 편집

사용자 요청으로 현재 연결된 6쌍을 `Stage1_DocumentGimmicks/MushroomFbxJumpPads/JumpPadPair_01~06` 아래에 각각 묶고, `Assets/_Project/Prefabs/InteractiveObjects/StageOneJumpPadPairs`에 개별 프리팹으로 저장했다. 각 부모의 자식은 원본 버섯 모델, 해당 `WallWaterInlet`, `Gameplay_LeafSocket_AndWaterJet`이다. 하나의 부모를 이동/삭제하면 그 점프대와 주입구 및 기믹을 함께 편집한다. 외형·판정의 월드 배치는 유지했다. 버섯 모델의 부모 변경은 이 묶음 편집 요청에 따른 것이다.

| 부모 이름 | 버섯 모델 |
| --- | --- |
| JumpPadPair_01_MushroomVisual_1 | MushroomVisual (1) |
| JumpPadPair_02_MushroomVisual_2 | MushroomVisual (2) |
| JumpPadPair_03_MushroomVisual | MushroomVisual |
| JumpPadPair_04_Mushroom | Mushroom |
| JumpPadPair_05_MushroomVisual_4 | MushroomVisual (4) |
| JumpPadPair_06_MushroomVisual_3 | MushroomVisual (3) |

주입구·잎 소켓·분출 오브젝트의 연결이 모두 같은 묶음 내부에 있고 6개 모두 프리팹에 연결됨을 확인했다. 묶음 밖에 있는 나머지 `WallWaterInlet`은 이 6개 점프대와 연결되지 않는다.
