# Project development rules

- Follow the supplied game design; clarify ambiguities affecting architecture.
- Before implementing a new feature or changing architecture, explain future extension needs, options and a recommendation in Korean, and obtain the user's choice. The initial separated CharacterController motor architecture is approved.
- Keep tuning values and content in serialized configuration/assets, not gameplay code or key checks.
- UI must use UGUI, authored in the scene/prefab hierarchy before play and shown/hidden with SetActive. Never create UI at runtime.
- Separate input, locomotion backend, abilities, presentation and networking. Minimize changes required to add abilities without speculative frameworks.
- Discuss major architecture changes before proceeding. Photon Fusion with Host (client-server) topology is selected and approved. Fusion SDK 2.1.2 build 2279 is installed; App ID is configured and real Host/Client connection plus two-player scene transition have been verified. Main-menu UGUI and Host/Client session flow with automatic scene transition at two players are approved. Host-authoritative player spawning, local predicted movement and grounded Space jump are approved and implemented. Space is reserved for normal jump; ability keys will be decided later.

- Viewport split/personal switching is approved: Duyeong left, Sodam right; Tab is a rebindable presentation action. Each peer selects its own mode. Keep authored common/character UGUI roots and owner-scoped forced-mode overrides separate from player input/network simulation. Prototype role assignment maps serialized spawn slots to roles.

- Leaf ability architecture approved: component-based unlock/input/leaf gameplay/target reactions/Fusion adapters. R cycles Off -> Pin -> Platform -> Off; left click uses selected mode, immutable per throw. Flying/installed/returning leaves share capacity 3. At capacity recall oldest installed leaf, wait for its return, require another click; no queued auto-throw. Potion unlock is a separate authoritative hook; prototype grants both Duyeong and Sodam for multiplayer testing, plus the offline player.
