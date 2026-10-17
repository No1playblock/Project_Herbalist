from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
import math

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent / "images"
OUT.mkdir(parents=True, exist_ok=True)

FONT = r"C:\Windows\Fonts\malgun.ttf"
BOLD = r"C:\Windows\Fonts\malgunbd.ttf"
BG = (247, 248, 247)
INK = (26, 31, 30)
MUTED = (92, 104, 100)
GREEN = (96, 247, 135)
DARK_GREEN = (25, 83, 58)
LINE = (207, 216, 211)
BLUE = (64, 129, 230)
ORANGE = (238, 154, 64)


def font(size, bold=False):
    return ImageFont.truetype(BOLD if bold else FONT, size)


def txt(draw, xy, value, size=27, fill=INK, bold=False, anchor=None):
    draw.text(xy, value, font=font(size, bold), fill=fill, anchor=anchor)


def rounded(draw, box, fill="white", outline=LINE, radius=24, width=2):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)


def arrow(draw, a, b, fill=INK, width=5, head=15):
    draw.line([a, b], fill=fill, width=width)
    angle = math.atan2(b[1] - a[1], b[0] - a[0])
    p1 = (b[0] - head * math.cos(angle - 0.5), b[1] - head * math.sin(angle - 0.5))
    p2 = (b[0] - head * math.cos(angle + 0.5), b[1] - head * math.sin(angle + 0.5))
    draw.polygon([b, p1, p2], fill=fill)


def base(title, subtitle, number):
    im = Image.new("RGB", (960, 1200), BG)
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, 960, 13), fill=GREEN)
    txt(d, (54, 48), f"HERBALIST  /  {number}", 23, DARK_GREEN, True)
    txt(d, (54, 99), title, 45, INK, True)
    txt(d, (55, 167), subtitle, 23, MUTED)
    d.line((55, 222, 905, 222), fill=LINE, width=2)
    return im, d


def card(d, box, label, lines, accent=GREEN, label_size=29, body_size=24):
    rounded(d, box)
    x1, y1, x2, y2 = box
    d.rounded_rectangle((x1 + 20, y1 + 20, x1 + 31, y2 - 20), radius=5, fill=accent)
    txt(d, (x1 + 51, y1 + 20), label, label_size, INK, True)
    for i, line in enumerate(lines):
        txt(d, (x1 + 51, y1 + 68 + i * 38), line, body_size, MUTED)


def fit_crop(path, size, focus=(0.5, 0.5)):
    im = Image.open(path).convert("RGB")
    return ImageOps.fit(im, size, method=Image.Resampling.LANCZOS, centering=focus)


def hero():
    a = ROOT / "Temp/PortfolioCaptures/stage01_live_game_full.png"
    b = ROOT / "Assets/_Project/Documentation/UI/Screenshots/06-stage02-hud-1920x1080.png"
    c = ROOT / "Assets/_Project/Documentation/UI/Screenshots/07-stage03-altar-hud-1920x1080.png"
    out = Image.new("RGB", (1600, 900), INK)
    out.paste(fit_crop(a, (1050, 900), (0.52, 0.55)), (0, 0))
    out.paste(fit_crop(b, (540, 440), (0.51, 0.52)), (1060, 0))
    out.paste(fit_crop(c, (540, 450), (0.51, 0.42)), (1060, 450))
    d = ImageDraw.Draw(out, "RGBA")
    for box in [(0, 790, 1050, 900), (1060, 351, 1600, 440), (1060, 805, 1600, 900)]:
        d.rectangle(box, fill=(12, 21, 17, 222))
    d.rectangle((1050, 0, 1060, 900), fill=GREEN)
    d.rectangle((1060, 440, 1600, 450), fill=GREEN)
    txt(d, (38, 803), "01  EXTERIOR · 약초와 나무 등반", 35, "white", True)
    txt(d, (1088, 367), "02  MAZE · 물 경로 조종", 28, "white", True)
    txt(d, (1088, 820), "03  ALTAR · 회전 제단", 28, "white", True)
    out.save(OUT / "01_gameplay_capture_collage.png", optimize=True)


