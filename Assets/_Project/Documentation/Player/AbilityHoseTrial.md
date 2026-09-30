# Sap Hose behavior

R near a SapSource starts/cancels extraction in the sandbox. The held blue blob moves to the body-relative right chest; hold Use (left mouse button) to spray. Empty-space shots are valid. Host/offline raycasts from the blob toward camera aim each tick, so aim distance updates continuously. Same-surface nearby marks merge and grow within serialized limits; unbound marks disappear after the configured last-hit lifetime. Current stages can create sap contextually at an eligible inlet. The Host owns hit, growth, expiry and replication.

The old Placement mode, flying blob, placement preview and sap-leaf binding were removed on 2026-09-30. Rollback requires restoring source/assets from Git or the local cleanup backup; changing a ScriptableObject field alone no longer restores them. `SapHoseChecks.cs` covers hose behavior; real two-peer/visual checks still require Unity Editor and a client.
