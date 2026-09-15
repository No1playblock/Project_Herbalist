# Temporary play-test mode menu

`Assets/Scenes/MainMenu.unity` starts with an authored UGUI mode selection panel.
Single Player loads the scene configured on PrototypeModeMenu directly without starting a Photon runner. Multiplayer reveals the existing LobbyCard. Back to Modes is available only without an active connection. Existing room creation, joining, waiting and error handling remain in LobbyScreen/FusionLobbySession.

## Removing the solo menu later

When asked to remove solo play, remove the temporary entry only unless explicitly asked to remove offline Editor testing too:

1. In MainMenu, remove `MainMenuCanvas/PrototypeModeMenu` (component plus ModeSelectionPanel and its single/multiplayer buttons).
2. Remove `MainMenuCanvas/LobbyCard/BackToModeSelection`, whose runtime click listener is owned by PrototypeModeMenu.
3. Set `MainMenuCanvas/LobbyCard` active in the saved scene. Keep LobbyScreen on MainMenuCanvas and preserve EntryPanel/ProgressPanel/ErrorPanel, their references and the existing error Back button.
4. Delete `Assets/Scripts/Networking/PrototypeModeMenu.cs` and its meta after no scenes/prefabs reference it. Remove this document if no longer useful.
5. Keep MainMenu and PlayerMovementPrototype in Build Settings: multiplayer still loads the gameplay scene. Keep LobbySettings, FusionLobbySession and all room UI.
6. Keep `HasNetworkSession` and the PlayerSpawnLayout offline guard. They let a directly opened gameplay scene work offline; an idle lobby singleton must not disable its player. Do not remove the offline Player or Q test switch just because the menu entry is being removed.
7. Verify MainMenu opens directly to room entry, room create/join/wait/error/leave flows still work, two peers spawn without an extra offline player, and direct Editor gameplay still works.

## Validation

The menu controller prevents repeat single-load clicks and mode switching while connecting, waiting or leaving. All UI exists before Play and visibility changes through SetActive. No runtime UI is instantiated.
