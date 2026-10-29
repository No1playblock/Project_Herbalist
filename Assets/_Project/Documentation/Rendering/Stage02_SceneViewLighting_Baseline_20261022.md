# Stage 02 조명 변경 전 설정 (2026-10-22)

대상: `Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity`. 게임 뷰를 씬 뷰에 가깝게 조정하기 **전** 에디터에서 읽은 값이다. 1스테이지 씬이나 공용 렌더 파이프라인 에셋에는 적용하지 않는다.

## 2스테이지 원래 값

| 항목 | 변경 전 값 |
| --- | --- |
| RenderSettings ambientMode | Flat (`m_AmbientMode: 3`) |
| ambientLight / ambientSkyColor | `(0.023153365, 0.009134057, 0.0047769533, 1)` |
| ambientEquatorColor | `(0.114, 0.125, 0.133, 1)` |
| ambientGroundColor | `(0.047, 0.043, 0.035, 1)` |
| ambientIntensity | `1` |
| fog | 꺼짐 (`m_Fog: 0`), 색 `(0.5, 0.5, 0.5, 1)`, density `0.01`, linear range `0–300` |
| skyboxMaterial | Unity 기본 내장 Skybox (`fileID: 10304`, `guid: 0000000000000000f000000000000000`) |
| reflectionIntensity | `0.2` |
| MazeFillLight_0 | 활성, Directional, 흰색, intensity `0.75`, rotation `(0,0,0)`, 그림자 없음 |
| MazeFillLight_1 | 활성, Directional, 흰색, intensity `0.75`, rotation `(0,180,0)`, 그림자 없음 |
| Directional Light | 활성, Directional, color `(1,0.957,0.839,1)`, intensity `1.2`, rotation `(45,330,0)`, Soft shadow, strength `1` |
| PlayerCamera | FOV `55`, clip `0.1–150`, HDR/MSAA 켜짐, Skybox clear, background `(0.192,0.302,0.475,0)`; 별도 URP camera data/후처리 없음 |
| URP asset | `Assets/_Project/Settings/Rendering/PC_RPAsset.asset` (공용, 이 작업에서 변경하지 않음) |

씬 뷰는 Scene Lighting이 **꺼져** 있고, Scene Effects/Post toggle은 켜져 있다. 2스테이지에는 Volume 컴포넌트가 없다. 따라서 당시 두 뷰 차이의 주 원인은 포스트 프로세싱보다는 서로 다른 조명 경로다.

## 1스테이지 참고값 (변경하지 않음)

`Stage_01_ModelingReference.unity`의 RenderSettings: fog 켜짐, density `0.002`, fog color `(0.41509432,0.41509432,0.41509432,1)`; ambientMode `Skybox` (`m_AmbientMode: 0`), ambientSkyColor 흰색, ambientIntensity `1.44`, 전용 Skybox GUID `123caa41e84e041d0bd65bb3ae888dd1`. 이 설정은 2스테이지 원래 설정과 다르므로 독립적으로 유지한다.

## 복원

2스테이지 씬의 Lighting > Environment에서 위 RenderSettings를 되돌리고, Hierarchy의 조명 세 개 Inspector 값을 표대로 복원한다. 카메라와 공용 URP 에셋은 이 작업의 변경 대상이 아니다.

## 적용한 조정

`Stage_02_Mazes.unity`의 Flat 환경광 `ambientLight`/`ambientSkyColor`만 중성 회색 `(0.45, 0.45, 0.45, 1)`로 변경했다. 뗏목과 미로 재질, 세 조명, 카메라, 공용 URP 설정 및 1스테이지 씬 값은 유지했다. 플레이 화면과 씬 뷰의 같은 방향 캡처를 `Temp/Stage2RenderCompare/`에 남겨 확인했다(이 임시 폴더는 Git 추적 대상이 아니다).