def movement():
    im, d = base("입력 예측과 재시뮬레이션", "NetworkPlayer / CharacterControllerMotor", "01")
    rounded(d, (55, 250, 445, 318), fill=(224, 244, 234), outline=(224, 244, 234))
    rounded(d, (515, 250, 905, 318), fill=(226, 237, 251), outline=(226, 237, 251))
    txt(d, (250, 284), "INPUT AUTHORITY", 28, DARK_GREEN, True, "mm")
    txt(d, (710, 284), "STATE AUTHORITY", 28, (37, 86, 153), True, "mm")
    card(d, (55, 348, 445, 497), "입력 수집", ["Move / LookAngles", "JumpSequence 증가"], GREEN, 28, 23)
    card(d, (515, 348, 905, 497), "같은 Tick 시뮬레이션", ["권한 상태로 판정", "MotorState 갱신"], BLUE, 27, 23)
    arrow(d, (447, 419), (512, 419), ORANGE, 6)
    rounded(d, (55, 537, 905, 858))
    txt(d, (89, 563), "FixedUpdateNetwork", 32, INK, True)
    steps = [
        ("01", "RestoreState", "Position / Velocity / Grounded / ExternalFlight"),
        ("02", "Simulate", "CharacterController.Move + 입력 방향 / 중력"),
        ("03", "CaptureState", "다음 Tick과 재시뮬레이션에 사용할 상태 저장"),
    ]
    for i, (n, title, detail) in enumerate(steps):
        y = 630 + i * 75
        d.ellipse((89, y - 5, 143, y + 49), fill=GREEN)
        txt(d, (116, y + 21), n, 21, INK, True, "mm")
        txt(d, (165, y - 5), title, 27, INK, True)
        txt(d, (165, y + 31), detail, 22, MUTED)
        if i < 2:
            arrow(d, (116, y + 51), (116, y + 73), LINE, 4, 9)
    rounded(d, (55, 893, 905, 1119), fill=(28, 37, 34), outline=(28, 37, 34))
    txt(d, (89, 922), "점프 중복 방지", 31, GREEN, True)
    txt(d, (89, 973), "JumpSequence != LastJumpSequence", 30, "white", True)
    txt(d, (89, 1027), "마지막 소비 번호를 [Networked] 상태에 저장", 25, (220, 228, 224))
    txt(d, (89, 1069), "재시뮬레이션에서도 한 번의 입력만 처리", 25, (220, 228, 224))
    im.save(OUT / "02_client_prediction.png", optimize=True)


def leaf():
    im, d = base("나뭇잎 투척의 권한 분리", "NetworkLeafAbility / NetworkLeafProjectile", "02")
    card(d, (55, 256, 905, 386), "CLIENT → HOST  |  입력만 전달", ["AbilityUse 시퀀스 + LookAngles", "Client가 선택한 충돌 대상은 보내지 않음"], BLUE, 29, 24)
    arrow(d, (480, 390), (480, 425), ORANGE, 5)
    card(d, (55, 431, 905, 587), "HOST  |  투척·충돌·설치 판정", ["권한 위치에서 Aim Ray 재구성 → SphereCast", "LeafInstallTarget의 ID·소켓·점유 조건 확인"], GREEN, 29, 24)
    arrow(d, (480, 590), (480, 625), ORANGE, 5)
    card(d, (55, 633, 905, 789), "NETWORK STATE  |  결과만 복제", ["LeafState + TargetId + NetworkTransform", "3개 HUD 슬롯 × 2비트 상태"], ORANGE, 29, 24)
    arrow(d, (480, 792), (480, 826), ORANGE, 5)
    card(d, (55, 833, 905, 987), "CLIENT  |  ApplyReplica", ["TargetId로 설치 대상 검색", "외형·발판 Collider·설치/해제 반응 갱신"], BLUE, 29, 24)
    rounded(d, (55, 1018, 905, 1126), fill=(28, 37, 34), outline=(28, 37, 34))
    txt(d, (86, 1042), "WALL", 25, GREEN, True)
    txt(d, (86, 1082), "표면 법선 + 조준 방향", 24, "white")
    d.line((450, 1033, 450, 1110), fill=(112, 128, 118), width=2)
    txt(d, (485, 1042), "SOCKET", 25, GREEN, True)
    txt(d, (485, 1082), "작성된 위치 + 회전값", 24, "white")
    im.save(OUT / "03_leaf_replication.png", optimize=True)


