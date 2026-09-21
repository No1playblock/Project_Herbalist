#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class LatticePlayModeCheck
{
    static LatticePlayModeCheck()
    {
        EditorApplication.playModeStateChanged += CheckLatticeEditMode;
    }

    private static void CheckLatticeEditMode(PlayModeStateChange state)
    {
        // 只在即将进入Play模式时检查
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 查找场景中所有的Lattice组件
            Lattice[] lattices = GameObject.FindObjectsOfType<Lattice>();
            
            foreach (Lattice lattice in lattices)
            {
                // 如果有任何一个Lattice处于Edit Mode
                if (lattice.isEditModeActive)
                {
                    // 取消进入Play模式
                    EditorApplication.isPlaying = false;
                    
                    // 显示警告对话框
                    EditorUtility.DisplayDialog(
                        "Edit Mode Active", 
                        "Cannot enter Play mode while a Lattice component is in Edit Mode. Please exit Edit Mode on all Lattice components first.", 
                        "OK");
                    
                    // 选中有问题的Lattice对象，方便用户操作
                    Selection.activeGameObject = lattice.gameObject;
                    
                    break;
                }
            }
        }
    }
}
#endif