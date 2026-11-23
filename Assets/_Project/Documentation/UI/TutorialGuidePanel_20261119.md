# Tutorial callout panel and spaced ring

Reference: the two embedded tutorial examples in `D:/download/초유록 텍스트 (1).xlsx`. Only the tutorial callout styling is applied; gameplay conditions and quest content are unchanged.

## Sprite

- Asset: `Assets/_Project/Art/UI/Textures/Tutorial/UI_TutorialCaption_9Slice.png`
- Generated with the built-in image generation tool; genuine alpha is preserved. No text is baked into the image.
- Source: 2172 x 724. Sprite rect: x=31, y=130, width=2111, height=467 (Unity bottom-left coordinates).
- Border: left/right=160 px, bottom/top=120 px. Pixels per unit=400.
- `GuideCaption` uses Image.Type.Sliced, with the center filled. The four corner decorations are protected while the center and straight edges stretch.
- `GuideText` remains TextMeshPro. Caption size follows preferred text dimensions with serialized padding, minimum/maximum size and viewport limits.
- Existing authored caption objects are reused in PF_GameOverlayHud and all three gameplay scenes. No runtime UI hierarchy is created.

## Ring

The existing SpotlightWorldRing_Setup remains the source of ring settings. Dash count=18, dash fraction=0.44, core width=0.04, glow width=0.10. The old 25/0.62/0.18 combination made neighboring glow capsules overlap. The new gap is about 0.155 of unit radius between core ends (previously about 0.055), before perspective scaling.

## Verification and editing

The official builtin `com.unity.2d.sprite@1.0.0` package was added through Package Manager so Sprite Editor metadata can be edited using the supported data-provider API. Frame-edit capabilities were checked before setting rect, pivot, border and sprite name mapping.

The saved prefab/scene uses an enabled caption Image. Text is centered, uses 22-point maximum/18-point minimum sizing, and has 44-unit horizontal/16-unit vertical inset. The panel chooses its size from the text's preferred dimensions, bounded by serialized limits and the owner's viewport; text measurements are cached until content or viewport changes.

Live checks at 1920x1080 (Canvas units):

| Case | Panel size | Text lines | Result |
| --- | --- | ---: | --- |
| Normal guide | 453.43 x 76 | 1 | No overflow, inside owner viewport |
| Short guide | 281.39 x 76 | 1 | No overflow, corners preserved |
| Long guide | 552.88 x 127.57 | 3 | No overflow, corners preserved |
| Long guide in split view | 508.22 x 127.57 | 3 | Inside 960x1080 owner viewport |

Captures and measurements: `Logs/TutorialPanelReview`. The normal case is activated through the real Stage One tutorial logic in a temporary potion-complete play fixture; subsequent length cases change only the shown text. No fixture progress or camera pose is saved to scenes.

## Generation prompt

Use case: stylized-concept. Asset type: production-ready Unity uGUI 9-slice tutorial caption background for a Korean woodland fantasy cooperative game. Create ONE isolated, wide horizontal empty dialogue/tutorial panel, viewed perfectly front-on, with REAL transparent surroundings. Match a refined illustrated woodland game UI: nearly black charcoal and very dark forest-green translucent-looking interior, very thin warm antique-gold inset outline, a restrained cool cyan/teal edge light around the exterior, delicate tiny gold curled corner details only. Soft rounded rectangle, width roughly 4.5 times its height, centered with modest transparent padding. The panel should feel elegant and quiet over a 3D forest scene. Interior must be empty and very dark with only a faint uniform fine texture, for white Korean text to be rendered separately by the game. IMPORTANT 9-SLICE RULES: all ornament confined to the four corners; long top and bottom edges are straight, thin and uniform; center 65 percent is uniform and fully stretchable in BOTH axes; left/right edge middle sections uniform; no decorative motifs in stretchable center; no asymmetric shading or perspective. No text, no letters, no icons, no title badge, no pointer tail, no arrows, no ring, no mockup scene, no montage, no watermark. One complete single panel sprite only, with all edges and corner glow inside image, no cropped edges. High-quality hand-painted 2D fantasy game UI asset with crisp borders and a generous clear reading area.
