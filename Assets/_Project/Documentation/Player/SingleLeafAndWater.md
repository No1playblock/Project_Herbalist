# 단일 나뭇잎과 물 능력

2026-09-29 승인된 A 구조: 나뭇잎은 하나이며 대상이 설치 위치와 방향을 정한다.

- R: 나뭇잎 투척. 좌클릭: 가장 오래된 설치 나뭇잎 회수. 기존 Input Action 이름은 바꾸지 않는다.
- 벽: 조준 방향을 유지하고 긴 축의 끝을 얕게 박는다.
- 점프대·미로 누수·제단: `LeafInstallTarget`의 Socket 위치/회전을 사용한다. Single Occupant로 중복 설치를 제한한다.
- 새 대상은 양수 고유 Target ID를 지정하고 Collider와 `LeafInstallTarget`을 배치한다. 별도 능력 모드를 추가하지 않는다.
- `On Installed` / `On Released` 이벤트는 첫 설치와 마지막 회수 시 반응을 연결한다. 옛 `On Pinned` 이벤트 참조는 유지된다.
- 개수·사거리·수명은 `LeafAbilitySettings`와 스테이지 설정 에셋에서 조절한다. 260928 레벨 나뭇잎은 영구 설치된다.

물과 나뭇잎 결합 판정, Bound 상태, 결합 반경, `SapBindingSource`와 관련 네트워크 필드는 삭제했다. 물은 자체 수명으로 사라지고 나뭇잎 회수는 물을 제거하지 않는다. 누수는 설치된 나뭇잎만으로 막는다. 물은 파란색이며 기존 표면 데칼·입체 물막·스트림을 유지한다. 내부 `Sap` 코드/에셋 이름은 기존 참조 호환성을 위해 유지한다.

기존 두 나뭇잎 프리팹의 중복 외형을 제거하고 `LeafVisual` 하나로 통일했다. 기존 끝 앵커·Collider·외형 참조는 직렬화 이름 이관으로 보존했다. 백업은 프로젝트 루트의 `Temp/AbilityCleanupBackup_20260929_202058/original-assets.zip`에 있다. 전체 코드 복원은 변경 전 Git 버전을 사용한다. 백업은 Git에 포함하지 않는다.

검증 진입점은 `AbilityCleanupChecks.Authored/Run`, `DesignRevisionChecks`, `PlayerInteractionChecks`, `SapHoseChecks`다. 옛 모드·결합 전용 검증은 삭제했다. Fusion 네트워크 필드가 변경됐으므로 Host와 Client 모두 같은 버전으로 다시 빌드해야 한다.

검증 결과: 12개 씬의 스크립트 참조와 두 나뭇잎 프리팹 참조, 독립 수명·누수 차단/회수, R 투척·좌클릭 회수, 통과 점프, 물 분사, 점프대 탑승, 미로 및 제단 검사를 통과했다. Windows 개발 빌드는 오류 0개로 완료했고 실제 Photon Host/Client에서 단일 나뭇잎 설치 자세/회수와 능력 보존 씬 전환을 양쪽 PASS로 확인했다. 로그는 `build/RevisionNetworkCheck/host_cleanup.log`, `client_cleanup.log`다. 네트워크 검증은 설치 자세를 직접 지정해 복제 상태를 검사하며, 투척 비행과 전체 등반 경로를 사람이 플레이한 검사는 아니다.

전체 씬 점검 중 `Sandbox_Rendering`에 기존 `sodam_test` 프리팹(GUID `b2a0b5655cb8dab4a89ce481222ca1de`) 누락이 확인됐다. 변경 전 HEAD에도 같은 참조가 있으며 이번 능력 정리와는 별개다.

## 몸 주위 수액 고리 (2026-11-01)

수액 능력이 해금되고 선택되어 있으면 플레이어 프리팹의 `WaterOrbit`가 상시 표시된다. 분사 중 고리의 일부가 서서히 열리고, 멈추면 다시 닫힌다. 조종 중 앞쪽 구체는 숨긴다. 기존 재바인딩 입력과 스테이지별 사용 방식은 유지하며, contextualUse 스테이지에서는 R을 누르는 동안 분사한다. 몸의 전방/머리 회전 각도 제한 없이 후방·측면·상하로 조준할 수 있고, 주입구 사용도 몸을 강제로 회전시키지 않는다.

