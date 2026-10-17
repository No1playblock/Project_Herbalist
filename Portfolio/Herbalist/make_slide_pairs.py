"""Build 16:9 portfolio visuals from authored diagrams and traceable source images."""

from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps

HERE = Path(__file__).resolve().parent
OUT = HERE / "images"
CAP = OUT / "captures"
FONT = Path(r"C:\Windows\Fonts\malgun.ttf")
BOLD = Path(r"C:\Windows\Fonts\malgunbd.ttf")


def font(size, bold=False):
    return ImageFont.truetype(str(BOLD if bold else FONT), size)


def text(draw, xy, label, size, color, bold=False):
    draw.text(xy, label, font=font(size, bold), fill=color)


def image_panel(canvas, source, box, label, tag="GAME CAPTURE"):
    x0, y0, x1, y1 = box
    with Image.open(source) as src:
        focus = (0.5, 0.78) if source.name == "stage01_live_game_full.png" else (0.5, 0.5)
        fitted = ImageOps.fit(src.convert("RGB"), (x1 - x0, y1 - y0), method=Image.Resampling.LANCZOS, centering=focus)
    canvas.paste(fitted, (x0, y0))
    d = ImageDraw.Draw(canvas, "RGBA")
    compact = y1 - y0 < 200
    d.rectangle((x0, y1 - (40 if compact else 58), x1, y1), fill=(11, 25, 21, 225))
    text(d, (x0 + 18, y1 - (36 if compact else 50)), label, 22 if compact else 26, "white", True)
    text(d, (x1 - 186, y1 - (29 if compact else 42)), tag, 14 if compact else 16, (167, 250, 190), True)


def make(name, title, diagram, panels, note):
    im = Image.new("RGB", (1600, 900), (247, 248, 247))
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, 1600, 12), fill=(96, 247, 135))
    text(d, (48, 33), title, 39, (26, 31, 30), True)
    d.line((48, 100, 1552, 100), fill=(207, 216, 211), width=2)
    with Image.open(OUT / diagram) as src:
        thumb = ImageOps.contain(src.convert("RGB"), (610, 742), method=Image.Resampling.LANCZOS)
    im.paste(thumb, (48, 119))
    d.rounded_rectangle((682, 119, 1552, 861), radius=18, fill="white", outline=(207, 216, 211), width=2)
    if len(panels) == 1:
        source, label, tag = panels[0]
        image_panel(im, source, (700, 138, 1534, 606), label, tag)
    else:
        h = 150
        for i, (source, label, tag) in enumerate(panels):
            y = 138 + i * 156
            image_panel(im, source, (700, y, 1534, y + h), label, tag)
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((700, 630, 1534, 840), radius=14, fill=(28, 37, 34)) if len(panels) == 1 else None
    note_y = 654 if len(panels) == 1 else 630
    for line in note:
        text(d, (728, note_y), line, 25, (238, 244, 240) if len(panels) == 1 else (54, 67, 60), False)
        note_y += 42
    im.save(OUT / name, optimize=True)


if __name__ == "__main__":
    make(
        "10_movement_with_gameplay.png", "입력 예측과 재시뮬레이션",
        "02_client_prediction.png",
        [(CAP / "stage1_two_players_raw.png", "Host / Client 2인 화면", "GAME CAPTURE")],
        ["클라이언트의 로컬 입력을 먼저 시뮬레이션", "Host 상태를 받아 같은 Tick을 재시뮬레이션", "JumpSequence로 점프 입력 중복 소비 방지"],
    )
    make(
        "11_leaf_with_gameplay.png", "나뭇잎 능력의 권한 분리",
        "03_leaf_replication.png",
        [(CAP / "stage1_leaf_hud_raw.png", "나뭇잎 슬롯과 게임 HUD", "GAME CAPTURE")],
        ["Host가 충돌 대상과 설치 위치 확정", "Client는 복제 상태로 외형·발판 표시", "실제 캡처는 능력 HUD 중심 장면"],
    )
    make(
        "12_maze_with_gameplay.png", "물 미로 시뮬레이션과 표현",
        "04_maze_simulation.png",
        [(OUT / "live/stage2_maze_control_crop.png", "물 미로 조종 화면", "GAME CAPTURE")],
        ["Host는 경로·누수·잔량 계산", "Client는 복제 상태로 물길을 그림", "실제 게임 화면은 Stage 2 프로토타입 캡처"],
    )
    print("Built three 1600×900 technical page visuals")
