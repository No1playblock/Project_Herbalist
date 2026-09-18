# Orbit-camera ability trial

Approved: leaf shots face the aim direction; sap uses a reversible continuous hose mode.

## Controls and behavior
- Leaf: an accepted throw turns the body horizontally toward aim; camera/pitch is unchanged.
- Sap: R extracts/cancels using the existing nearby source selection and movement lock.
- The held blob stays at the body-relative right chest offset. It has no source-to-player stream.
- Hold the existing Use action (left mouse button) to spray; release stops. Empty-space spray is allowed.
- Host/offline casts from the blob toward the camera aim, limited by controlRange. The leading stream extends with flightSpeed.
- Hits on collidable surfaces create marks. Nearby hits on the same surface with similar normals merge and grow to hoseMaxScale.
- Unbound marks expire hoseMarkLifetime (5 seconds) after their last hit. Once sap binds to a leaf, both lifetimes stop; the pair persists until the leaf is recalled or removed. Recall removes the bound sap too.
- Existing SapReceiver reactions still apply. Decorative colliders can show marks without a receiver.
- The legacy capacity limit applies only to Placement mode. Hose marks merge by surface/proximity and, while unbound, expire after the last hit; existing nearby marks never block a new distant hit.
- The placement preview is hidden in Hose mode. Existing authored UGUI shows hold-to-spray instructions.
- Host owns hits, growth and lifetime. Held input, stream endpoints/mode and mark scale are replicated with Fusion.

## Rollback
Set Assets/PlayerPrototype/SapAbilitySettings.asset controlMode to Placement.
The old hoverOffset, extraction stream, placement preview, click-to-launch and persistent bound lifetime remain.
Set Assets/PlayerPrototype/LeafAbilitySettings.asset faceAimOnThrow to false to restore the prior leaf facing behavior.
These switches are independent of the camera PlayerTuning.facingMode.
Baseline before this trial: commit 0075562.

## Verification
Assets/Editor/SapHoseChecks.cs verifies empty-space spray, advancing stream, wall marks, merging/growth/cap, release, last-hit expiry, body-relative hover, leaf facing, bound sap/leaf persistence beyond both lifetimes and recall/removal cleanup, legacy placement, continuous near-to-far aiming and isolation from the legacy capacity limit.
This edit-mode geometry check is not a real two-peer transport or visual playthrough test.
