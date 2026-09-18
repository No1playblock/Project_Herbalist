# Player movement and networking

## Controls

- WASD: camera-relative movement; diagonal input is normalized.
- Mouse: orbit camera without holding a button; head follows within configured limits.
- Space: one grounded jump per press. Holding Space does not repeat, and airborne presses do not grant a second jump.
- Tab: toggle split/personal screen locally. Ability bindings remain undecided.

Use MainMenu for multiplayer. The host spawns one NetworkPlayer for each connected player after both peers enter Sandbox_Abilities. Spawn points live under PlayerSpawnLayout. Green/orange materials distinguish the two slots. Each peer enables only its own input and AudioListener. PlayScreenController controls which cameras render. Leaving players are despawned by the host.

Opening Sandbox_Abilities directly preserves the offline player for local testing. Its scene object is disabled when entering through a Fusion session.

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


## Screen modes (2026-09-11)

PlayScreen in Sandbox_Abilities owns the local presentation mode. Default: split, Duyeong left / Sodam right. Serialized regions map spawn slot 0 (green) to Duyeong and slot 1 (orange) to Sodam; this is not a character selection system. Personal mode renders the local camera fullscreen, with the partner still visible in the world. A missing partner falls back to local fullscreen while preserving the preferred mode.

Change PlayerControls.inputactions > Presentation/ToggleScreen to rebind Tab. The screen component clones this action independently of movement. Screen choice is never network input or an RPC. Initial mode, personal viewport, role/slot mapping and split rectangles are serialized on PlayScreen. PlayerView updates enabled remote cameras using replicated look angles without enabling remote input or audio.

PlayHUD contains authored CommonHUD, DuyeongHUD, SodamHUD and SplitDivider. Character roots resize via anchors and show/hide with SetActive; CommonHUD stays screen-wide. No runtime UI creation. Prototype labels use the existing Latin font. Future interaction indicators belong to the corresponding character root; aiming must use the owning camera's viewport center.

Level/sequence adapters can call SetOverride(owner, mode) and SetOverride(owner, null). The latest active owner wins; any override blocks Tab. Releasing the final override restores the player's preference. Adapters must release their token on exit/disable. This API is local; future authoritative level rules must distribute their state to both peers. No trigger zones or level networking are introduced yet.

Verified with a Windows Development Build and actual Photon Editor Host + separate Client: role-aligned viewports, one AudioListener, Tab/hold behavior, personal fullscreen, nested override restoration, HUD anchors and continued movement. Client finished in Personal while Host remained Split. Both layouts were visually inspected. Partner disconnect returned Host to fullscreen. PlayScreenSmokeTest is opt-in via -herbalist-test-screen in development builds.


## Leaf ability prototype (2026-09-12)

R cycles Off -> Pin -> Platform -> Off. Left-click throws only in Pin/Platform mode. Bindings are Ability/Cycle and Ability/Use in PlayerControls.inputactions. The selected form is copied into the projectile at launch. Turning the ability off does not remove existing leaves. Shots follow a curved path and return on a miss or an incompatible surface. Only authored LeafInstallTarget components accept installation.

LeafAbilitySettings.asset contains range, speed, arc width, collision radius, return speed, cooldown, capacity (3), lifetime (5 seconds), launch offset and offline prefab. Installed platforms are solid on the Leaf layer. The projectile hit mask excludes that layer so an existing leaf does not incorrectly intercept another shot; the player still collides with platforms.

Flying, installed and returning leaves all occupy capacity. At capacity, clicking recalls the oldest installed leaf and waits for THAT leaf to return. It does not queue an automatic throw; click again afterwards. If all leaves are airborne/returning there is no installation to recall, so additional input is ignored. Recall immediately removes the platform collider and pin effect. Expiry, capacity recovery, revoke and owner cleanup share the same release path.

PlayerAbilityController exposes UnlockLeaf/RevokeLeaf for future authoritative potion effects. Offline test grant is serialized on Player.prefab; NetworkLeafAbility.prototypeUnlockSlots grants slots 0 and 1 (Duyeong and Sodam) for multiplayer testing. The offline character also receives a test grant regardless of its temporary role label. No potion UI or crafting system is implemented.

LeafThrowAbility/LeafProjectile contain gameplay; NetworkLeafAbility/NetworkLeafProjectile adapt spawning, input and state to Fusion. Host alone advances leaf gameplay and decides hits, timers and installation. Clients render NetworkTransform poses and apply replicated form/state/target IDs to colliders and target effects. Movement remains predicted, but this first leaf implementation waits for Host confirmation rather than predicting projectiles. Do not call authority grant functions from client potion UI; future potion validation belongs on Host.

LeafInstallTarget defines allowed forms, optional socket and installation/release UnityEvents. Each scene target needs a unique positive targetId for replicas. SapBindingSource is an integration hook: authority calls Deposit before the shot; only a subsequent leaf hit binds it. Bound leaves and sap remain until recall, and sap deposited after installation never retroactively binds. Standalone sap creation/movement/network replication is not implemented here; a future sap ability must own that lifecycle. Local test targets provide this hook for automated checks, not a new player sap input.

Sandbox_Abilities/LeafAbilityTestTargets has PlatformTarget (x=6,z=6) and PinTarget (x=-6,z=6). The pin target lights an indicator while pinned. This verifies the target event contract; real maze plugs and altar behavior will implement their own reactions. The player's LeafHUD is preauthored under each character HUD, with mode/count and an active-mode reticle. No UI objects are created at runtime.

Verification: Editor LeafAbilityChecks.Run exercises real virtual R/click input, Off blocking, incompatible target return, immutable thrown form, five-second expiry, revoke/unlock, sap ordering/binding/removal, three-leaf capacity and pin effect release. Windows Development Build passed; actual Photon Editor Host + separate Client with -herbalist-test-leaf passed authoritative spawning, three replicated solid platforms, R mode replication, oldest recall, Off and expiry. High latency/loss and moving-platform carrying are not validated by these local-machine checks.

Additional multiplayer check: a different player stood grounded on the bound leaf platform; disconnecting its owner removed all leaves and the standing player fell and landed normally.


## Gameplay cursor

GameplayCursor on PlayScreen locks and hides the cursor during focused gameplay. Escape releases it; clicking inside Game View captures it again. The capture click is consumed and cannot throw a leaf. Look and ability inputs are blocked while released or unfocused; movement is unchanged. Returning focus does not steal the cursor: click to resume. Scene exit/disable releases it for menus. Future menus call SetUiMode(owner, true/false); any open owner blocks recapture. Bindings are Presentation/ReleaseCursor and Presentation/CaptureCursor. Pointer input smoke tests now require a focused, captured Game View/player window. Verified virtual input: normal click, Escape, consumed recapture click, next click and menu blocking.

Pin-form leaves now install horizontally and support players with an authored collider, while retaining their target-pinning effect. Both offline and network prefabs are updated; replica state enables the same collider. Verified horizontal orientation, actual CharacterController standing, and immediate collider/effect removal on recall in play mode.
