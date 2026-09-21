# Exterior -> interior stage flow

## Scenes and layout
- MainMenu Multiplayer starts Assets/_Project/Scenes/Stages/Stage_01/Stage_01_Exterior.unity. Single Player keeps the existing Sandbox_Abilities ability sandbox.
- Stage One starts southwest of Sacred_Tree_1.0 at (-44, .03, -40). Exterior exploration loops around the tree, with herb points at (-41, 4.25, 20) and (40, .65, -22). The first uses four 0.8m stone rises and a 3.6m gathering rock; the return route detours around the southern root tip.
- SacredTree_Entrance belongs to the actual Sacred_Tree model, with a potion-controlled seal at (1.3, 2.8, -14) and interior entry volume centered at (1.3, 2, -9). The obsolete freestanding gate is removed.
- Stage Two starts at the center of the six Root_Big_Final trees, around (48, .03, -58), with separated player spawn points. The six existing tree meshes were resized and arranged around the center for prototype traversal; their mesh colliders, readable mesh import, SapSource, SapReceiver and LeafInstallTarget are configured. IDs 201-206 are unique and paired.
- CatchFloor_NoRespawn is a solid lower floor. Falling does not trigger respawn, reset the partner or clear placed ability objects.
- UpperArrivalBranch is the only authored upper standing platform; it lies around (48.65, 13.55, -58.19). Intermediate leaf platforms do not exist until players create them.
- ExpectedExteriorRoute and SuggestedLeafRoute_NoAuthoredPlatforms are Editor-only gizmo guides. The internal landing samples rise by .75m, following actual inner-facing reachable tree surfaces and a clear-body path; the upper goal is offset to avoid covering the final leaf. Leaf placement remains freeform.
- Trees use MAT_RootBig01 through MAT_RootBig06 (URP Lit) with individual base color, OpenGL normal, AO and packed metallic/smoothness textures. Tree render bounds are grounded and arranged at radius 8.2 around the central spawn; horizontal half-extent is normalized to 4.9. Existing interaction IDs 201-206 are preserved.

## Runtime responsibilities
- StageLevel stores the next scene path and whether network players require earned abilities.
- StageExit listens to StageOneFlow's one-shot completion and only the Host requests transition.
- FusionLobbySession snapshots each PlayerRef's slot and actual acquired ability, despawns old players, and loads the next scene through Fusion. PlayerSpawnLayout restores slots; NetworkLeafAbility initializes factories then restores the saved ability. Prototype slot grants are suppressed in authored earned-ability stages.
- Session state is cleared on new connection or leaving. No character-to-ability mapping is reimposed.
- StageTwoGoal checks both slots inside the authored upper volume on Host and replicates completion. UGUI is preauthored. A third-stage transition is not implemented.
- Direct Editor StageTwo runs the existing offline ability test defaults; multiplayer only restores earned abilities.

## Validation
- Stage One: LevelTraversalChecks.Run() in Play mode passed 19 ordered route/landing points using the real CharacterController motor, including rock jumps and actual Sacred_Tree entry. The seal is temporarily opened by the check and restored afterward.
- Stage Two: the same check passed 19 landing points using transient leaf-sized colliders installed sequentially with capacity 3. All test objects are removed and player pose restored. This checks movement geometry, not a complete manual two-person puzzle solve or timed ability aiming.
- All six sources successfully returned closest mesh surface points; all six paired target/receiver IDs and readable mesh imports were verified.
- Real two-process Photon test: Windows Development build, -herbalist-stage-test host/client ROOM -herbalist-level-test -swap-potions. Both logged [StageLevelsCheck] PASS with slot0=Leaf, slot1=Sap, stage2clear=true. Tests cover gathering/crafting/drinking, real scene transition, preserved reversed ability choices and two-player-only upper completion. It teleports test actors for progression setup; it does not claim a manual puzzle playthrough.
- Local logs and verification builds are ignored under Temp/StageLevelsBuild.

## Editing
Move authored source/entrance/arrival/spawn transforms to tune layout; keep network IDs stable. StageLevelLayout is a one-time Editor authoring tool and intentionally refuses to overwrite an existing Stage Two scene. Saved scenes are the source of truth. Re-run traversal and network transition checks after significant geometry or progression edits.

## Latest art restoration (2026-09-19)
- Latest FBX files are used; RootBig05 is rotated upright to account for its changed long axis.
- Created eight materials, including RootEndpoint and RootNormalKit, and remapped FBX material imports. The two extra models are prepared for placement; they are not added to the six-tree gameplay layout.
- Metallic/smoothness textures pack source metalness into R and 1 - source roughness into A for URP Lit. Source maps remain available.
- The damaged T_RootNormalKit_Normal.png is not referenced. The valid NormalOpenGL texture is used instead. Height maps remain as source assets; displacement is not enabled.
- Updated the suggested leaf route to match current surfaces. Moved the upper platform inward to avoid blocking jumping headroom; its standing surface is approximately (48.65, 13.55, -58.19).
- Play-mode CharacterController traversal passed all 19 points using temporary test platforms. All six source closest-surface queries and receiver/leaf target pairs passed. This is geometry validation, not a full two-player ability playthrough.
