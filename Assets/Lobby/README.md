# Lobby connection prototype

Start with Assets/Scenes/MainMenu.unity (first enabled build scene).
All UGUI panels, buttons, labels and input fields are authored in this scene. LobbyScreen only switches their active state and updates text.

## Flow

1. On computer A, enter a room name and choose CREATE ROOM.
2. Host stays in MainMenu with 1 / 2 connected.
3. On computer B (or a standalone build), enter the same room name and choose JOIN ROOM.
4. At two active players, the host closes the room and calls Fusion LoadScene for PlayerMovementPrototype. The client follows via NetworkSceneManagerDefault.
5. Waiting players can leave. Invalid names and failed connections show an error panel with BACK. Retry uses a fresh transport runner.

PlayerMovementPrototype now uses PlayerSpawnLayout to disable the offline player during sessions and spawn two predicted NetworkPlayers. Movement, Space jump and body/head state are synchronized; see Assets/PlayerPrototype/README.md.

## Configuration

Assets/Lobby/LobbySettings.asset controls room size, scene paths, room-name limit, connection timeout, region, app version and status messages.
Room names are trimmed and normalized to lowercase; supported characters are a-z, 0-9, - and _.
Both peers must use the same App ID, region and app version. Region currently defaults to asia so peers do not independently choose different regions.
Enter a Fusion 2 App ID in Assets/Photon/Fusion/Resources/PhotonAppSettings.asset.
Run In Background is enabled for desktop window switching.

FusionLobbySession persists across scenes and owns the transport lifecycle; LobbyScreen subscribes to it. FusionRunner.prefab contains only NetworkRunner and NetworkSceneManagerDefault; it is the only runtime-instantiated application prefab in this flow, and has no UI.
Host migration and rejoining an already started game are outside this prototype. Host loss returns the client to the main menu with an error.

## Verification on 2026-09-09

- C# compilation checked.
- Main UGUI and error panel screenshots inspected at 16:9.
- Actual UGUI button events exercised: invalid room validation, host connection attempt, failure panel and BACK.
- Initial attempt without App ID returned InvalidAuthentication. After the user supplied the App ID, authentication and room creation succeeded.
- Real two-process test passed: Unity Editor Host stayed in MainMenu at 1/2; a Windows Development Build joined via Photon; both peers reached PlayerMovementPrototype with two active players.
- Host shutdown returned the host to MainMenu and the client to its error panel. Joining a nonexistent room returned GameNotFound with zero players; BACK restored the entry panel.
- Windows Development Build succeeded with zero build errors. Temporary test client was terminated after verification. Tests were on one machine through Photon; separate-machine latency testing remains future work.
- Existing Unity Package Manager ScriptableSingleton errors are unrelated to lobby compilation.

For automated two-process validation, a Development Build accepts -herbalist-test-join ROOM_NAME. This uses the same lobby service; the entry point is excluded from release builds.
