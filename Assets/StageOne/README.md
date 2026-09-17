# Stage One: gathering, crafting and drinking

## Entry and test locations
- MainMenu multiplayer -> StageOnePrototype. Single Player -> original PlayerMovementPrototype for independent ability testing.
- StageOnePrototype now contains the Sacred_Tree exterior. Two herb points lie on opposite sides of the tree; one requires a short rock jump sequence. ExpectedExteriorRoute is an Editor-only planning guide. SacredTree_Entrance/StageTwo_EntryVolume replaces the old freestanding test door. Both players entering after potion completion advances to StageTwoPrototype.
- StageNetworkPlayer is a separate prefab with the interaction adapter and authored held-item visuals. Existing character/ability test prefabs are preserved.

## Controls and rules
- E: gather nearest reachable herb with empty hands, or give the held item to the nearby partner.
- F: Sodam turns the held herb into its matching potion, immediately for this prototype.
- G: drink the held potion. Any already acquired ability blocks all further potion consumption without deleting the held potion.
- Bindings are in StageControls.inputactions. StageOneSettings controls range, line-of-sight mask, role-to-slot mapping, glow layer and UI text.
- One object per hand. Transfer requires the receiver's empty hand. There is no dropping or discarding, preventing loss of the two unique puzzle ingredients.
- Duyeong gathers, Sodam crafts. Either character can acquire either ability. Recipes and items are ScriptableObjects; recipes currently consume one ingredient and produce one potion.
- Glow is an authored mesh rendered on DuyeongHerbGlow layer. StageActor adjusts each character camera's culling mask, including remote cameras in split view. The shared herb mesh remains visible to both players.
- StageOne disables prototype automatic ability grants and Q switching. The original ability prototype keeps them.

## Ownership and extension
- StageProgress contains inventory, recipe and clear rules, independent of Fusion and input.
- StageOneFlow validates range, obstruction, character role, source availability and ownership before modifying progress.
- NetworkStageActor uses input-authority -> state-authority RPCs. Clients do not send item IDs, target positions or claimed roles. Host selects and checks targets. NetworkStageState replicates authoritative progress; existing NetworkLeafAbility replicates granted ability and its gameplay.
- UGUI and held-item visuals are authored before play; runtime only updates text, colors and SetActive.
- All configured recipe outputs must be manufactured and consumed, then both players must currently be inside SacredTree_Entrance/StageTwo_EntryVolume. Clear is latched once.
- onEntranceOpened and onCleared fire once on each peer for presentation. A future scene-transition listener must check StageOneFlow.Authority and use Fusion scene loading. StageExit now invokes the Host session transition to the configured StageTwoPrototype scene.
- The gameplay model currently targets exactly two character slots, up to 64 authored herb sources and 32 recipes. One permanently acquired ability per player means this stage config must use two distinct required abilities. Multi-ingredient recipes/inventory UI are outside this prototype.
- Source array order and item catalog indices are replicated identifiers: keep the same assets in both peers' builds, and do not reorder them during a session.

## Validation tools
- Editor: Herbalist.Editor.StageOneChecks.Run() tests both ability assignments, prohibited roles/actions, full hands, duplicate harvest/craft, failed grant rollback, second potion rejection, two-person clear and one-shot completion.
- Opt-in development build: -herbalist-stage-test host ROOM / -herbalist-stage-test client ROOM. Uses real Photon sessions, request RPCs and replicated state, and moves test actors on Host only. Look for [StageOneNetworkCheck] PASS or FAIL in each log.
- Direct Editor StageOne play exposes Duyeong alone for visual/gather testing; it cannot bypass two-player progression. Use original PlayerMovementPrototype for solo ability testing.

## Verified 2026-09-16
- Editor rule checks PASS for both character/ability assignments.
- Scene Play: automatic ability grant and Q switch disabled; E gathering works; no console errors. Authored HUD visually inspected with role heading unobscured.
- Windows Development build succeeded with zero errors.
- Real Photon Host/Client, separate processes: both logged `PASS ... crafted=3 consumed=3 inside=2 clear=True`. The client crafted, drank the first potion, rejected a second drink without losing the potion, then transferred it back. Host verified one player inside does not clear, then both entering clears. Both peers verified ability replication and camera glow isolation.
- Initial test found a stale Fusion prefab catalog. Forced prefab/config import fixed it; setup now refreshes the catalog. Successful logs: Temp/StageOneBuild/host3.log and client3.log (local ignored test artifacts).

## Stage transition revision (2026-09-17)
See Assets/StageTwo/README.md for layout, ability persistence and transition verification. Historical verification below/above describes the earlier standalone stage test.
