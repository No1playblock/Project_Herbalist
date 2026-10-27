# 1스테이지 캐릭터·점프대 튜닝 가이드

기준 씬은 `Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch.unity`입니다. 아래 값은 현재 프로젝트에 저장된 값이며, Unity Inspector에서 같은 이름의 에셋이나 컴포넌트를 선택해 수정합니다. 거리·높이·속도는 Unity 월드 단위 기준입니다.

## 먼저 열 파일

| 하고 싶은 작업 | 수정 위치 | 영향 범위 |
| --- | --- | --- |
| 모든 씬의 플레이어 크기 조절 | `PF_Player`, `PF_NetworkPlayer`, `PF_StageNetworkPlayer`, `PF_Stage01ModelingNetworkPlayer`의 루트 Transform | 솔로·멀티플레이어의 모델, 충돌 캡슐, 카메라 기준점과 능력 발사 위치 |
| 1스테이지 이동·일반 점프·카메라 조절 | `Assets/_Project/Data/Characters/SO_Stage01ModelingPlayerTuning.asset` | 이 씬의 솔로 두 플레이어와 전용 멀티플레이어 프리팹 |
| 다른 씬의 기본 플레이어 조절 | `Assets/_Project/Data/Characters/SO_PlayerTuning.asset` | 기본 `PF_Player`, `PF_StageNetworkPlayer`를 그대로 쓰는 씬 |
| 버섯·나뭇가지 점프대의 충전·상승·물줄기 조절 | `Assets/_Project/Data/Stages/Stage_01/SO_ModelingMushroomJumpPad.asset` | 현재 1스테이지의 **두 종류 모두** |
| 점프대의 위치·외형·주입구·분사구 조절 | 씬의 `PF_MushroomJumpPad` 또는 `PF_BranchJumpPad` 인스턴스 | 선택한 배치 하나. 프리팹 자체를 수정하면 그 변형의 모든 배치 |

## 씬에서 캐릭터 보며 작업하기

현재 모델링 씬에는 솔로용 `PF_Player` 인스턴스가 이미 두 개 있습니다. Hierarchy의 `Player`는 두영, `Player_Sodam`은 소담으로 시작합니다. 두 인스턴스의 루트 Scale은 CharacterModeling 기준인 `(2.7, 2.7, 2.7)`로 맞춥니다. 먼저 이 두 오브젝트를 선택하고 Scene 뷰에서 `F`로 포커스를 맞추면 됩니다.

2026-10-26 사용자 요청으로 플레이어 프리팹과 씬의 `SodamVisual`, `DuyeongVisual` 내부 Local Scale을 모두 `(1, 1, 1)`로 통일했습니다. 소담의 기존 1.363 배율은 제거했습니다. 플레이어 루트 크기·이동 튜닝·카메라 줌은 이 변경에 포함하지 않습니다.

| 용도 | 프리팹 | 사용 방법 |
| --- | --- | --- |
| 솔로 플레이어 원본 | `Assets/_Project/Prefabs/Characters/PF_Player.prefab` | 입력·카메라·충돌·능력을 포함합니다. 현재 씬에 이미 두 개가 있으므로 단순 외형 확인을 위해 추가 배치하지 않습니다. |
| 현재 1스테이지 멀티플레이어 | `Assets/_Project/Prefabs/Characters/PF_Stage01ModelingNetworkPlayer.prefab` | `PlayerSpawnLayout`이 Host에서 `PlayerSpawn_1`, `PlayerSpawn_2` 위치에 생성합니다. 루트 Scale은 2.7이고 1스테이지 전용 튜닝을 참조합니다. 씬에 직접 배치하지 않습니다. |
| 다른 씬의 공용 멀티플레이어 | `Assets/_Project/Prefabs/Characters/PF_StageNetworkPlayer.prefab` | 루트 Scale 3.2와 기본 튜닝을 사용합니다. 현재 모델링 씬의 `PlayerSpawnLayout`은 이것을 사용하지 않습니다. |
| 소담 외형만 배치 | `Assets/_Project/Art/Characters/sodam/BranchV12/Prefabs/sodamv12.prefab` | 다른 씬에서 크기·배치를 비교할 때 사용합니다. 플레이어 조작 컴포넌트는 없습니다. |
| 두영 외형만 배치 | `Assets/_Project/Art/Characters/Duyeong/prefab/dy_v5.prefab` | 다른 씬에서 크기·배치를 비교할 때 사용합니다. 플레이어 조작 컴포넌트는 없습니다. |

