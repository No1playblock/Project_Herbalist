# HERBALIST 포트폴리오 이미지

`images/live/`는 Unity Editor의 **Play 모드 Game View**에서 직접 촬영한 원본과 그 자르기본이다. 캐릭터, UI, 미로, 숲은 게임이 렌더링한 화면이며 생성 이미지가 아니다.

| 파일 | 내용 |
| --- | --- |
| `images/live/stage1_forest_terrain_fixed.png` | Stage 1 숲 입구의 실제 플레이 화면 |
| `images/live/stage1_leaf_unlocked_play.png` | Stage 1 로컬 분할 화면과 나뭇잎 슬롯 HUD. 능력 해금 테스트 화면이며 투척 장면은 아님 |
| `images/live/stage2_maze_control_play.png` | Stage 2 수액 미로 조종 중 실제 플레이 화면. 오프라인 테스트 훅으로 능력을 해금하고 조종을 시작함 |
| `images/live/stage2_maze_control_crop.png` | 위 Game View 캡처에서 미로 영역만 잘라낸 이미지 |
| `images/live/stage1_movement_20261019_jump.png` | 2026-10-19 Stage 1 플레이어 점프 중 Game View 원본 |
| `images/live/stage1_movement_20261019_tech_crop.png` | 위 점프 화면의 기술 페이지용 자르기본 |
| `images/live/stage2_maze_20261019_trail.png` | 2026-10-19 새 모델링의 미로에서 수액 물길을 만든 Game View 원본 |
| `images/live/stage2_maze_20261019_tech_crop.png` | 위 물길 화면의 기술 페이지용 자르기본 |
| `images/00_overview_gameplay.png` | 위 Stage 1·2 원본 두 장을 자른 포트폴리오용 합성. 새 장면을 그리거나 생성하지 않음 |

기술 페이지 `10`~`12`는 구조 도식과 게임 캡처를 배치한 이미지다. `10`과 `12`는 2026-10-19에 새로 촬영한 화면을, `11`은 `images/captures/`에 보관한 기존 캡처를 사용한다. `10`의 오프라인 점프 화면은 이동 동작을 보여주며 네트워크 예측·재시뮬레이션 자체의 증거로 사용하지 않는다. `12`의 도식은 현재 플레이 씬에서 사용하는 `SapMazeSurface` 메시 수로 이동에 맞췄다. 구조 도식 원본은 `02_client_prediction.png`~`05_stage_transition.png`이다. 씬 전환 페이지는 전환 전후의 새 플레이 캡처가 없어 합성하지 않았다.

Stage 3는 **폭포**로 기획이 확정됐지만 현재 저장소에 플레이 가능한 폭포 씬이 없어 화면을 넣지 않았다. 개요에서 요청한 **두 캐릭터가 동시에 각자 능력을 사용하는 게임 장면**도 이번 오프라인 Play 캡처로는 확보하지 못했다. 해당 장면은 Host/Client 2인 세션에서 촬영해야 한다. 현재 개요 이미지는 Stage 1·2의 실제 화면만 보여준다.

`build_images.py`는 게임 캡처 개요와 구조 도식을, `make_slide_pairs.py`는 기술 페이지용 배치를 만든다. 두 스크립트는 Pillow와 Windows 맑은 고딕을 사용한다.
