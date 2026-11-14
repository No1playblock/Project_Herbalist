# 소담 루트 모션

소담의 전신 애니메이션 중 실제 이동이 있는 `LeafCatch`, `ClothGrab`, `ClothHardLand`, `ClothSwingLand`, `TalismanSit`의 원본 RootT 곡선을 `SO_SodamAnimationProfile`에 저장한다. 달리기와 일반 점프처럼 시작과 끝의 위치가 같은 동작은 기존 CharacterController 이동과 점프가 담당한다. 머리·팔·다리 전용 레이어의 원본 RootT는 캐릭터 전체 이동에 사용하지 않는다.

`PlayerController`와 `NetworkPlayer`는 설정 에셋의 곡선을 시뮬레이션 시간으로 샘플링한 이동량을 `CharacterControllerMotor`에 넘긴다. Fusion에서는 Host가 동작과 재생 시간을 네트워크 상태로 지정하므로 재시뮬레이션 때 같은 구간을 다시 계산할 수 있다. Animator의 `Apply Root Motion`은 중복 이동을 막기 위해 꺼 둔다. 천이나 부적에 탑승한 동안에는 기존 운송 위치가 우선한다.
몸의 회전은 기존 카메라·이동 입력 제어가 맡고, 이 곡선에서는 위치 변화만 사용한다.

## 동작 타이밍 조정 (2026-10-26)

`SodamAnimationPresentation`은 Animator 평가 전 Update에서 동작을 전환한다. 일반 점프는 기존 모터의 즉시 도약을 유지하며 상체 0.7초, `leg.sodam_lower_jump` 0.57초 지점부터 시작한다. `SO_SodamAnimationProfile`의 각 레이어 `Start Time Seconds`와 포즈 `Blend Seconds`로 조정한다. 시작 시간은 상태 재생 시간 단위이므로 클립 시간 / Animator 상태 속도로 입력한다(상체 0.53846, 하체 0.7125, 잎 던지기 0.125초). 잎 동작 중 이륙하면 이전 전신 동작을 해제하며, 공중에서 잎을 던지거나 받을 때도 점프 하체 레이어를 적용한다.

잎은 입력을 수락하면 던지기 시퀀스를 먼저 시작하고, Leaf 설정의 `Throw Preparation Seconds` 후 Host/오프라인 시뮬레이션에서 실제 생성한다. 현재 준비 시간은 0.25초이며 준비 중 조준은 계속 갱신된다. 소담 던지기는 클립 0.2초부터 1.6배속으로 재생하고 0.65초 뒤 기본 동작으로 전환한다. 클립이나 속도를 변경하면 손이 뻗는 시점과 준비 시간을 함께 맞춘다. `Clear`는 준비 중인 발사를 취소하고, 게임 일시정지는 준비 타이머도 멈춘다. 용량 초과 시 기존의 가장 오래된 잎 회수 후 다음 던지기 준비를 시작한다.

잎 잡기는 회수 완료 대신 도착 예상 시간으로 시작한다. `Catch Anticipation Seconds`는 현재 0.3초다. 잡기 클립은 1.6배속, 유지 시간은 1.15초이며 루트 이동 곡선 재생 속도도 1.6으로 맞춘다. 던지기/잡기 시퀀스와 분사 여부는 기존 `NetworkLeafAbility`를 통해 복제한다.

잎 던지기·잡기·수액 분사 중 지상 이동 시 `leg.Run`에 평소 이동 클립을 적용한다. 상체 동작은 처음부터 다시 시작하지 않고 계속 재생한다. 잎 잡기 중 멈추면 leg.Idle로 원본 잡기 클립의 걷는 다리를 덮어쓴다. 잎 던지기·수액은 멈추면 원래 서 있는 자세를 사용하며, 공중에서는 `leg.sodam_lower_jump`로 전환한다. 이동 여부는 기존 Animator `Speed`와 프로필 `Moving Speed Threshold`를 따르며, 탑승용 동작은 기존 별도 자세를 유지한다.

물 자세는 추출/제어 여부 대신 실제 `Spraying` 상태를 따른다. `water cannon.fbx`의 가져오기 범위를 20–100프레임(30fps)으로 설정해 팔을 올리는 준비 구간을 제외하고, 준비된 자세로 0.04초 동안 블렌딩한다. 이 구간을 반복하므로 준비 동작이 다시 나타나지 않는다. 분사를 멈추면 남은 물줄기의 소멸과 별개로 기본 동작의 블렌딩을 시작한다.

