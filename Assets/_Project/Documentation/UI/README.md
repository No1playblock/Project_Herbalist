# Shared UGUI

The active menu is the authored `PF_MainMenuScreen` in MainMenu. Game overlay and player HUD prefabs are authored before play and toggled with SetActive. The active play route starts at `Stage_01_ModelingReference`, then continues to `Stage_02_Mazes` and `Stage_03_Altar`; `Sandbox_Abilities` remains for solo ability testing. Inactive legacy menu and HUD roots were removed on 2026-09-30.

`Stage_01_ModelingReference` includes the Stage 1 `PlayScreenController` and `GameplayCursor`, the shared `PF_GameOverlayHud`, one `PF_PlayerHud` per spawn slot, and an `EventSystem` with the Input System UI module. The cursor captures and hides while the Game view has focus, and releases for UI modes or when the release action is used. HUD roots are authored in the scene/prefabs and remain presentation-only.
