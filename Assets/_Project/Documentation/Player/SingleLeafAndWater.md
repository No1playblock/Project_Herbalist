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
