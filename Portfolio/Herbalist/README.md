# HERBALIST 포트폴리오 이미지

16:9 포트폴리오 페이지에 넣을 이미지 세트. **게임 캡처**, **AI 연출 이미지**, **구조 도식**의 출처를 구분한다. 현재 3스테이지는 **폭포**이며 제단 프로토타입 화면은 사용하지 않는다.

| 파일 | 용도 | 출처 |
| --- | --- | --- |
| `images/00_overview_abilities_concept.png` | 개요: 두 캐릭터가 각각 나뭇잎·물 능력을 사용하는 장면 | 2026-10-17 Stage 1 게임 캡처를 참조해 이미지 생성 도구로 만든 **연출 이미지**. 실제 게임 화면이 아님 |
| `images/01_gameplay_capture_collage.png` | 개요: 세 스테이지 이미지 | Stage 1·2 실제 게임 캡처 + Stage 3 폭포 연출 이미지. 오른쪽 아래에 구분 표기 |
| `images/06_waterfall_concept.png` | Stage 3 폭포 참고 이미지 | Stage 1 캡처를 참조해 이미지 생성 도구로 만든 **연출 이미지**. 실제 Stage 3 화면이 아님 |
| `images/10_movement_with_gameplay.png` | 이동·입력 예측 기술 페이지 | 구조 도식 + 2026-10-05 두 플레이어 게임 캡처 |
| `images/11_leaf_with_gameplay.png` | 나뭇잎 능력 기술 페이지 | 구조 도식 + 실제 나뭇잎 HUD 게임 캡처 |
| `images/12_maze_with_gameplay.png` | 물 미로 기술 페이지 | 구조 도식 + 2026-10-05 Stage 2 프로토타입 게임 캡처 |
| `images/13_transition_with_gameplay.png` | 씬 전환 기술 페이지 | 구조 도식 + Stage 1·2 실제 게임 캡처 + Stage 3 폭포 연출 이미지 |

`images/captures/`에는 사용한 게임 캡처 원본을 변형 없이 복사했다. `images/stage01_live_game_full.png`도 2026-10-17 Unity Play 모드에서 촬영한 원본이다. 이 Stage 1 캡처는 최신 모델링 브랜치를 합치기 **전** 화면이므로 현재 배치의 최종 모습으로 설명하지 않는다. Unity 에디터 또는 실행 빌드가 현재 연결되지 않아 이번 작업에서 새 라이브 캡처는 촬영하지 못했다.

구조 도식의 원본은 `02_client_prediction.png`부터 `05_stage_transition.png`까지다. `build_images.py`로 도식과 개요 콜라주를, `make_slide_pairs.py`로 도식과 게임 캡처를 결합한 16:9 기술 페이지를 다시 만든다. 두 스크립트는 Python Pillow와 Windows 맑은 고딕을 사용한다.

이미지 생성 도구에 준 핵심 요청은 다음과 같다.

- 개요: 실제 Stage 1 캡처의 두 캐릭터·숲 스타일을 참고해, 한 캐릭터는 초록 나뭇잎을 투척·설치하고 다른 캐릭터는 푸른 물줄기를 기믹에 주입하는 한 장면. UI와 글자는 제외.
- 폭포: 같은 캐릭터와 게임 스타일을 참고해, 두 사람이 폭포 양옆 발판에서 협동하는 3스테이지 참고 장면. 제단·전투·UI·글자는 제외.

최종 PDF에 넣을 때 연출 이미지에는 **‘연출 이미지’ 또는 ‘Concept Art’** 캡션을 유지한다. 실제 캡처로 대체할 때는 해당 캡션과 입력 경로를 함께 갱신한다.
