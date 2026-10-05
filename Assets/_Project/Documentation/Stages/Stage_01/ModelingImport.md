# Stage 1 modeling import

`feat-stage1Modeling` (`dd12b91`)의 Stage 1 모델링 씬을 `Stage_01_ModelingReference.unity`로 가져왔다. 현재 플레이 씬 `Stage_01_Exterior_260928.unity`를 덮어쓰지 않는 참고 씬이다. 원본 브랜치는 현행 `Assets/_Project` 게임 코드와 씬을 대량 삭제하므로 전체 병합 대신 이 씬과 사용 자산만 옮겼다.

- 씬: `Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference.unity`
- 나무·바위·갈대 모델 및 재질: `Assets/_Project/Art/Environments/Stage_01/Stage1_Assets`
- 지형 데이터: `Assets/_Project/Data/Stages/Stage_01/Stage1_ModelingTerrain.asset`
- 지형 레이어: `Assets/_Project/Data/Stages/Stage_01/TerrainLayers/NewLayer.terrainlayer`과 `NewLayer 1.terrainlayer` (각각 `Terrain_3.png`, `Terrain_2.png`를 참조)
- 씬이 참조하는 수풀·하늘 에셋: `Assets/Raygeas/Suntail Village`와 `Assets/Free Stylized Hand-Painted Skybox`의 필요한 파일만 포함

Unity `.meta` GUID를 보존해 이동했다. 가져온 파일 243개의 참조 검사에서 중복·미해결 GUID가 없었다. 원본 브랜치에서도 누락돼 있던 나무와 수풀 재질의 텍스처 슬롯은 같은 재질에 이미 연결된 베이스 텍스처로 교체했다. 실제 플레이 씬에 모델을 배치하고 충돌·동선·성능을 확인하는 작업은 별도로 진행한다.
