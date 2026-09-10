# Player movement and networking

## Controls

- WASD: camera-relative movement; diagonal input is normalized.
- Mouse: orbit camera without holding a button; head follows within configured limits.
- Space: one grounded jump per press. Holding Space does not repeat, and airborne presses do not grant a second jump.
- Ability bindings remain undecided.

Use MainMenu for multiplayer. The host spawns one NetworkPlayer for each connected player after both peers enter PlayerMovementPrototype. Spawn points live under PlayerSpawnLayout. Green/orange materials distinguish the two slots. Each peer enables only its own input, camera and AudioListener. Leaving players are despawned by the host.

Opening PlayerMovementPrototype directly preserves the offline player for local testing. Its scene object is disabled when entering through a Fusion session.

## Configuration and ownership

PlayerTuning.asset holds move speed, gravity, jump height (1.5), air acceleration (16), camera and head settings. PlayerControls.inputactions holds keyboard/mouse bindings. NetworkPlayer.prefab contains the network components and presentation references. Player layer capsules exclude the Player layer: players do not push or block one another in this prototype.

PlayerInputReader reads private Input System actions. Jump performed events increment a sequence, which survives render frames between network input polls. Fusion stores this sequence with each input; the last consumed sequence is network state so re-simulation consumes the same jump deterministically. An airborne or blocked jump press is consumed without a later automatic jump.

PlayerController drives offline Update/LateUpdate; network mode keeps local look input and presentation but stops offline movement. FusionLobbySession submits local input. NetworkPlayer runs movement on state authority (host) and input authority (predicting client) in FixedUpdateNetwork. Proxies only render replicated state and have disabled physical controllers.

PlayerMotor remains independent of Fusion. CaptureState/RestoreState contain position, velocity and grounded state. NetworkPlayer restores these networked fields before each simulated tick, then captures the result. NetworkTransform interpolates the displayed root. Body yaw, view angles, jump sequence and movement blocking are also networked. Camera uses current local look immediately; remote head uses replicated angles.

Movement locks suppress voluntary movement and jump while gravity continues. Host-owned lock decisions are replicated. Future abilities must establish their authoritative state and tick timing before requesting locks; local owner tokens alone are not a network ability system.

## Verification — 2026-09-10

- Unity 6000.6.0f1 compilation and Windows Development Build succeeded.
- Actual Photon Host (Editor) + Client (separate Windows process) spawned exactly two players, with one enabled camera on each peer.
- Client virtual keyboard W/Space input moved 4.4211 units to the wall and jumped approximately 1.44 units above its grounded height. Holding Space through landing produced exactly one press/jump sequence; grounded state returned true.
- Host and client final simulation positions matched; client rendered position matched its simulation position after settling.
- Virtual mouse input produced view angles (45, 41), observed identically on host; body remained at yaw 0 while stationary.
- Motor smoke checks passed for grounded jump, rejected air jump, movement-lock rejection, unlock, and matching position after restoring and re-simulating the same input sequence.
- Screenshot inspected with both visible player colors. No gameplay errors in final network run.

Tests ran on one machine through Photon. High-latency/loss tuning, moving-platform support, animation and ability networking are future work. Test input/position logging is development-only; no UI is created at runtime.
