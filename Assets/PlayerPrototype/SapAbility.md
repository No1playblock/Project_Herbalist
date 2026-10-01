# Sap ability prototype

Implemented from the 2026-09-13 level design and the approved extraction/control interpretation.

## Test controls

Open `Assets/PlayerPrototype/PlayerMovementPrototype.unity` for offline testing. The offline player receives Sap. MainMenu still connects two players and opens this scene; prototype slot 0 (Duyeong) receives Sap and slot 1 (Sodam) receives Leaf. This is an Inspector-configured test assignment, not a character restriction. Potion code should call the authoritative `UnlockSap` / `UnlockLeaf` hooks. A player cannot acquire both simultaneously.

Near `SapAbilityTestObjects/SapExtractionTree`, press the rebindable Ability/Cycle action (R) to extract one blob. Mouse look guides it toward the center ray; in open space it follows a configured point along that ray. It moves at a configured speed, collides with geometry and stays within the source control range. Move and normal jump are locked during control; looking remains available. Press R again to cancel and refund the extracted supply unit. Left click attaches/injects only when the blob reaches a valid target, then ends control and unlocks movement. Invalid clicks do not discard the blob.

Existing LeafHUD roots display the equipped ability and valid-placement prompt. No UI is instantiated at runtime. The gold sphere is a prototype liquid visual; the flattened gold patch marks an attachment.

## Authoring

- `SapAbilitySettings.asset`: extraction/control range, free-space aim distance, movement speed, collision radius/mask, placement tolerance, five-second lifetime, binding/refresh radius, visual sizes and capacity (prototype 8). Capacity includes controlled, attached and bound blobs. At capacity a new extraction is rejected; existing bound supports are preserved.
- `SapSource`: explicit opt-in tree, interaction collider, extraction point and supply units. Zero units means unlimited. Place the extraction point outside the tree collider with enough clearance for the configured blob radius. Obstacles can block both the aim ray and movement; steer around them. Supply is consumed when extracting and refunded only on cancellation.
- `SapReceiver`: unique positive scene ID, optional LeafInstallTarget and optional inlet transform. Without an inlet, the collider surface accepts free placement. With an inlet, hits must be within its configured radius. `onSupplied` / `onDrained` drive authored reactions as deposits appear/disappear. Replicas invoke these presentation events too; future networked device gameplay must remain Host-authoritative in its adapter.
- `SapInletTest`: minimal inlet with an authored indicator which is active while supplied. Full jump-pad/altar behavior is not included.
- Existing leaf test targets have receivers using their existing IDs. Decorative scene geometry does not automatically become a source or receiver.
- Offline assignment: PlayerAbilityController / Prototype Offline Ability. Network assignment: NetworkLeafAbility / Prototype Sap Slots and Prototype Unlock Slots. The existing adapter and HUD retain their historical names but now route both abilities.

## Gameplay and networking

SapControlAbility handles extraction/control and owner-scoped motor locks. SapDeposit handles attachment, lifetime and binding. SapSource handles supply; SapReceiver handles accepted surfaces/inlets and reactions. These gameplay components do not reference Fusion. NetworkSapDeposit replicates transform/state/receiver; the player adapter reconstructs aim on the Host and runs ability input there. Local visual prediction of sap is not implemented.

A new attachment lasts five seconds, starting on placement. Reapplying within the refresh radius refreshes an unbound deposit rather than creating an overlapping duplicate. A leaf must hit the same receiver, within binding radius, after the sap was placed. Leaf-first placement never binds retroactively. Bound sap and its leaf persist until leaf recall; recall removes the matching sap immediately. Matching is positional, not target-wide. Disabling a target or destroying its owner cleans up their deposits.

The old SapBindingSource remains as a legacy test hook on targets without SapReceiver; new receivers use actual deposits. It does not control the new ability. Ordinary attachment uses a configurable five-second lifetime. Maze finite-volume/path movement is a later movement implementation; it is not implemented by treating free-flight blobs as maze fluid.

## Verification (2026-09-14)

- SapAbilityChecks.Run in play mode: extraction, movement/jump lock, movement to a valid surface, cancel, refresh without duplication, expiry, receiver supply/drain state, position-specific sap-first binding, no retroactive leaf-first binding, bound persistence, recall cleanup, unsupported surface rejection and extraction range passed.
- Windows Development build succeeded with zero build errors.
- Actual Fusion Host in Editor plus separate Windows Client: SapNetworkSmokeTest Host PASS and CLIENT_PASS. Verified extraction/cancel and movement lock, replicated attachment/expiry, Client leaf inputs processed by Host, binding beyond the five-second lifetime and replicated recall cleanup. Test injects input reader sequence counters, exercising Fusion OnInput and Host validation; it does not test OS keyboard focus/cursor capture.
- Test executable/logs and screenshots are under ignored `Temp/SapTest/`. To repeat: run a Development Client with `-herbalist-test-join ROOM -herbalist-test-sap-observer`, and call SapNetworkSmokeTest.RunHost after creating ROOM in the Editor Host.

- Disconnect cleanup verified separately: after the Client owning a bound leaf disconnected, Host peer count became one and both leaf/sap counts became zero.
