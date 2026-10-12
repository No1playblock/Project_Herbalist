# Cooperative herb and potion minigames (2026-10-08)

The Stage One flow now uses two in-scene UGUI views in the authored `PF_PlayerHud` prefab. Only the local character's view is shown on each peer. Host state authority validates starts, directional inputs, the two-player herb transfer, brewing, and final item changes. `NetworkStageState` replicates the active minigame and its progress.

## Flow

1. Duyeong finds a glowing herb and presses Interact (default E). Both players enter the gathering view and their movement and ability input are locked by the Stage One owner token. Duyeong observes the world herb. Sodam sees both codex entries and tells Duyeong the matching ten-step direction sequence.
2. A wrong direction ends the attempt without harvesting. Ten correct directions consume the source and put the herb in Duyeong's hand.
3. With both characters nearby, each presses Interact within `StageOneSettings.transferConfirmSeconds`. Either may press first. The herb moves to Sodam only after the second press.
4. Sodam presses Interact while holding a herb to start brewing. Each Craft press (default Space) counts toward the most recent one-second window. Duyeong sees the current temperature and fire strength; Sodam sees the target band and the brewing gauge. At 0-1 presses per second the pot cools by 6°C/s, at 2-3 it holds, and at 4+ it heats by 6°C/s. The gauge gains 8%/s in the target band and loses 12%/s outside it. Reaching 100% within the recipe's time limit creates the potion. Reaching 100°C or running out of time ends the attempt and keeps the herb.
5. Drinking, permanent ability choice, and the Stage One gate continue through the existing flow.

The original `Stage_01_Exterior_260928` sources use the same flow and HUD prefab. `Stage_01_ModelingReference`, the current menu destination, now has two copied herb sources near the spawn points and a Stage One network flow. The interior volume at the tree is a temporary Stage One completion region on this modeling map; the map's final entrance layout remains to be authored.

## Offline role test

The solo scene contains two separate offline player actors: Duyeong in slot 0 and Sodam in slot 1. Both remain in the world with their own inventory, potion and ability state. F transfers local control and the personal camera/HUD between them; the unpossessed actor continues physical simulation. Duyeong gathers and holds herbs, and both actors must confirm a nearby transfer with Interact. During brewing, each Space press fans while Sodam is possessed; F can still transfer control. Outside brewing, Space retains its normal jump behavior. In a Fusion session the offline actors are hidden and role authority remains tied to the assigned network slot.

## Authoring

- The two herb item assets define appearance text, gathering method, and exactly ten `StageDirection` values.
- `StageOneSettings` defines the shared one-second fan bands, temperature change, gauge rates, and 100°C failure threshold. Each `StageRecipe` defines initial temperature, target band, and time limit.
- Item assets have stable content IDs and a medicine-material flag; herb codex Sprite references remain optional until final codex art is supplied. Recipes retain direct ingredient/result asset references.
- `SO_StageMinigameUiText` contains the role-specific copy. The `PF_PlayerHud` contains the panel and text references; no UI objects are created during play.
- The supplied PDF's data-sheet sections contain no herb or potion values. Current appearance descriptions, sequences, and recipe numbers are prototype content to tune with design.

## Verification

The original minigame checks covered wrong and correct gathering directions, two-party transfer, successful brewing and overheat failure. In the modeling scene's Play mode, both offline actors were active in slots 0 and 1 with their respective roles. Switching possession in both directions changed local control and the active camera; the screen remained in Personal mode. Unity reported no errors or warnings. The full gathering-to-brewing interaction should still be played through manually with E, F and Space.
