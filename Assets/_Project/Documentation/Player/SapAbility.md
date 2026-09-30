# Sap / water ability

`SapAbilitySettings` holds extraction, stream, hose, mark and collision tuning. `SapSource` supplies the optional source-extraction sandbox flow. `SapControlAbility` owns held-sap state and the owner-scoped movement lock; `SapHoseSprayer` projects a continuous stream toward current aim. `SapDeposit` owns the held blob and surface marks, including last-hit expiry. `SapReceiver` exposes supplied/drained reactions. `SapInjectionPort` accepts continuous injection in current stages. Fusion adapters replicate authoritative state and presentation.

The current revision uses contextual water at stage inlets and blue visual assets. The legacy mouse-preview Placement flight, sap-leaf binding and finite placement inventory were removed. A placed leaf and water mark have independent lifetimes; a leaf blocks a maze leak only while installed. `Sandbox_Abilities` retains source extraction and Hose for direct solo testing.
