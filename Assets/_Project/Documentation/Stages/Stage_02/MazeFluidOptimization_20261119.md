# Stage Two fluid presentation optimization (2026-11-19)

Approved scope: keep the finite-volume gameplay, leakage, refills, rounded outline and animated blue water. Reduce presentation/network work and prevent liquid from stretching up relief walls.

## Presentation

All three authored mazes now use `SapMazeDensitySurface`, with preauthored renderer children and baked wall visibility masks. The original relief mesh stays static; occupied density is uploaded to an R16 texture. The density shader retains rounded boundary shading and animated ripples, with flowing reflection ribbons and view-dependent highlights. Reflection color, strength, speed and scale are serialized on `MAT_Maze1Density`. Dynamic tessellation remains a fallback for unsupported texture formats or missing authored density data.

The shader rejects steep geometric faces using the maze's existing maximum floor slope, and limits relief displacement from the authored floor plane. `SO_MazeFluidVisual.maximumReliefHeightWorld` is 1.4 m, calibrated against all three existing relief meshes; a smaller arbitrary limit cuts genuine floor surfaces. The visual lift is included in the shader limit. Dedicated wall masks continue to reject density across barriers.

The fallback mesh also accepts only floor-facing relief. Clearance is computed from original vertices and applied once per vertex, rather than accumulating displacements as adjacent triangles are processed. Foreground ridges may naturally occlude liquid; raising water over every ridge recreates the wall-climbing artifact.

## Host and client

Gameplay still runs on the Host at its original simulation tick rate. `fluidSnapshotInterval` is independently serialized (0.05 s). Host snapshots compare packed values before writing network array entries and increment `FluidVersion` only when the published snapshot changes. Field replacement, attempt/phase transitions and emptying bypass throttling. Small gameplay state, including remaining volume and leak state, still publishes normally.

Clients restore the field only on a new version or local field replacement. This removes repeated restoration of identical network arrays on every rendered frame. `fluidRenderInterval` remains 0.05 s, and the smoothing settings remain unchanged.

## Verification

`SapMazeFluidChecks.Run()` passes conservation, wall separation, reverse/merge, snapshots, leakage/refill, small drops and exact leak contact tests.

`SapMazeFloorShaderChecks.Run()` renders synthetic geometric surfaces through the real shader: 0/30-degree floors remain visible, a 75-degree wall and an out-of-band elevated surface produce zero water pixels.

`SapMazeDensityChecks.RunMaze(name)` traverses each complete authored route, compares paired dynamic-mesh and density updates with identical snapshots, verifies unchanged mass, checks thin-wall fixtures and tests visible pixels for wall penetration/disconnected density.

Local Unity Editor paired-update averages (not game frame times or multiplayer FPS):

| Maze | Dynamic mesh | Density texture |
| --- | ---: | ---: |
| 1 | 35.427 ms | 3.783 ms |
| 2 | 29.761 ms | 3.203 ms |
| 3 | 34.221 ms | 3.296 ms |

Maze 1 already used density presentation before this change. The table compares rendering paths; it does not claim the same improvement over the previous live configuration for Maze 1. All three checks reported zero visible wall pixels and zero density samples disconnected across a wall. Detailed reports and captures are under `Logs/Maze1Density`.

The final Development build succeeded with zero build errors. Two standalone Fusion peers passed `MazeNarrowNetworkCheck` with the client owning Sap (Host slot 1, Client slot 0). Four forward/reverse segments of Maze 3 used actual pointer input, preserved 60 volume, reached their targets, reported zero wall cells, and produced identical packed-cell hashes on both peers. Both used the density renderer. Reports and gameplay captures are under `Logs/MazeOptimizationNetworkFinal`. This is a synchronization regression check, not a multiplayer FPS benchmark.

The test uses a runtime copy of lobby settings to skip the 169-second opening cinematic, and waits for the Host room before launching the Client. Authored cinematic and lobby settings are unchanged.
