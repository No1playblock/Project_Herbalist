# HERBALIST 포트폴리오 이미지 초안

현재 게임의 3스테이지는 **폭포**다. 아래 합성 이미지와 생성 스크립트의 제단 화면은 이전 프로토타입을 사용한 임시 자료이므로 최종 포트폴리오에 그대로 사용하지 않는다.

원본 포트폴리오 PDF의 16:9 페이지 구성을 기준으로 제작했다. 원본 PDF와 게임 에셋은 수정하지 않았다.

| 파일 | 용도 | 출처 |
| --- | --- | --- |
| `images/stage01_live_game_full.png` | 프로젝트 개요의 단독 게임 화면 | 2026-10-17 현재 `Stage_01_ModelingReference`를 Unity Play 모드에서 3840×2160으로 캡처 |
| `images/01_gameplay_capture_collage.png` | 프로젝트 개요의 임시 3스테이지 이미지 | 위 실시간 캡처 + 기존 `Documentation/UI/Screenshots`의 미로·제단 프로토타입 화면 합성. 제단 부분은 폭포 화면으로 교체 필요 |
| `images/02_client_prediction.png` | 플레이어 이동 기술 페이지 왼쪽 이미지 | `NetworkPlayer`, `CharacterControllerMotor`의 실제 처리 순서를 도식화 |
| `images/03_leaf_replication.png` | 나뭇잎 능력 기술 페이지 왼쪽 이미지 | `NetworkLeafAbility`, `NetworkLeafProjectile`, `LeafInstallTarget`의 권한·복제 흐름 도식화 |
| `images/04_maze_simulation.png` | 미로 기술 페이지 왼쪽 이미지 | `SapMazeSimulation`, `SapMazeBoard`의 그래프 상태·순환 버퍼 도식화 |
| `images/05_stage_transition.png` | 씬 전환 기술 페이지 왼쪽 이미지 | `FusionLobbySession`의 스냅샷·로드·복원 흐름 도식화 |

이미지를 다시 만들려면 이 폴더에서 `build_images.py`를 실행한다. Python Pillow와 Windows 맑은 고딕이 필요하다.

현재 미로·제단 캡처는 기존 UI 문서에 저장된 프로토타입 화면이다. 특히 제단은 현재 3스테이지가 아니므로 폭포 게임 화면으로 교체해야 한다. 최신 아트와 실제 2인 플레이 화면으로 교체할 때 `build_images.py`의 `hero()` 입력 경로와 제목도 함께 바꾼다.
