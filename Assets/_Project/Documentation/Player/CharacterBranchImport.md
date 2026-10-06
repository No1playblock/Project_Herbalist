# Character branch assets (2026-10-05)

- `feat-dooyoungModeling` (`dd517e6`): Duyeong V5 model, prefab, skinning, spring bones, animation FBXs/controller, textures, materials and lilToon presets are under `Assets/_Project/Art/Characters/Duyeong`.
- `chore-sodamCharacter` (`8cad8ff`): the latest Sodam V12 model, prefabs, spring bones, animation FBXs, textures, lilToon materials and character shader assets are under `Assets/_Project/Art/Characters/sodam/BranchV12`. This separate folder preserves its GUIDs and leaves the existing Sodam V3 player visual intact.
- The existing `Packages/jp.lilxyzw.liltoon` is already version 2.3.4, matching the source branch, so it was not copied again.
- The branch's unreferenced `Revert(T-pose).cs` EditorWindow was omitted because it sits outside an Editor assembly and references `UnityEditor`. Branch test scenes, root Volume profile and unrelated project settings were not imported.

Both imported character prefabs load with valid Humanoid avatars, no missing scripts and no missing materials. Their runtime materials use lilToon variants without outline; both model hierarchies use the Default layer.

The Sodam V12 import initially retained some GUID-resolved references to the older blue outfit textures and URP/embedded materials. Its `BranchV12` lilToon materials now reference the imported pink textures and normals, and both V12 source prefabs assign lilToon materials to the remaining skirt, inner mesh, hair and eye slots. The three player prefabs inherit those fixes. Offline Stage One verification showed the pink outfit; all 27 active Sodam material slots were present and none enabled outline.

`PF_Player`, `PF_NetworkPlayer` and `PF_StageNetworkPlayer` now contain the imported Sodam V12 and Duyeong V5 visual prefabs. Offline play shows Sodam; Fusion slot 0 shows Duyeong and slot 1 shows Sodam. The existing `PlayerCharacterPresentation` owns role visibility and drives only the selected Animator, while the CharacterController motor retains movement authority. Root motion is disabled. Both player models use `AC_SodamLocomotion`, restored to its earlier Idle/Walking animation setup on 2026-10-06 because the newly imported running and jumping clips did not fit the characters. The supplied FBXs remain in `Assets/_Project/Animations/Characters/Sodam/Imported` for future adjustment.

The offline player objects embedded directly in `Stage_02_Mazes`, `Stage_03_Altar`, and `Stage_03_BranchRide` also use the `BranchV12/Prefabs/sodamv12.prefab` visual. These scenes previously retained an unpacked V3 visual even though the reusable player prefabs had been upgraded.

The offline player prefab had a missing `SapPlacementPreview` component and inactive preview hierarchy from the removed Placement mode. They were removed so Unity could save the updated prefab.
