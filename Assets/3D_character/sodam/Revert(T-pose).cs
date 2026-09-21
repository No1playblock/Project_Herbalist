using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SyncBoneTransforms : EditorWindow
{
    public GameObject targetCharacter; // 찌그러졌지만 스프링본이 들어있는 캐릭터
    public GameObject cleanFBX;        // 씬에 새로 내린 원본 T-포즈 FBX

    [MenuItem("Tools/뼈대 T-포즈 강제 동기화")]
    public static void ShowWindow()
    {
        GetWindow<SyncBoneTransforms>("T-포즈 동기화");
    }

    private void OnGUI()
    {
        GUILayout.Label("스프링본 세팅 유지 ➔ 뼈 위치만 T-포즈로 덮어쓰기", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetCharacter = (GameObject)EditorGUILayout.ObjectField("1. 포즈 꼬인 캐릭터 (기존)", targetCharacter, typeof(GameObject), true);
        cleanFBX = (GameObject)EditorGUILayout.ObjectField("2. 원본 T-포즈 FBX (신규)", cleanFBX, typeof(GameObject), true);

        EditorGUILayout.Space();

        if (GUILayout.Button("뼈 위치 T-포즈로 강제 복원", GUILayout.Height(40)))
        {
            if (targetCharacter == null || cleanFBX == null)
            {
                EditorUtility.DisplayDialog("알림", "두 오브젝트를 모두 드래그해서 넣어주세요!", "확인");
                return;
            }

            ResetBoneTransforms(targetCharacter, cleanFBX);
        }
    }

    private static void ResetBoneTransforms(GameObject target, GameObject source)
    {
        // 원본 FBX의 자식 본 트랜스폼 수집
        Dictionary<string, Transform> sourceBones = new Dictionary<string, Transform>();
        foreach (Transform t in source.GetComponentsInChildren<Transform>(true))
        {
            if (!sourceBones.ContainsKey(t.name))
            {
                sourceBones.Add(t.name, t);
            }
        }

        Undo.RegisterFullObjectHierarchyUndo(target, "Sync Bone Transforms");
        int count = 0;

        // 기존 캐릭터의 모든 본 위치/회전/스케일을 원본 FBX 수치로 강제 덮어쓰기
        foreach (Transform targetT in target.GetComponentsInChildren<Transform>(true))
        {
            if (sourceBones.TryGetValue(targetT.name, out Transform srcT))
            {
                targetT.localPosition = srcT.localPosition;
                targetT.localRotation = srcT.localRotation;
                targetT.localScale = srcT.localScale;
                count++;
            }
        }

        EditorUtility.DisplayDialog("완료", $"{count}개 뼈의 위치와 회전값이 T-포즈로 정렬되었습니다!", "확인");
    }
}