새 씬에서 **외형을 놓고 보기만 할 때**는 두 모델 프리팹을 `ReferenceCharacters` 같은 빈 부모 아래에 배치하면 됩니다. 게임 플레이를 시험할 때는 `PF_Player`를 사용하고, 모델만 배치한 오브젝트에는 입력·카메라·충돌이 없음을 기억하세요. 현재 씬의 `PlayerSpawnLayout`에 솔로 플레이어 두 개와 네트워크 프리팹이 이미 연결되어 있으므로 중복 플레이어를 놓으면 카메라·입력·스폰이 겹칠 수 있습니다. 모델만 직접 배치했을 때는 플레이어 프리팹 내부의 외형 Transform/루트 Scale 오버라이드가 적용되지 않아 크기가 달라 보일 수 있습니다.

네 플레이어 프리팹 모두 `SO_SapAbilitySettings`와 `SO_LeafAbilitySettings`를 참조합니다. 새 씬에서도 같은 프리팹을 사용하면 능력 설정이 함께 따라옵니다. 다만 수액 구체 반지름과 나뭇잎 충돌 반지름처럼 설정 에셋에 월드 단위로 저장된 값은 플레이어 루트 Scale에 자동으로 곱해지지 않습니다.

## 캐릭터 튜닝: `SO_Stage01ModelingPlayerTuning`

씬의 두 솔로 플레이어는 `PlayerController`, `CharacterControllerMotor`, `PlayerView`의 `Tuning` 참조를 이 에셋으로 오버라이드합니다. 멀티플레이어용 `PF_Stage01ModelingNetworkPlayer`도 같은 에셋을 참조합니다. **셋 모두 동일한 에셋**을 사용하므로 이 값을 바꾸면 두 캐릭터에 함께 적용됩니다. 프리팹 원본의 `Tuning`만 바꾸고 씬 오버라이드를 그대로 두면 이 씬의 솔로 플레이에는 반영되지 않습니다.

### 크기·이동·점프

| Inspector 값 | 현재 값 | 의미와 조절 방향 |
| --- | ---: | --- |
| `Character Scale Multiplier` | 1 | 프리팹 루트 크기에 추가로 곱하는 배율. 현재 1스테이지의 오프라인 두 인스턴스와 전용 네트워크 프리팹 루트 Scale은 2.7입니다. 크기 실험 시 두 방식의 배율을 중복해서 올리지 마세요. |
| `Move Speed` | 18 | 지상에서 WASD로 이동할 때의 목표 수평 속도. 크게 하면 더 빠릅니다. |
| `Gravity` | 88 | 일반 점프·낙하 때 아래로 가속하는 값. 크게 하면 더 빨리 떨어지고 같은 `Jump Height`를 위한 초기 점프 속도도 커집니다. |
| `Terminal Speed` | 140 | 일반 낙하의 최대 하강 속도. |
| `Ground Stick Speed` | 8 | 바닥에 붙어 있을 때 아래로 유지하는 작은 속도. 경사나 미세한 단차에서 접지를 돕습니다. |
| `Body Turn Speed` | 540°/초 | `MovementDirection`에서 몸이 이동 방향으로 도는 최대 속도. |
| `Facing Mode` | `MovementDirection` | 카메라와 독립적으로 WASD 이동 방향을 향해 몸을 돌립니다. `CameraAligned`는 몸을 카메라 방향에 맞춥니다. |
| `Jump Height` | 5.5 | **스페이스 일반 점프**의 목표 높이. 초기 상승 속도는 `sqrt(2 × Gravity × Jump Height)`로 계산됩니다. 점프대 상승 높이와는 별개입니다. |
| `Air Acceleration` | 70.4 | 공중에서 WASD로 수평 속도를 바꾸는 정도. 크게 하면 점프·점프대 하강 중 방향 전환이 빨라집니다. |

### 카메라·시선

