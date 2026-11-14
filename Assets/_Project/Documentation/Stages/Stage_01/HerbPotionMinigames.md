# Cooperative herb and potion minigames (2026-10-08)

The Stage One flow now uses two in-scene UGUI views in the authored `PF_PlayerHud` prefab. Only the local character's view is shown on each peer. Host state authority validates starts, directional inputs, the two-player herb transfer, brewing, and final item changes. `NetworkStageState` replicates the active minigame and its progress.

## Flow

1. Duyeong finds a glowing herb and presses Interact (default E). Both players enter their own gathering view, and the Stage One owner token locks movement and ability input. Duyeong sees an illustrated close-up of the herb. Sodam selects a specimen in the six-entry codex and reads its ten-step direction sequence to Duyeong.
2. A wrong direction ends the attempt without harvesting. Ten correct directions consume the source and put the herb in Duyeong's hand.
3. With both characters nearby, each presses Interact within `StageOneSettings.transferConfirmSeconds`. Either may press first. The herb moves to Sodam only after the second press.
4. Sodam presses Interact while holding a herb to start brewing. Each Craft press (default Space) counts toward the most recent one-second window. Duyeong sees the current temperature and fire strength; Sodam sees the target band and the brewing gauge. At 0-1 presses per second the pot cools by 6°C/s, at 2-3 it holds, and at 4+ it heats by 6°C/s. The gauge gains 8%/s in the target band and loses 12%/s outside it. Reaching 100% within the recipe's time limit creates the potion. Reaching 100°C or running out of time ends the attempt and keeps the herb.
5. Drinking, permanent ability choice, and the Stage One gate continue through the existing flow.

`Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference`, the current menu destination, has the two playable herb sources and the Stage One network flow.

## Offline role test

The solo scene contains two separate offline player actors: Duyeong in slot 0 and Sodam in slot 1. Both remain in the world with their own inventory, potion and ability state. F transfers local control and the personal camera/HUD between them; the unpossessed actor continues physical simulation. Duyeong gathers and holds herbs, and both actors must confirm a nearby transfer with Interact. During brewing, each Space press fans while Sodam is possessed; F can still transfer control. Outside brewing, Space retains its normal jump behavior. In a Fusion session the offline actors are hidden and role authority remains tied to the assigned network slot.

## Authoring

- The two herb item assets define appearance text, gathering method, exactly ten `StageDirection` values, and the planted image plus ten pull frames. Each correct step reveals one progress dot with the accepted key and shows the corresponding frame. The existing replicated success result lets the local HUD hold the tenth frame briefly, even when the Host clears the minigame in the same snapshot. No gameplay input or replication fields are added.
- The codex uses the two playable herb assets and four display-only entries from `Data/UI/Gathering`: 이슬고사리, 달빛방울꽃, 붉은매듭풀, and 은별이끼. Display-only entries are not registered as ingredients, recipes, or world sources. Selecting an entry only changes Sodam's local display.
- The two gathering views are authored under `PF_PlayerHud`. Each peer uses the existing owner-scoped Personal view override while its gathering UI is open, and Sodam's book releases the pointer for selecting entries. All gathering UI textures are in `Art/UI/Textures/Gathering`. The two frame sequences reuse the corresponding botanical portraits so the silhouette stays consistent as roots rise through the same soil line.
- `StageOneSettings` defines the shared one-second fan bands, temperature change, gauge rates, and 100°C failure threshold. Each `StageRecipe` defines initial temperature, target band, and time limit.
- Brewing now fills the local screen through the existing owner-scoped Personal view override. The preauthored `PF_PlayerHud/BrewingHud` uses the supplied parchment, cauldron/fire and gauge sprites in `Art/UI/Textures/Brewing`; its current-temperature marker, target band, vertical brewing fill and countdown remain live UGUI elements. Duyeong still sees current temperature while Sodam sees the target band. The supplied scroll sprite belongs to the shared stage header in `PF_GameOverlayHud`, which hides during a minigame.
- Item assets have stable content IDs and a medicine-material flag. Recipes retain direct ingredient/result asset references.
- `SO_StageMinigameUiText` contains the role-specific copy. The `PF_PlayerHud` contains the panel and text references; no UI objects are created during play.
- The supplied PDF's data-sheet sections contain no herb or potion values. Current appearance descriptions, sequences, and recipe numbers are prototype content to tune with design.

## Verification

In the imported map's offline Play mode, both herb attempts were exercised through the existing command handler. The HUD started at zero, a wrong input closed the attempt without harvesting, and all ten accepted inputs for each herb showed the expected sprite and filled-dot count, including the final frame after success. All six codex entries accepted UGUI pointer events and updated the selected entry. Unity reported no errors or warnings. The existing `StageOneChecks` also passed. This verification did not open a real Fusion Host/Client pair.
