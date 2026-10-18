# Stage 1 모델링 브랜치 반영 (2026-10-17)

- 소스: `origin/feat-characterModeling` (`bf1dd7cc`), `origin/feat-environmentModeling` (`30806f7e`).
- 대상: `Stage_01_ModelingReference.unity`. 캐릭터 브랜치의 지형·암석·식생·조명 배치를 기준으로 합쳤다.
- 기획자가 조정한 버섯 점프대 13개, 천 7개, 가지 점프대 1개의 프리팹 인스턴스 블록을 기존 씬과 동일하게 보존했다. 잎 설치 대상의 씬 설정도 유지했다.
- 새 씬이 참조하는 모델·재질·텍스처 등 217개와 두 브랜치의 Stage 1 환경 에셋을 가져왔다. 아이비 오브젝트에 필요한 Real Ivy 공통 스크립트도 함께 가져왔다. 큰 TGA 파일은 Git LFS로 관리한다.
- 캐릭터 아트 경로의 변경 82개는 실제 내용이 동일한 LFS 전환이므로 중복 반영하지 않았다. `feat-environmentModeling`의 최신 Stage 2 전용 에셋은 이 Stage 1 동기화에 포함하지 않았다.
- 모델링 브랜치에 원본 프리팹이 없는 나무 LOD 그룹 2개, ProBuilder와 누락된 셰이더를 쓰는 숲 장식 조명 12개, 모델링용 `player_position` 1개는 씬에서 제외했다. 지형의 누락된 재질 참조는 기존 `Terrain_Grass.mat`으로 연결했다.

정적 검증: 점프대·천 프리팹 21개 설정 일치, 씬 내부 참조 누락 0, 중복 에셋 GUID 0. URP Core의 `Volume` 스크립트 GUID는 패키지 캐시에 속하므로 `Assets` 메타 파일 검사에는 나타나지 않는다. Unity 에디터에서 씬을 열어 배치·충돌·재질을 최종 확인해야 한다.
