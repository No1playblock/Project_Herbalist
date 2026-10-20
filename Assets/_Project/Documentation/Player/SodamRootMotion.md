# 소담 루트 모션

소담의 전신 애니메이션 중 실제 이동이 있는 `LeafCatch`, `ClothGrab`, `ClothHardLand`, `ClothSwingLand`, `TalismanSit`의 원본 RootT 곡선을 `SO_SodamAnimationProfile`에 저장한다. 달리기와 일반 점프처럼 시작과 끝의 위치가 같은 동작은 기존 CharacterController 이동과 점프가 담당한다. 머리·팔·다리 전용 레이어의 원본 RootT는 캐릭터 전체 이동에 사용하지 않는다.

`PlayerController`와 `NetworkPlayer`는 설정 에셋의 곡선을 시뮬레이션 시간으로 샘플링한 이동량을 `CharacterControllerMotor`에 넘긴다. Fusion에서는 Host가 동작과 재생 시간을 네트워크 상태로 지정하므로 재시뮬레이션 때 같은 구간을 다시 계산할 수 있다. Animator의 `Apply Root Motion`은 중복 이동을 막기 위해 꺼 둔다. 천이나 부적에 탑승한 동안에는 기존 운송 위치가 우선한다.
몸의 회전은 기존 카메라·이동 입력 제어가 맡고, 이 곡선에서는 위치 변화만 사용한다.

설정 에셋의 `Use Root Motion`은 해당 동작의 이동을 켠다. `Override Movement Input`은 동작이 이동하는 동안 WASD 이동량을 대신한다. `Ignore Root Motion Y`는 이미 지면에 닿은 천 착지가 다시 떠오르지 않도록 수직 성분을 제외한다. `Visual Offset Y`는 클립의 발 높이를 맞추며, 현재 천 착지 두 동작에 설정되어 있다. 클립을 교체하거나 길이·속도를 바꾸면 RootT 및 발 높이 곡선을 다시 샘플링해야 한다.
