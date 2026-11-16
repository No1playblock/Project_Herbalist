"""Refresh the listening page from the serialized Unity catalog; no Unity edits."""
import ast
import json
import re
import shutil
from pathlib import Path
from urllib.parse import quote

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
asset = (ROOT / 'Assets/_Project/Data/Audio/SO_GameSfx.asset').read_text(encoding='utf-8-sig')
paths = {}
for meta in (ROOT / 'Assets/_Project/Audio/SFX').rglob('*.meta'):
    match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M)
    audio = meta.with_suffix('')
    if match and audio.suffix.lower() in ('.wav', '.mp3', '.ogg'):
        paths[match[1]] = audio

conditions = {
1:'UGUI 버튼 클릭',2:'버튼에 마우스를 올리거나 키보드 포커스 이동',3:'뒤로·취소·닫기 버튼 클릭',
4:'로비 대기 중 참가 인원 증가',5:'로비에서 로컬 캐릭터 선택 변경',6:'로비 준비 완료 상태 진입',
7:'StageLevel이 있는 스테이지 시작',8:'도움말 패널 열기 · 원본에서 1.3초 추출',9:'플레이어 등장 또는 체크포인트 순간 이동',
10:'모든 바닥에서 걷기 · 흙 발소리 6종으로 통일',11:'현재 걷기에는 미사용 · 흙 발소리(10번)로 대체',
12:'현재 걷기에는 미사용 · 흙 발소리(10번)로 대체',13:'소담 일반 점프 이륙',14:'두영 일반 점프 이륙',
15:'흙에 착지 · 약한 착지는 light 음원',16:'목재·나무껍질에 착지 · 약한 착지는 light 음원',17:'게임에서 꺼짐 · 원본 비교 청음만 가능',
18:'채집 미니게임 시작 · 소담 도감 펼치기',19:'채집 미니게임 시작 · 두영 잎 살펴보기',20:'로컬 채집 방향 입력',
21:'채집 성공 시 약초를 뽑는 소리',22:'채집 성공 결과 알림 · 21번과 함께 재생',23:'채집 실패 결과 알림',
24:'약 제조 중 화로 불',25:'제조 중 온도 임계값 이상 · v4 채택 음원',26:'소담의 부채질 입력마다',27:'약 제조 실패 결과',28:'약 제조 성공 결과',29:'약 복용 상태 전환 · 제공된 v4 세 모금',
31:'물줄기가 유지되는 동안 · 중단·회수 시 정지',32:'물의 첫 충돌 또는 착지',33:'나뭇잎이 발사되어 비행 상태 진입',
34:'나뭇잎이 날아가는 동안 · 설치·회수 시 정지',35:'나뭇잎 설치 상태 진입',36:'나뭇잎 회수 상태 진입',
37:'Stage 1 외부 숲 공간 배경음',38:'매달린 천이 움직일 때',39:'점프대 수액 충전 증가 · 제목 기준 주입음',
40:'점프대 분출 시작 · 제목 기준 작동음',41:'잎이 설치된 점프대가 분출할 때',42:'미로 수액이 실제로 이동할 때',
43:'로컬 미로 조작 중 포인터 조작 불가',44:'막히지 않은 미로 누수',45:'발판 하강 단계 시작',46:'하강 시작 시 장력음 1회 · 반복하지 않음',47:'발판 하강 단계 종료',
48:'별도 가지 씬 · 부적 탑승',49:'별도 가지 씬 · 부적이 움직이는 동안',50:'별도 가지 씬 · 부적 하차',
51:'별도 가지 씬 · 가지 과녁 상태 변경',52:'가지 방향 변경 · 최종 제단 회전에도 낮은 음량으로 재사용',
53:'별도 가지 씬 · 낙석 등장',54:'별도 가지 씬 · 낙석이 가지에 접촉',55:'돌이 접촉한 채 실제 수평 이동할 때만 · 현재 수직 낙석에서는 들리지 않을 수 있음',
56:'별도 가지 씬 · 낙석 접촉 해제 또는 낙하 후 사라짐',57:'별도 가지 씬 · 잎에 의한 낙석 차단 해제 상태 전환',
58:'현재 최종 제단 출구 활성화 · 신비 약초 획득 동작은 현재 없음'}
fallbacks = {5:4,6:1,9:4,43:3,58:8}
def group(n):
    if n <= 9: return 'UI·로비'
    if n <= 17: return '캐릭터'
    if n <= 29: return '채집·제조'
    if n <= 36: return '물·나뭇잎'
    if n <= 41: return 'Stage 1 환경'
    if n <= 47: return 'Stage 2 미로'
    return '가지·제단'
def clip(guid, light=False):
    p = paths[guid]
    assert p.is_file(), p
    media = HERE / 'audio'
    media.mkdir(exist_ok=True)
    target = media / (guid + p.suffix.lower())
    if not target.exists() or target.stat().st_size != p.stat().st_size:
        shutil.copy2(p, target)
    return {'name':p.name, 'url':quote('audio/' + target.name, safe='/'), 'light':light}

items=[]
for block in re.split(r'^  - id: ', asset, flags=re.M)[1:]:
    n=int(block.splitlines()[0])
    def field(key):
        m=re.search(r'^    '+key+r':\s*([^\r\n]*)',block,re.M)
        return m.group(1).strip() if m else ''
    label=field('label')
    if label.startswith('"'): label=ast.literal_eval(label)
    clips_section=block.split('    clips:',1)[1].split('    lightClip:',1)[0]
    clips=[clip(g) for g in re.findall(r'guid: (\w+)',clips_section)]
    light=re.search(r'guid: (\w+)',field('lightClip'))
    if light: clips.append(clip(light.group(1),True))
    items.append({'id':n,'label':label,'group':group(n),'condition':conditions[n],
                  'loop':field('loop')=='1','fallback':fallbacks.get(n),
                  'inactive':n in (11,12,17),'volume':float(field('volume')),'clips':clips})
assert len(items)==57 and sum(bool(i['fallback']) for i in items)==5
page=HERE/'index.html'
html=page.read_text(encoding='utf-8')
data=json.dumps(items,ensure_ascii=False).replace('<','\\u003c')
html=re.sub(r'(?<=<!-- SFX_DATA_START -->).*?(?=<!-- SFX_DATA_END -->)',
            lambda _: '\n<script id="sfx-data" type="application/json">'+data+'</script>\n',html,flags=re.S)
page.write_text(html,encoding='utf-8')
print(f'Built {page}: {len(items)} cues, {sum(len(i["clips"]) for i in items)} clip choices; audio references verified.')
