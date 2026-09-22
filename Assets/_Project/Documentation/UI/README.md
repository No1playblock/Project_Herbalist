# Shared game UI

Approved A architecture: preauthored UGUI prefabs instantiated into scenes in the Editor; runtime uses SetActive. No UI is created during gameplay.

## Prefabs and scenes
- MainMenuScreen: home, room-code entry, two-character selection, loading and options instances.
- OptionsPanel: persistent local volume and mouse-sensitivity preferences.
- LoadingPanel: illustration and activity indicator; no fabricated percentage.
- GameOverlayHud: stage name/objective, subtitle hook, shared pause menu and options/loading.
- PlayerHud: per-viewport ability markers, help, held item, feedback and interaction prompt.
- Integrated into MainMenu, Sandbox_Abilities, Stage_01_Exterior and Stage_02_Interior.
- Existing prototype UI stays disabled for rollback/reference. Edit prefab sources for common changes.
- GameUiBuilder is a one-time authoring tool, not a runtime builder. The saved prefabs/scenes are the source of truth.

## Session rules
RoomControl owns distinct character selection and shared pause through Fusion reliable control messages.
Host validates membership, duplicate selections and pause ownership. Starting requires two distinct choices and the Host button.
PlayerSpawnLayout resolves selected roles independently of connection order; the existing stage transition preserves role and potion ability.
A single pause owner freezes player movement, ability simulation/lifetimes, progression and character animation while network transport continues.
Only the requesting peer can resume. If that peer disconnects the Host clears the pause.
Esc opens/resumes pause; H toggles the local key guide; Tab retains the existing split/personal toggle.
Bindings live in InputAction assets and labels read the actual bindings. Volume/look preferences are local PlayerPrefs.

## Presentation and content
Menu/loading artwork is cropped from the supplied game-screen PDF. It is reference art, not newly commissioned final art.
Character cards use neutral silhouettes pending final portraits. Ability markers use text badges pending final icons.
NanumGothic is included under SIL OFL; see Assets/_Project/Fonts/NanumGothic/OFL.txt.
Stage One settings and item names are localized into Korean.
GameOverlayHud.ShowSubtitle is a hook for future dialogue/narration; no scripted dialogue timeline is fabricated.
The current options include volume and mouse sensitivity; further option content is not specified in the PDF.
See Assets/_Project/Documentation/Player/PrototypeModeMenu.md for removal of the temporary solo entry.

## Verification
Development Host/Client test (-herbalist-ui-check host|client ROOM) passed:
waiting instead of auto-start, duplicate choice rejection, Host-selected Sodam spawning in role slot 1,
pause replication, frozen Host position, rejection of non-owner resume and successful resume on both peers.
Menu/options/room/personal HUD/pause screenshots were inspected in Play mode.
The original solo sandbox currently has pink environment materials; this UI task does not replace those materials.