def maze():
    im, d = base("그래프 미로와 물길 복제", "SapMazeSimulation / SapMazeBoard", "03")
    rounded(d, (55, 255, 905, 676))
    txt(d, (88, 279), "그래프 기반 이동", 30, INK, True)
    pts = [(155, 400), (335, 400), (515, 400), (515, 560), (745, 560)]
    for i in range(len(pts) - 1):
        d.line((pts[i], pts[i + 1]), fill=(143, 172, 151), width=13)
    d.line((pts[1], pts[2]), fill=BLUE, width=15)
    for i, p in enumerate(pts):
        r = 30
        d.ellipse((p[0] - r, p[1] - r, p[0] + r, p[1] + r), fill=BLUE if i in (1, 2) else GREEN, outline=INK, width=3)
        txt(d, p, str(i), 25, "white" if i in (1, 2) else INK, True, "mm")
    d.ellipse((499, 544, 531, 576), outline=ORANGE, width=6)
    txt(d, (573, 348), "From=1  →  To=2", 25, INK, True)
    txt(d, (573, 389), "Progress=0.6", 25, BLUE, True)
    txt(d, (575, 602), "누수 노드", 24, ORANGE, True)
    rounded(d, (55, 710, 905, 862), fill=(229, 241, 252), outline=(196, 220, 243))
    txt(d, (88, 735), "HOST SIMULATION", 28, (37, 86, 153), True)
    txt(d, (88, 782), "Tick(dt) → 소단계 분할 → 이동·누수·완료 판정", 26, INK)
    txt(d, (88, 821), "From / To / Progress / Volume / Phase 복제", 24, MUTED)
    rounded(d, (55, 890, 905, 1124), fill=(28, 37, 34), outline=(28, 37, 34))
    txt(d, (88, 916), "CLIENT PRESENTATION", 28, GREEN, True)
    txt(d, (88, 968), "TrailPoints[128] 순환 버퍼", 28, "white", True)
    txt(d, (88, 1018), "TrailFirst + TrailCount로 샘플 순서 복원", 24, (220, 228, 224))
    txt(d, (88, 1062), "Volume에 따라 물방울·물길 크기 표시", 24, (220, 228, 224))
    im.save(OUT / "04_maze_simulation.png", optimize=True)


def transition():
    im, d = base("씬 전환 시 플레이어 상태 보존", "FusionLobbySession / StageTransition", "04")
    labels = [
        ("01", "Host 완료 조건 확인", "두 플레이어 활성·능력 해금 확인"),
        ("02", "PlayerRef별 스냅샷", "Slot + PlayerAbilityKind 저장"),
        ("03", "기존 플레이어 Despawn", "씬 오브젝트와 입력 소유권 정리"),
        ("04", "NetworkRunner.LoadScene", "Host가 다음 Stage Scene 로드"),
        ("05", "Spawn + 능력 복원", "같은 PlayerRef에 Slot·Ability 적용"),
    ]
    for i, (n, title, detail) in enumerate(labels):
        y = 258 + i * 173
        card(d, (55, y, 905, y + 132), f"{n}  {title}", [detail], GREEN if i in (0, 4) else BLUE, 28, 24)
        if i < 4:
            arrow(d, (480, y + 136), (480, y + 166), ORANGE, 5)
    im.save(OUT / "05_stage_transition.png", optimize=True)


if __name__ == "__main__":
    hero()
    movement()
    leaf()
    maze()
    transition()
    print("Created", len(list(OUT.glob("*.png"))), "images in", OUT)