| Inspector 값 | 현재 값 | 의미와 조절 방향 |
| --- | ---: | --- |
| `Camera Distance` | 19.2 | 충돌 보정 전, `CameraTarget`에서 카메라까지의 기본 거리. 실제 거리는 아래 줌 배율을 곱합니다. |
| `Camera Zoom Min/Max Multiplier` | 0.5 / 1.5 | 휠로 선택할 수 있는 거리 배율 범위. 장애물이 없을 때 현재 거리 범위는 9.6~28.8입니다. |
| `Camera Zoom Step` | 5 | 휠 한 단위당 줌 배율 변화량. 크게 할수록 한 번에 더 많이 당겨지거나 멀어집니다. 코드는 휠 입력을 120으로 나누어 적용합니다. |
| `Initial Pitch` | 30° | 카메라 시작 상하 각도. |
| `Pitch Limits` | -90° / 90° | 카메라를 위·아래로 돌릴 수 있는 각도 범위. |
| `Mouse Sensitivity` | 0.15 | 마우스 이동에 따른 카메라 회전 민감도. 게임 옵션의 시점 배율도 함께 곱해집니다. |
| `Camera Probe Radius` | 0.64 | 카메라가 벽에 가려지는지 검사하는 구의 반지름. |
| `Camera Wall Padding` | 0.32 | 벽 충돌 지점에서 카메라가 떨어져 남기는 거리. |
| `Camera Obstacles` | Default 및 22번 레이어 | 카메라 거리 충돌 검사에 포함할 레이어 마스크. |
| `Head Yaw/Pitch Limit` | 75° / 90° | 몸에 대한 머리의 좌우·상하 회전 제한. |
| `Head Turn Speed` | 240°/초 | 머리가 목표 시선으로 회전하는 속도. |
| `Head Tracking Fade Angles` | 75° / 90° | 카메라와 몸의 좌우 각도 차이가 75°부터 커질 때 머리 추적을 약하게 하고, 90°에서 중립으로 돌리는 범위. |

마우스 휠로 바꾼 배율은 소담·두영별로 `PlayerPrefs`에 저장됩니다. 게임을 껐다 켜도 마지막 배율이 남으므로, `Camera Distance`를 바꿨는데 카메라가 예상과 다르면 저장된 휠 배율도 확인하세요.

## 점프대 튜닝: `SO_ModelingMushroomJumpPad`

`PF_MushroomJumpPad`와 `PF_BranchJumpPad`는 `PF_ExteriorJumpPad_Base`의 동작을 공유하고 같은 설정 에셋을 참조합니다. **이 에셋의 값은 버섯과 나뭇가지에 동시에 적용됩니다.** 한쪽만 다르게 만들려면 별도의 `SapJumpPadSettings` 에셋을 만든 뒤 해당 변형의 `ExteriorJumpPad > Settings` 참조를 바꿔야 합니다.

물은 `WallWaterInlet`/`BranchWaterInlet`의 `SapInjectionPort`로 들어가고, 분출 위치는 각 변형의 잎 소켓(`LeafCenter`)과 `SapJet`/`BranchSapJet`입니다. `ExteriorJumpPad`의 `Ports`, `Sockets`, `Leaf Centers`, `Jets` 배열이 이들을 짝짓습니다. 현재 변형은 한쪽 주입구라 같은 쪽으로 분출합니다. 물은 나뭇잎을 설치하지 않아도 분출하지만, 플레이어 운반은 **설치된 나뭇잎 위에 선 경우에만** 시작합니다.

### 실제 동작에 사용되는 값

| Inspector 값 | 현재 값 | 의미와 조절 방향 |
| --- | ---: | --- |
| `Charge Units` | 1 | 발사 예약까지 필요한 누적 주입량. 두 변형의 주입구 `Units Per Second`가 각각 1이므로 연속 주입 시 약 1초입니다. |
| `Injection Grace` | 0.2초 | 주입이 잠깐 끊겨도 기존 충전량을 유지하는 시간. 이보다 오래 끊기면 충전량이 0으로 초기화됩니다. |
| `Launch Delay Seconds` | 3초 | `Charge Units`가 찬 뒤 실제 분출과 플레이어 발사까지 기다리는 시간. 이 사이에 음성으로 충전 완료와 남은 시간을 알리고, 플레이어는 나뭇잎 위에 올라탈 수 있습니다. |
| `Voice Volume` | 1 | 점프대 음성 안내의 크기. |
| `Charged/Two Seconds/One Second/Launch Voice` | 한국어 음성 4개 | 충전 완료, 2초·1초 남음, 발사 순간에 재생할 음성 파일. 음색이나 문구를 바꾸려면 이 참조를 교체합니다. |
| `Flight Duration` | 1초 | 나뭇잎·물줄기 분출 연출의 전체 시간. 현재 상승 0.5초 뒤 하강 0.5초입니다. |
| `Cooldown` | 2.6초 | 분출 시작 시점부터 재충전이 막히는 시간. 분출 종료 뒤에 시작하는 대기시간이 아닙니다. |
| `Standing Tolerance` | 0.64 | 설치된 나뭇잎이 플레이어 발밑에 있는지 검사할 때 허용하는 수직 거리. 너무 작으면 탑승 인식이 어렵고 너무 크면 멀리 선 플레이어도 포함될 수 있습니다. |
| `Presentation Rise` | 15 | 잎 소켓과 물기둥이 출발점에서 올라가는 최대 높이. 일반 `Jump Height`와 다릅니다. |
| `Eruption Rise Duration` | 1.5초 | 나뭇잎이 최대 높이까지 올라가는 목표 시간. 실제 상승시간은 `min(Eruption Rise Duration, Flight Duration ÷ 2)`입니다. |
| `Rider Launch Speed` | 73 | 분출이 시작될 때 탑승자에게 즉시 주는 위쪽 초기 속도. 플레이어는 올라가는 나뭇잎보다 먼저 위로 날아가며, 공중에서 WASD로 조종할 수 있습니다. |

