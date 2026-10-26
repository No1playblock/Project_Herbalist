"""Capture an unmodified frame from a running two-player Windows build."""

import ctypes
import ctypes.wintypes
import time
from pathlib import Path

from PIL import ImageGrab


OUT = Path(__file__).resolve().parent / "images" / "live"
USER32 = ctypes.windll.user32
SW_RESTORE = 9
VK_SPACE = 0x20
KEYEVENTF_KEYUP = 0x0002


def window_for_pid(pid):
    windows = []
    callback = ctypes.WINFUNCTYPE(ctypes.c_bool, ctypes.c_void_p, ctypes.c_void_p)

    def visit(hwnd, _):
        process_id = ctypes.c_ulong()
        USER32.GetWindowThreadProcessId(hwnd, ctypes.byref(process_id))
        if process_id.value == pid and USER32.IsWindowVisible(hwnd):
            windows.append(hwnd)
        return True

    USER32.EnumWindows(callback(visit), 0)
    if not windows:
        raise RuntimeError(f"No visible game window for PID {pid}")
    return windows[0]


def activate(hwnd):
    USER32.ShowWindow(hwnd, SW_RESTORE)
    USER32.SetForegroundWindow(hwnd)
    time.sleep(0.2)


def capture(hwnd, name):
    rect = ctypes.wintypes.RECT()
    USER32.GetWindowRect(hwnd, ctypes.byref(rect))
    image = ImageGrab.grab(bbox=(rect.left, rect.top, rect.right, rect.bottom), all_screens=True)
    path = OUT / name
    image.save(path)
    print(path, image.size, flush=True)


if __name__ == "__main__":
    import sys
    host = window_for_pid(int(sys.argv[1]))
    client = window_for_pid(int(sys.argv[2]))
    OUT.mkdir(parents=True, exist_ok=True)
    activate(host)
    capture(host, "stage1_multiplayer_before_jump.png")
    activate(client)
    USER32.keybd_event(VK_SPACE, 0, 0, 0)
    time.sleep(0.1)
    activate(host)
    for index in range(12):
        capture(host, f"stage1_multiplayer_jump_{index:02}.png")
        time.sleep(0.045)
    USER32.keybd_event(VK_SPACE, 0, KEYEVENTF_KEYUP, 0)
