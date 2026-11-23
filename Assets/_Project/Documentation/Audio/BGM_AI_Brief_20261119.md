# 초유록 BGM 제작 브리프

작성일: 2026-11-19  
용도: 음악 생성 AI 입력 및 후보 음원 평가  
상태: 음악 방향과 수치는 제작 제안이며, 최종 확정 전 청취·플레이 검수가 필요하다.

## 1. 게임 소개

두 플레이어가 두영과 소담으로 협력해 약초를 채집하고 물약을 만든 뒤, 나뭇잎과 물 능력을 활용해 거대한 나무와 그 내부를 탐험하는 협동 어드벤처·퍼즐 게임이다. 한 명이 길을 만들거나 장치를 조작하면 다른 한 명이 이를 활용한다. 플레이의 중심은 전투보다 탐색, 관찰, 의사소통, 시행착오와 협력이다.

현재 플레이 경로는 메인 메뉴 → 1스테이지 숲과 나무 등반 → 전환 연출 → 2스테이지 하강 수액 미로 → 3스테이지 부적 가지 타기다. 아래는 현재 확인된 플레이를 바탕으로 한 음악 제안이며, 스토리 세부 설정이나 악기 편성은 확정된 공식 설정이 아니다.

## 2. 전체 음악 방향

- 핵심 감정: 자연의 생명력, 작고 따뜻한 모험, 신비로움, 서로 돕는 즐거움, 발견의 성취감.
- 시각적 연상: 거대한 나무와 숲, 약초와 물약, 나뭇잎 발판, 흐르는 물, 천과 부적, 나무 내부의 장치.
- 권장 색채: 어쿠스틱 질감의 나무 타악기, 부드러운 현과 목관, 가벼운 뜯는 소리, 은은한 종과 공기감 있는 패드. 한국적인 목관·현악·타악의 음색을 섬세하게 섞는 방향을 제안한다. 특정 전통음악 양식을 고증한 결과로 간주하지 않는다.
- 멜로디: 기억하기 쉬운 짧은 공통 동기를 곡마다 느린 선율, 분산 화음, 리듬 패턴으로 변형해 같은 여정이라는 인상을 준다.
- 분위기: 귀엽기만 한 동요나 비장한 전투곡보다 따뜻하고 신비로운 모험에 가깝게. 위험 구간은 흥미와 긴장을 주되 공포나 압박을 과하게 높이지 않는다.
- 구성: 기본적으로 가사와 보컬 없이, 긴 플레이에도 피로하지 않은 반복 가능한 음악. 급격한 음량 변화와 지나치게 잦은 멜로디 반복을 피한다.
- 기존 효과음과의 관계: 숲 환경음, 발소리, 물 흐름, 천, 기계, 부적, 낙석, UI와 안내음이 들릴 공간을 남긴다. 물소리나 새소리를 음악에 크게 고정하지 않는다.

## 3. 곡 목록 및 생성 프롬프트

길이와 BPM은 생성·편집을 위한 시작점이다. AI가 정확한 길이나 매끄러운 루프를 보장하지 않을 수 있으므로 후편집과 실기 검수를 전제로 한다. 각 프롬프트는 단독으로 입력할 수 있다.

### A. 메인 메뉴 / 로비

- 목적: 게임의 세계와 두 사람의 모험을 편안하게 소개한다. 선택과 대기 시간이 길어져도 부담이 적어야 한다.
- 제안: 65–80 BPM, 90–120초 루프. 여백이 많은 도입, 따뜻한 목관과 뜯는 현, 은은한 종. 협동을 연상시키는 두 악기의 짧은 주고받기.
- 프롬프트: `Instrumental main menu music for a gentle two-player cooperative adventure in a vast magical forest. Warm wooden flute, delicate plucked strings, soft Korean-inspired acoustic colors and subtle bells. Two small melodic voices answer each other with curiosity and friendship. Spacious, intimate, quietly wondrous, 72 BPM. Understated arrangement, no vocals, no dramatic climax, designed for a seamless game loop.`

### B. 1스테이지 / 숲 탐색과 약초 채집