### 물줄기 외형에 사용되는 값

| Inspector 값 | 현재 값 | 의미와 조절 방향 |
| --- | ---: | --- |
| `Water Stream Width` | 1.408 | LineRenderer 물줄기 폭. |
| `Water Stream Segments` | 22 | 물줄기 곡선을 이루는 구간 수. 올리면 곡선이 부드러워지지만 점 수가 늘어납니다. |
| `Water Stream Sway` | 0.0144 | 물줄기가 좌우로 흔들리는 크기. |
| `Water Spray Drops Per Meter Per Second` | 1.8 | 줄기 길이와 시간에 비례하는 물방울 발생 밀도. |
| `Water Spray Drop Size` | 0.028~0.068 | 물방울 입자의 무작위 크기 범위. |
| `Water Spray Side Speed` | 0.45~1.25 | 물방울이 줄기 옆으로 퍼지는 속도 범위. |
| `Water Spray Forward Speed` | 0.6 | 물방울이 줄기 진행 방향으로 움직이는 속도. |
| `Water Spray Max Drops Per Frame` | 6 | 한 프레임에 생성할 최대 물방울 수. |

`SapJet`/`BranchSapJet`의 LineRenderer 재질과 입자 시스템 자체는 프리팹 안에서 조절합니다. 위 에셋은 줄기의 폭·흔들림·물방울 방출 수치를 조절합니다.

### Inspector에 보이지만 현재 1스테이지 점프대가 읽지 않는 값

`SapJumpPadSettings`가 다른 점프대와 설정 형식을 공유해 다음 필드도 표시됩니다. 현재 `ExteriorJumpPad` 동작에서는 **사용하지 않으므로 값을 바꿔도 이 점프대의 높이·발사 힘은 바뀌지 않습니다.**

| 값 | 현재 값 | 설명 |
| --- | ---: | --- |
| `Required Riders` | 0 | 현재 분출 조건에 사용하지 않음 |
| `Gravity` | 18 | 점프대 운반·낙하에 사용하지 않음. 최고점 이후 플레이어의 `PlayerTuning > Gravity`가 적용됨 |
| `Presentation Duration` | 0.65초 | 현재 분출 연출에 사용하지 않음 |
| `Jump Window` | 0.3초 | 현재 분출 동작에 사용하지 않음 |
| `Jump Reach` | 5 | 현재 분출 동작에 사용하지 않음 |
| `Launch Height Multiplier` | 5 | 현재 분출 동작에 사용하지 않음. 높이는 `Presentation Rise`가 결정 |

## 배치·튜닝 순서

1. Hierarchy의 기존 `Player`와 `Player_Sodam`으로 캐릭터의 현재 크기를 확인합니다. 새 씬에서 외형 비교만 한다면 소담·두영 **모델 프리팹**만 배치합니다.
2. 이동감은 `SO_Stage01ModelingPlayerTuning`의 `Move Speed`, `Jump Height`, `Gravity`, `Air Acceleration` 순서로 조절합니다. 현재 1스테이지 플레이어 크기는 루트 Scale 2.7과 `Character Scale Multiplier`를 함께 확인합니다. 크기를 바꾸면 솔로 인스턴스와 전용 네트워크 프리팹을 모두 같은 값으로 맞춥니다.
3. 충전 뒤 대기 시간은 `Launch Delay Seconds`, 나뭇잎과 물기둥의 높이는 `Presentation Rise`, 플레이어가 솟는 높이는 `Rider Launch Speed`, 나뭇잎 상승 시간은 `Eruption Rise Duration`, 전체 연출 시간은 `Flight Duration`으로 조절합니다.
4. 점프대 프리팹 변형을 씬으로 드래그해 위치를 잡습니다. 선택 시 하늘색 기즈모는 주입구, 초록색 기즈모는 잎/물 분출점입니다. 복제한 뒤에는 `LeafInstallTarget`의 ID가 고유한지 확인하고 씬을 저장합니다. 필요하면 `Tools > Herbalist > Stage 1 > Assign Jump Pad IDs`를 실행합니다.
5. 프리팹 인스턴스 Inspector에서 굵게 표시된 값은 씬 오버라이드입니다. 에셋을 바꿨는데 한 배치만 다르게 보이면 해당 오버라이드를 먼저 확인합니다.