검사: `Herbalist.Editor.SodamAnimationTimingChecks.Run()`은 실제 플레이어를 조작하지 않고 발사 지연, 중복 입력, 취소, 도착 전 잡기, 하체 점프 상태와 물 자세 복귀를 검사한다. 두 프로세스 Host/Client 검사는 별도로 수행한다.

설정 에셋의 `Use Root Motion`은 해당 동작의 이동을 켠다. `Override Movement Input`은 동작이 이동하는 동안 WASD 이동량을 대신한다. `Ignore Root Motion Y`는 이미 지면에 닿은 천 착지가 다시 떠오르지 않도록 수직 성분을 제외한다. `Visual Offset Y`는 클립의 발 높이를 맞추며, 현재 천 착지 두 동작에 설정되어 있다. 클립을 교체하거나 길이·속도를 바꾸면 RootT 및 발 높이 곡선을 다시 샘플링해야 한다.

2026-10-26 검증 결과: 타이밍 검사 통과, 스크립트 컴파일 오류 없음. 별도 SapHoseChecks.Run()은 authored raised surface volume 검사에서 실패했으며, 이 검사는 이번 애니메이션 수정 경로보다 앞에서 수면 볼륨 프리팹을 확인한다. 실제 두 프로세스 Host/Client 재검사는 수행하지 않았다. 검증 중 DX12 GPU 장치 오류로 에디터가 종료되어 프로젝트 그래픽 설정 변경 없이 DX11 실행 옵션으로 복구했다.


## 정지 상태 잎 회수 수정 (2026-10-26)

잡기 원본 클립에는 다리의 보행과 RootT 이동이 들어 있다. 지상 정지 시 leg 레이어를 끄면 이 보행이 노출되므로, LeafCatch에 한해 프로필 Idle 클립을 사용하는 leg.Idle를 유지한다. 이동 중에는 기존 leg.Run, 공중에서는 기존 점프 하체를 사용하며 상체 잡기는 다시 시작하지 않는다. LeafCatch > Use Root Motion도 해제해 잡기 자체가 정지 플레이어를 움직여 걷기로 판정되거나 발소리를 만들지 않도록 했다. 입력 이동은 기존대로 유지한다.

SodamAnimationTimingChecks.Run() 통과: 정지 잡기 발 위치 변화 한계, 상체 연속 재생, 정지→이동 복귀, 기존 이동·공중 하체와 발사/회수 타이밍 검증.

## 두영 능력 애니메이션 공통 적용 (2026-10-26)

기존 `SodamAnimationPresentation` 컴포넌트의 직렬화 이름은 유지하고, 활성 캐릭터가 두영일 때 잎 던지기·잡기 시퀀스와 실제 수액 `Spraying` 상태를 같은 프로필로 읽는다. `AC_DuyeongLocomotion`에 Humanoid 능력 클립 3개와 하체 마스크 레이어를 연결했다. 두영의 기존 Idle/Run/Jump 클립과 자동 전환은 유지한다. 정지 잡기는 두영의 `leg.Idle`, 이동은 두영의 `leg.Run`, 공중 능력 사용은 기존 점프 하체를 사용한다. 분사를 멈추거나 잡기/던지기가 끝나면 하체 레이어를 해제하고 두영 기본 이동으로 복귀한다. 새 네트워크 필드는 추가하지 않았다.

`DuyeongAbilityPresentationChecks.Run()`은 실제 두영 Humanoid 모델에서 던지기·잡기 시퀀스, 정지 발 고정, 이동·공중 하체, 상체 연속 재생, 수액 분사와 종료 복귀 및 기존 자동 달리기 전환을 검사하며 통과했다. 소담 기존 타이밍 검사도 통과했다. 실제 2인 Host/Client 플레이 재검사는 별도다.

캐릭터별 능력 외형 분기는 없다. 두 캐릭터는 같은 잎·수액 설정과 생성 프리팹을 사용한다. 현재 잎 메시는 `Assets/_Project/Art/Abilities/Leaf/Models/Leaf_low.fbx`이고, 수액은 `PF_SapDeposit`의 `M_WaterOrb`, `MAT_SapStream` 및 물 분사·충돌 VFX 재질을 공유한다. 솔로/네트워크 잎 메시 일치와 수액 재질 15종의 일치를 확인했다. 네트워크 플레이어 프리팹 4종도 모두 동일한 `PF_NetworkLeafProjectile`, `PF_NetworkSapDeposit`을 참조한다.