- 목적: 약초를 찾고, 함께 물약을 만들며, 거대한 나무 주변을 탐색하는 구간의 기본 분위기.
- 제안: 78–96 BPM, 120–180초 루프. 따뜻한 목관·현과 가벼운 나무 타악기. 자연 환경음 및 채집·제조 피드백과 겹치지 않도록 촘촘한 고음과 강한 타악은 자제한다.
- 프롬프트: `Instrumental exploration music for a cooperative adventure: two small travelers gather herbs, make potions and explore around an enormous living tree. Organic wooden flute, gentle plucked strings, light wooden percussion, subtle Korean-inspired acoustic timbres. Warm, curious, playful but calm, 86 BPM. Leave space for forest ambience and gameplay sounds; evolving phrases, no vocals, seamless looping game background.`

### C. 1스테이지 / 나무 등반

- 목적: 나뭇잎 발판, 물 분출과 매달린 천을 활용한 높은 곳으로의 협동 이동. 즐거운 고도감과 가벼운 긴장.
- 제안: 92–110 BPM, 120–180초 루프. B의 동기를 유지하면서 리듬을 조금 더 움직이게 한다. 실패 후 재시도에도 지나치게 몰아붙이지 않는다.
- 프롬프트: `Instrumental cooperative climbing music high in a giant tree, using leaves, flowing water and hanging cloth to reach the next branch. Build on a gentle forest-adventure motif with nimble plucked strings, airy flute and restrained wooden percussion. Buoyant, adventurous and slightly suspenseful, 100 BPM, not heroic battle music. Leave room for water, movement and communication; no vocals, loopable.`

### D. 1→2스테이지 / 전환 연출

- 목적: 나무 외부의 밝은 공간에서 내부로 이동하는 순간의 연결. 실제 연출 길이에 맞춰 편집할 수 있어야 한다.
- 제안: 15–30초의 비반복 전환곡, 또는 2스테이지 곡으로 자연스럽게 이어질 수 있는 짧은 스팅. 따뜻한 동기를 유지하며 공간이 깊어지는 느낌을 준다. 정확한 길이와 진입·종료 지점은 영상 확인 후 확정.
- 프롬프트: `Short instrumental transition cue for two companions entering the mysterious interior of a giant tree after a forest climb. Begin with a warm familiar flute and plucked-string motif, then gradually open into deep airy resonance and a gentle sense of descent. Wondrous rather than ominous, no vocals, restrained ending with a clean tail, approximately 20 seconds.`

### E. 2스테이지 / 하강 수액 미로

- 목적: 두 플레이어가 서로 다른 정보를 전달하며 수액과 나뭇잎으로 누수를 해결하고 미로를 통과하는 집중 구간.
- 제안: 68–84 BPM 또는 박자가 두드러지지 않는 자유로운 흐름, 150–210초 루프. 여백과 반복 가능한 은은한 음형을 중심으로 나무 내부의 깊이와 신비를 표현한다. 누수·물소리 및 대화 전달을 방해하지 않는다.
- 프롬프트: `Instrumental puzzle exploration music inside a vast ancient living tree. Two players communicate to guide water through intricate mazes and seal leaks while descending. Quiet wooden resonance, soft plucked motifs, breathy flute and spacious subtle pads, gentle Korean-inspired acoustic colors. Thoughtful, mysterious and reassuring, around 76 BPM, low rhythmic density, no ticking clock or horror, no vocals, seamless long loop; leave space for water and player communication.`

### F. 3스테이지 / 부적 가지 타기

- 목적: 두 플레이어가 함께 부적에 올라 가지 분기와 낙석을 해결하며 내려가는 구간. 움직임과 협동의 긴장감을 강조한다.
- 제안: 100–120 BPM, 120–180초 루프. 앞선 공통 동기를 리듬으로 변형하고 나무 타악기와 가벼운 현을 더한다. 분기와 돌의 효과음이 위험을 알릴 수 있도록 강한 충격음이나 가짜 낙석 소리를 넣지 않는다.
- 프롬프트: `Instrumental music for two companions riding a paper talisman along branching paths inside a giant tree, cooperating to switch routes and avoid falling stones. Lively plucked strings, agile wooden percussion, airy flute and subtle Korean-inspired acoustic textures. Forward motion, playful suspense and teamwork, 108 BPM; adventurous rather than aggressive. Keep percussion clear of gameplay warning sounds, no simulated impacts, no vocals, seamless loop.`

