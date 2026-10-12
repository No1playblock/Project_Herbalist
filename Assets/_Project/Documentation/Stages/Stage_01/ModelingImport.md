# Stage 1 modeling import

`Stage_01_ModelingReference.unity` is now the Stage 1 player-testing map. It retains the existing offline `PF_Player` setup and the multiplayer `PlayerSpawnLayout`; `Stage_01_Exterior_260928.unity` remains available as the earlier gameplay scene.

## Modeling branch sync (2026-10-12)

Fetched `origin/feat-stage1Modeling` at `723b261`. The Branch and Mushroom meshes, textures, and materials added in the October 8 snapshot were already present with matching GUIDs and content. The later map changes were selectively merged into the current playable scene:

- Updated `Stage1_ModelingTerrain.asset` from the source LFS terrain (`Assets/New Terrain 3.asset`) and `ModelingSource/New Terrain 5.asset` from the second source terrain. The current GUIDs and scene references were preserved.
- Added the second Terrain object and 320 new `Reed_LOD` prefab placements to `Stage_01_ModelingReference.unity`, along with the source map's updated sun, fog, ambient light, and preview-camera pose. Existing player, jump pad, ability, HUD, and network objects remain in the scene.
- Synchronized the changed Terrain Layers, Stage 1 vegetation materials, the vendor foliage material variants, and `Suntail_Foliage_Fixed.shader`. New lighting-setting assets were copied with their source GUIDs.
- The source scene's unresolved Terrain material reference was mapped to the existing `Terrain_Grass` material. The source Input Actions and global PC render-pipeline asset were not copied: this project has its own gameplay bindings, and the source pipeline asset would reduce the global shadow distance from 50 to 1.

The map was merged by scene document ID rather than replacing the playable scene. This preserves the current game's authored components and scene GUID while incorporating the new source objects.

## Previous map sync (2026-10-08)

The latest map snapshot in `D:\download\Project_Herbalist` was synchronized into the reference scene and its assets:

- Source scene: `Assets/Scenes/Stage_1.unity`
- Stage 1 map: `Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference.unity`
- Terrain data: `Assets/_Project/Data/Stages/Stage_01/Stage1_ModelingTerrain.asset` and `ModelingSource/New Terrain 5.asset`
- Terrain layers: `Assets/_Project/Data/Stages/Stage_01/TerrainLayers`
- New map assets: `Assets/_Project/Art/Environments/Stage_01/Stage1_Assets/Materials/ImportedFromLatest` and `Models/ImportedFromLatest`

The scene keeps its existing GUID. It has 75 map prefab instances, two Terrain components, and the existing player prefab instance. All 205 source files in `Stage1_Assets` now match by GUID and content: 109 were added, 27 updated, and 69 were already identical. Import metadata was synchronized where its settings differed.

The download was missing two material assets referenced by the scene (the Terrain material template and the `Cylinder` renderer). Those references use the existing `Terrain_Grass` material so the scene has no unresolved external GUIDs.

## Earlier import

The initial modeling reference came from `feat-stage1Modeling` (`dd12b91`). It was imported selectively because that branch removed current game code and scenes. Unity `.meta` GUIDs were preserved when assets moved into the project structure. The latest map sync above supersedes its scene and Stage 1 map asset contents.

## Play in the reference scene

Open `Stage_01_ModelingReference.unity` and press Play to move the offline `PF_Player` through this map. Use WASD to move, the mouse to orbit the camera, and Space to jump. The scene preview camera is disabled during play so the player camera controls the view. Main Menu solo play and the multiplayer lobby both load this scene. Network sessions spawn `PF_StageNetworkPlayer` at one of the two authored spawn points.
