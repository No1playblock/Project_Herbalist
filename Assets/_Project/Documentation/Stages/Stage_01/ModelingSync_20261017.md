# Stage 1 모델링 브랜치 반영 (2026-10-17)

- 소스: `origin/feat-characterModeling` (`bf1dd7cc`), `origin/feat-environmentModeling` (`30806f7e`).
- 대상: `Stage_01_ModelingReference.unity`. 캐릭터 브랜치의 지형·암석·식생·조명 배치를 기준으로 합쳤다.
- 기획자가 조정한 버섯 점프대 13개, 천 7개, 가지 점프대 1개의 프리팹 인스턴스 블록을 기존 씬과 동일하게 보존했다. 잎 설치 대상의 씬 설정도 유지했다.
- 새 씬이 참조하는 모델·재질·텍스처 등 217개와 두 브랜치의 Stage 1 환경 에셋을 가져왔다. 아이비 오브젝트에 필요한 Real Ivy 공통 스크립트도 함께 가져왔다. 큰 TGA 파일은 Git LFS로 관리한다.
- 캐릭터 아트 경로의 변경 82개는 실제 내용이 동일한 LFS 전환이므로 중복 반영하지 않았다. `feat-environmentModeling`의 최신 Stage 2 전용 에셋은 이 Stage 1 동기화에 포함하지 않았다.
- 모델링 브랜치에 원본 프리팹이 없는 나무 LOD 그룹 2개, ProBuilder와 누락된 셰이더를 쓰는 숲 장식 조명 12개, 모델링용 `player_position` 1개는 씬에서 제외했다. 지형의 누락된 재질 참조는 기존 `Terrain_Grass.mat`으로 연결했다.

정적 검증: 점프대·천 프리팹 21개 설정 일치, 씬 내부 참조 누락 0, 중복 에셋 GUID 0. URP Core의 `Volume` 스크립트 GUID는 패키지 캐시에 속하므로 `Assets` 메타 파일 검사에는 나타나지 않는다. Unity 에디터에서 씬을 열어 배치·충돌·재질을 최종 확인해야 한다.

## 추가 동기화 (2026-10-19)

- `origin/feat-characterModeling`의 최신 `ce8ea60f`까지 확인했다. 1스테이지에서 실제 사용하는 `01_Mushroom` 재질과 텍스처를 가져와 기존 버섯 모델의 재질 참조에 연결했다. 원본 브랜치의 모델 임포트 배율 변경은 점프대 크기에 영향을 주므로 적용하지 않았다.
- 이전에 빠졌던 숲 장식 조명 12개를 같은 씬 배치로 복원하고 ProBuilder 6.1.3을 추가했다. 원본 조명 재질의 셰이더 참조가 원본 브랜치에서도 누락되어 있어, 현 프로젝트에서 사용할 수 있는 URP 파티클 셰이더로 복구한 별도 재질에 연결했다.
- 원본 프리팹 참조가 깨진 나무 LOD 그룹 2개는 현 프로젝트의 정상 프리팹으로 같은 위치와 크기에 배치했다. 새 나무 충돌체는 꺼서 기존 플레이어 동선을 보존했다.
- 모델링 브랜치의 그래픽·품질 설정을 반영했다. 원본 그래픽 설정의 누락된 화면 공간 그림자 셰이더 참조는 기존 유효한 설정을 유지했다. Stage 1에서 쓰지 않는 데모·애니메이션·버섯 변형 에셋은 가져오지 않았다. 플레이어 크기·튜닝, 점프대·천 기믹, 지형 복구 데이터와 갈대 재질 복구는 유지했다.
- Unity 6.6에서 씬을 다시 열어 검증한 결과, 씬 누락 스크립트/프리팹 0개, Console 오류 0개였다.

## Terrain Paint Details 복구 (2026-10-19)

- `New Terrain 3.asset`의 Detail 16칸 중 Missing이던 14칸을 조사했다. 누락 GUID가 가리키는 프리팹·꽃/갈대 텍스처 15개와 이들이 참조하는 모델·재질·셰이더 파일을 `feat-characterModeling`에서 가져왔다. 꽃 TGA 4개는 원본 Git LFS 객체로 복원했다.
- `StylizedGrassShader.shader`의 상대 경로 include인 `Shaders/Libraries/*.hlsl`과 각 Pass 파일도 가져왔다. GUID 의존성만 복사했을 때 발생한 `Libraries/Input.hlsl` 누락 컴파일 오류는 최종 검사에서 해소됐다.
- TerrainData 자체와 Detail 배치 데이터는 변경하지 않았다. Unity 6.6 배치 검사에서 `New Terrain 3`의 Detail 16칸 모두 모델 또는 텍스처 경로가 해석되며 Missing은 0개였다. 다른 4개 TerrainData의 Detail 목록도 Missing 0개였다.