`SapAbilitySettings`의 `orbitCenterOffset`, `orbitRadius`, `orbitTubeRadius`는 월드 단위다. `orbitUnwindDuration`, `orbitUnwindAngle`로 분사 지속 시간에 따른 고리 소모 속도를, `orbitRestoreDuration`으로 버튼을 놓은 뒤 복원 시간을 조절한다. 고리의 남은 길이는 일정한 속도로 줄어들며 기존 110도 열림 제한을 사용하지 않는다. 현재 세 수액 설정 모두 소모/복원 시간을 0.45초, 소모 각도를 360도로 맞춰 줄어드는 속도와 다시 채워지는 속도가 같다. `orbitRipple`, `orbitFlowSpeed`는 표면 움직임 설정이다. 확대 캐릭터용 `SO_ModelingSap`에는 별도 크기를 직렬화했다. `orbitEnabled`를 끄면 기존 구체/가슴 위치 연출로 돌아간다. 발사 방향 제한 해제는 유지된다.

`SapOrbitPresentation`은 기존 해금/선택/분사 복제 상태와 시선 정보를 읽는 연출이다. 충돌·주입·물 자국 수명은 기존 Host 시뮬레이션을 사용하며, 새로운 네트워크 필드는 추가하지 않았다. `Herbalist/Abilities/Apply Water Orbit`는 플레이어 프리팹 연결을 재적용한다. 씬에 직접 작성된 플레이어는 씬의 `WaterOrbit`와 컴포넌트 참조를 보존한다.

검증: `SapHoseChecks.Run()`의 전후좌우/상하 발사, 몸 방향 유지, 구체 숨김, 물줄기 충돌/주입/수명 검사가 통과했다. 5개 플레이어 프리팹의 고리 단일 구성과 렌더러 연결, 고리의 닫힘/열림 렌더링을 확인했다. 실제 두 기기의 Host/Client 플레이 검증은 별도다.

2026-11-02 수정: 고리는 별도 Lit 재질 대신 물대포 튜브의 매터리얼을 그대로 공유하고 같은 길이 기준 UV로 물 흐름을 표시한다. 물줄기 굵기에 잘못 적용된 보정 배율을 제거하여 고리/물줄기의 기준 지름을 orbitTubeRadius * 2로 통일했다. 고리 끝은 실제 발사 원점에 맞추며 출구 부근을 발사 접선으로 부드럽게 연결한다. 기본 소모 설정은 4초에 약 37%, 8초에 약 14%의 길이가 남고, 버튼을 놓으면 0.45초에 복원된다. 소모는 연출이며 분사를 차단하는 자원 제한은 아니다.

2026-11-02 매터리얼 프리셋: `Art/Materials/Abilities/MAT_SapStream.mat`에는 변경 직전 Unity에서 조정 중이던 값을 보존했다. `MAT_SapStream_Reference.mat`는 사용자 이미지 설정이며 고리와 물줄기 및 기존 공유 스트림 참조에 적용했다. 새 설정의 Flow Pattern Strength=0.086, Edge Falloff=2.79, Foam Strength=0, Foam Coverage=0.159, Foam Pattern Scale=8.6, Water Beam Pattern Strength=0.242, Water Beam Lengthwise Tiling=1.7, Water Beam Glow=1.73이다. 색과 텍스처는 조정 중이던 원본에서 복사했다.

`Water Beam Flow Speed`는 워터빔 텍스처가 길이 방향으로 이동하는 속도다. 적용된 이미지 프리셋은 0.1이며 0은 이 패턴만 정지, 작은 양수는 느리게, 큰 양수는 빠르게, 음수는 역방향이다. 스크롤을 타일링 이전에 적용해 타일링을 바꿔도 공간상의 이동 속도는 유지된다. 기존 `Flow Speed`는 표면 하이라이트/거품 흐름, `Water Beam Pattern Strength`는 워터빔 무늬 강도, `Water Beam Lengthwise Tiling`은 무늬 밀도다. 원본의 새 속도 값은 변경 전 외형의 이동 속도를 유지하도록 설정했다.