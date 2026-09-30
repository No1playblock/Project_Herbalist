# Removing the temporary solo menu entry

The active authored UGUI is `PF_MainMenuScreen.prefab` in MainMenu. The inactive legacy menu was removed on 2026-09-30. Solo currently opens the configured test scene and remains useful for direct ability checks.

When asked to remove solo play later, remove `Home/SoloTest` from `PF_MainMenuScreen.prefab`, its listener and `MainMenuScreen.Solo`, and `GameUiSettings.soloScene` if unused. Keep room create/join, character selection, Host Start, and direct Editor/offline test guards unless the request also covers offline tests. Recheck MainMenu and two-peer Fusion after the edit. All UI must remain preauthored and toggled via SetActive.
