# Stage 1 map

캐릭터·카메라·버섯/나뭇가지 점프대의 수정 위치와 Inspector 값 설명은 [CharacterAndJumpPadTuning.md](CharacterAndJumpPadTuning.md)를 참고하세요.

Current Stage 1 player scene: `Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference.unity`. The Main Menu solo route and multiplayer lobby both load this map. It uses two `PF_Player` offline instances, including their `StageActor` and held-item presentation, plus `PlayerSpawnLayout` with `PF_Stage01ModelingNetworkPlayer` and two spawn points for Fusion sessions. It also has the shared game overlay, the two slot-specific player HUDs, the play-screen controller, cursor capture/release behavior, and the Input System UI event module. The `StageLevel` points to `Stage_02_Mazes`.

`Stage_01_Exterior_260928.unity` remains available as the earlier authored exterior gameplay scene. The modeling map includes two herb sources, cooperative gathering and brewing, cloth swings, and mushroom and branch jump pads. The cooperative minigame setup is documented in `HerbPotionMinigames.md`; the approved exterior design remains in `Documentation/DesignRevision_260928.md`.

## Placing cloth and jump pads

Drag a **variant**, not a `_Base` template, from `Assets/_Project/Prefabs/InteractiveObjects/Stages/Stage_01` into the stage scene:

- `PF_HangingCloth_Black`, `Blue_A`, `Blue_B`, `Red`, `White_A`, `White_B`, and `Yellow` each contain their rendered fabric and `GripPoint`. Move the prefab root to move both together. Select it to see the yellow grab-point guide in Scene view; adjust the `GripPoint` child or the cloth's grab range if needed.
- Cloth W/S input pushes along the camera's horizontal forward/backward axis. A reverse press shortly before the swing turns is buffered until the return starts. Tune `_pumpImpulseSeconds` (push per press), `_damping` (momentum loss), and `_reversalInputWindow` (early-press timing allowance) on `PF_HangingCloth_Base`.
- `PF_MushroomJumpPad` and `PF_BranchJumpPad` each contain the model, leaf socket, water inlet, and water jet. Their root pivot is at the leaf launch point. Move or rotate the root to place the whole assembly; adjust `WallWaterInlet`/`BranchWaterInlet`, `SapJet`/`BranchSapJet`, and the model's `LeafCenter` only when changing their internal layout. The selected prefab shows the inlet in cyan and the leaf/water outlet in green.

New jump-pad instances receive a unique leaf-target ID in the Editor after placement or duplication. `Tools > Herbalist > Stage 1 > Assign Jump Pad IDs` runs the same check manually. Save the scene after placing them so multiplayer peers use the same authored ID. The original fabric FBX is still the mesh source; its separate scene instance is no longer needed.