### G. 스테이지 완료 / 여정의 마무리

- 목적: 협동 성공을 짧고 따뜻하게 축하한다. 현재 3스테이지 완료 화면에 적용할 후보이며 최종 엔딩 연출 여부는 별도 확인한다.
- 제안: 5–10초 비반복 스팅, 공통 동기의 밝은 변형과 자연스러운 잔향. 긴 팡파르나 큰 충격음은 지양한다.
- 프롬프트: `Short instrumental success cue for two friends completing a cooperative nature adventure. Resolve a gentle flute and plucked-string motif into a warm, hopeful harmony, with a delicate bell accent. Earned and intimate rather than grand or triumphant, no vocals, clean ending and natural reverb tail, approximately 7 seconds.`

## 4. 생성 및 납품 요청

1. 먼저 A와 B를 각각 2–3개 생성해 공통 동기, 한국적인 음색의 비중과 전체 분위기를 결정한다. 선택한 결과를 참조할 수 있는 도구라면 같은 참조와 설정을 사용해 C–G를 제작한다. 도구가 참조 음원이나 시드 고정을 지원하지 않으면 프롬프트의 악기·동기 설명을 반복하고 귀로 일관성을 확인한다.
2. 각 루프는 시작·끝에서 박자, 화성, 잔향과 음량이 자연스럽게 이어져야 한다. 도입부가 루프와 다르면 별도 intro와 loop로 납품한다. 전환곡과 완료 스팅은 루프 대신 자연스러운 끝과 잔향을 남긴다.
3. 가능하면 스테레오 WAV, 48 kHz / 24-bit 원본과 사용한 생성 설정·프롬프트를 함께 보관한다. 지원 형식이 다르면 원본 사양을 기록한다. 스템이나 루프 지점 제공 가능 여부는 도구별로 확인한다.
4. 파일명 예: `BGM_Menu_v01.wav`, `BGM_Stage01_Explore_v01.wav`, `BGM_Stage01_Climb_v01.wav`, `BGM_Transition_01_02_v01.wav`, `BGM_Stage02_Maze_v01.wav`, `BGM_Stage03_BranchRide_v01.wav`, `BGM_Clear_v01.wav`.
5. 게임 배포·상업적 이용·수정·반복 재생에 필요한 권리와 사용 중인 생성 서비스의 약관을 납품 전에 확인하고, 사용 모델·계정·생성일과 조건을 기록한다. 특정 기존 곡이나 작곡가의 스타일을 직접 복제하도록 요청하지 않는다.

## 5. 청취 및 게임 내 검수

- 이어 붙여 여러 번 재생했을 때 끊김, 클릭, 화성 충돌이나 부자연스러운 잔향이 없는가?
- 10분 이상 들었을 때 주요 음형과 고음역이 피로하지 않은가?
- 실제 게임에서 발소리, 물, 누수, 가지 회전, 낙석, 안내 및 UI 효과음이 충분히 구분되는가?
- 두 플레이어의 음성 대화를 가리지 않는가? 같은 곡을 스피커와 헤드폰 양쪽에서 확인한다.
- 탐색·퍼즐·이동의 에너지가 실제 플레이 속도와 맞고, 대기나 재시도 중 지나친 압박을 주지 않는가?
- 장면 전환과 일시정지에서의 재생·페이드 방식, 음량 및 BGM 설정은 구현 시 별도로 결정하고 실제 Host/Client 플레이에서 확인한다. 이 문서는 재생 시스템 구현을 승인하거나 완료했다는 뜻이 아니다.

## 6. 참고한 프로젝트 문서

- `Assets/_Project/Documentation/Stages/Stage_01/ImportedMapGimmicks_20261103.md` — 1스테이지 기믹과 수입 맵.
- `Assets/_Project/Documentation/Stages/Stage_02/README.md` — 2스테이지 미로.
- `Assets/_Project/Documentation/Stages/Stage_03/BranchRide.md` — 부적 탑승, 분기, 낙석과 완료.
- `Assets/_Project/Documentation/Audio/README.md` — 현재 효과음과 환경음.
- 프로젝트 지시사항의 2026-11-16 씬 정리 및 현재 플레이 경로. 과거 문서에 남은 제단·구형 스테이지 경로는 현재 곡 목록의 확정 근거로 사용하지 않았다.
