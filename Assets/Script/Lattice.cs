using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Lattice : MonoBehaviour
{
    // BEGIN DYNAMIC PROPERTIES
    [HideInInspector]
    public Vector3 ControlPoint_0 = new Vector3(-1.029718f, 0.1720397f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_1 = new Vector3(-1.029718f, 0.1720397f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_2 = new Vector3(-1.029718f, 0.1720397f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_3 = new Vector3(-1.029718f, 0.1720397f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_4 = new Vector3(-1.029718f, 0.581674f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_5 = new Vector3(-1.029718f, 0.581674f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_6 = new Vector3(-1.029718f, 0.581674f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_7 = new Vector3(-1.029718f, 0.581674f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_8 = new Vector3(-1.029718f, 0.9913082f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_9 = new Vector3(-1.029718f, 0.9913082f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_10 = new Vector3(-1.029718f, 0.9913082f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_11 = new Vector3(-1.029718f, 0.9913082f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_12 = new Vector3(-1.029718f, 1.400942f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_13 = new Vector3(-1.029718f, 1.400942f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_14 = new Vector3(-1.029718f, 1.400942f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_15 = new Vector3(-1.029718f, 1.400942f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_16 = new Vector3(0f, 0.1720397f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_17 = new Vector3(0f, 0.1720397f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_18 = new Vector3(0f, 0.1720397f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_19 = new Vector3(0f, 0.1720397f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_20 = new Vector3(0f, 0.581674f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_21 = new Vector3(0f, 0.581674f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_22 = new Vector3(0f, 0.581674f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_23 = new Vector3(0f, 0.581674f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_24 = new Vector3(0f, 0.9913082f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_25 = new Vector3(0f, 0.9913082f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_26 = new Vector3(0f, 0.9913082f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_27 = new Vector3(0f, 0.9913082f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_28 = new Vector3(0f, 1.400942f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_29 = new Vector3(0f, 1.400942f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_30 = new Vector3(0f, 1.400942f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_31 = new Vector3(0f, 1.400942f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_32 = new Vector3(1.029718f, 0.1720397f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_33 = new Vector3(1.029718f, 0.1720397f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_34 = new Vector3(1.029718f, 0.1720397f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_35 = new Vector3(1.029718f, 0.1720397f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_36 = new Vector3(1.029718f, 0.581674f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_37 = new Vector3(1.029718f, 0.581674f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_38 = new Vector3(1.029718f, 0.581674f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_39 = new Vector3(1.029718f, 0.581674f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_40 = new Vector3(1.029718f, 0.9913082f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_41 = new Vector3(1.029718f, 0.9913082f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_42 = new Vector3(1.029718f, 0.9913082f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_43 = new Vector3(1.029718f, 0.9913082f, 2.391312f);

    [HideInInspector]
    public Vector3 ControlPoint_44 = new Vector3(1.029718f, 1.400942f, -2.429266f);

    [HideInInspector]
    public Vector3 ControlPoint_45 = new Vector3(1.029718f, 1.400942f, -0.8224069f);

    [HideInInspector]
    public Vector3 ControlPoint_46 = new Vector3(1.029718f, 1.400942f, 0.7844527f);

    [HideInInspector]
    public Vector3 ControlPoint_47 = new Vector3(1.029718f, 1.400942f, 2.391312f);

    // END DYNAMIC PROPERTIES
// 在Lattice类中添加这些变量
[HideInInspector]
public bool isRecordingAnimation = false; // 表示是否正在记录动画
[HideInInspector]
public bool updateKeyframesWhenDragging = true; // 是否在拖动时更新关键帧

// 在Lattice类中添加这个方法，用于在拖动点时更新动画关键帧
public void UpdateAnimationKeyframes()
{
#if UNITY_EDITOR
    if (!isRecordingAnimation || !dynamicPropertiesCreated)
        return;
        
    // 获取当前选中的游戏对象
    UnityEngine.Object targetObject = this;
    
    // 获取Animation窗口
    System.Type animEditorType = System.Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
    if (animEditorType == null)
        return;
        
    // 获取当前打开的Animation窗口
    EditorWindow animWindow = null;
    MethodInfo getWindowMethod = animEditorType.GetMethod("GetWindow", 
                                     BindingFlags.Public | BindingFlags.Static);
    if (getWindowMethod != null)
    {
        animWindow = getWindowMethod.Invoke(null, null) as EditorWindow;
    }
    
    if (animWindow == null)
        return;
        
    // 获取AnimationWindow的state属性
    PropertyInfo stateProperty = animEditorType.GetProperty("state", 
                                   BindingFlags.NonPublic | BindingFlags.Instance);
    if (stateProperty == null)
        return;
        
    object state = stateProperty.GetValue(animWindow);
    if (state == null)
        return;
        
    // 获取ActiveRecordMode属性，检查是否在记录模式
    PropertyInfo recordingProperty = state.GetType().GetProperty("recording");
    if (recordingProperty == null)
        return;
        
    bool isRecording = (bool)recordingProperty.GetValue(state);
    if (!isRecording)
        return;
        
    // 获取当前编辑的AnimationClip
    PropertyInfo activeAnimationClipProperty = state.GetType().GetProperty("activeAnimationClip");
    if (activeAnimationClipProperty == null)
        return;
        
    AnimationClip activeClip = activeAnimationClipProperty.GetValue(state) as AnimationClip;
    if (activeClip == null)
        return;
    
    // 获取当前时间
    PropertyInfo currentTimeProperty = state.GetType().GetProperty("currentTime");
    if (currentTimeProperty == null)
        return;
        
    float currentTime = (float)currentTimeProperty.GetValue(state);
    
    // 为所有选中的控制点更新关键帧
    foreach (int index in selectedPoints)
    {
        string propertyPath = $"ControlPoint_{index}";
        
        // 使用反射获取属性的值
        FieldInfo field = this.GetType().GetField(propertyPath);
        if (field != null)
        {
            Vector3 value = deformControlPoints[index];
            
            // 设置关键帧
            Undo.RecordObject(activeClip, "Update Animation Keyframe");
            
            // 为x,y,z分别设置关键帧
            AnimationUtility.SetEditorCurve(
                activeClip,
                EditorCurveBinding.FloatCurve(GetGameObjectPath(this.gameObject), this.GetType(), $"{propertyPath}.x"),
                AddKeyToClip(activeClip, $"{propertyPath}.x", currentTime, value.x)
            );
            
            AnimationUtility.SetEditorCurve(
                activeClip,
                EditorCurveBinding.FloatCurve(GetGameObjectPath(this.gameObject), this.GetType(), $"{propertyPath}.y"),
                AddKeyToClip(activeClip, $"{propertyPath}.y", currentTime, value.y)
            );
            
            AnimationUtility.SetEditorCurve(
                activeClip,
                EditorCurveBinding.FloatCurve(GetGameObjectPath(this.gameObject), this.GetType(), $"{propertyPath}.z"),
                AddKeyToClip(activeClip, $"{propertyPath}.z", currentTime, value.z)
            );
        }
    }
    
    // 通知AnimationWindow更新
    animWindow.Repaint();
#endif
}

// 辅助方法，为现有曲线添加或更新关键帧
private AnimationCurve AddKeyToClip(AnimationClip clip, string propertyName, float time, float value)
{
#if UNITY_EDITOR
    // 获取现有曲线
    EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
        GetGameObjectPath(this.gameObject), this.GetType(), propertyName);
    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
    
    if (curve == null)
        curve = new AnimationCurve();
    
    // 查找是否已经有该时间的关键帧
    int keyIndex = -1;
    for (int i = 0; i < curve.keys.Length; i++)
    {
        if (Mathf.Approximately(curve.keys[i].time, time))
        {
            keyIndex = i;
            break;
        }
    }
    
    // 添加或更新关键帧
    Keyframe keyframe;
    if (keyIndex >= 0)
    {
        keyframe = curve.keys[keyIndex];
        keyframe.value = value;
        curve.MoveKey(keyIndex, keyframe);
    }
    else
    {
        keyframe = new Keyframe(time, value);
        curve.AddKey(keyframe);
    }
    
    return curve;
#else
    return null;
#endif
}

// 辅助方法，获取游戏对象的路径
private string GetGameObjectPath(GameObject obj)
{
    string path = obj.name;
    Transform parent = obj.transform.parent;
    
    while (parent != null)
    {
        path = parent.name + "/" + path;
        parent = parent.parent;
    }
    
    return path;
}
    // mesh and control points
    public Mesh modelMesh;
    public Mesh bakedMesh;
    [SerializeField] private Mesh originalMesh; // 保存最初的网格
    [HideInInspector]
    public List<Vector3> vertexCoordinates = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> deformControlPoints = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> originalPoints = new List<Vector3>(); // 保存原始控制点位置

    // max and min point of mesh
    [HideInInspector]
    public Vector3 minPoint;
    [HideInInspector]
    public Vector3 maxPoint;

    // width, depth height and number of CPs in each
    [HideInInspector]
    public float width, height, depth;
    public Vector3Int resolution = new Vector3Int(3, 4, 4);
    private float[] bernsteinCoeffsX;
    private float[] bernsteinCoeffsY;
    private float[] bernsteinCoeffsZ;

    // visual properties
    public Material lineMaterial;
    public Color lineColor = new Color32(0, 181, 255, 80);
    public Color pointColor = new Color32(0, 181, 255, 166);
    public Color selectedPointColor = new Color32(255, 215, 0, 200); // Gold color for selected points

    [HideInInspector]
    public float lineWidth = 2f;
    [HideInInspector]
    public float controlPointPixelSize = 10f;

    // visibility flags
    [HideInInspector]
    public bool showControlPoints = true;
    [HideInInspector]
    public bool showConnectionLines = true;

    // simulation state
    [HideInInspector]
    public bool isEditModeActive = false;

    // selection and interaction
    private int selectedPointIndex = -1;
    private Vector3 dragOffset;
    private Camera mainCamera;

    // Box selection
    private bool isBoxSelecting = false;
    private Vector2 boxSelectStart;
    private Vector2 boxSelectCurrent;
    private HashSet<int> selectedPoints = new HashSet<int>(); // Selected points indices
    private bool isDraggingSelectedPoints = false;
    private Dictionary<int, Vector3> selectedPointsOffsets = new Dictionary<int, Vector3>();

    private Vector3Int previousResolution;
    private MeshFilter meshFilter;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    [HideInInspector]
    public bool isSkinnedMesh = false;
    private MeshRenderer meshRenderer;
    private bool originalMeshRendererState;
    private bool initialized = false;
    private bool permanentlySaved = false; // 跟踪是否已永久保存变形

    // 在Lattice类中添加这个新的布尔变量
    [HideInInspector]
    public bool deformEveryFrame = true; // 默认每帧变形
    [HideInInspector]
    public bool needsDeform = false; // 用于标记是否需要变形

    // 用于跟踪是否已创建动态属性
    [HideInInspector]
    public bool dynamicPropertiesCreated = false;
    
    // 同步控制
    [HideInInspector]
    public bool syncFromAnimation = true;
    
    // 为了记住创建的属性数量
    [HideInInspector]
    public int createdPropertyCount = 0;

    // Face selection
    public enum FaceSelectionMode
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }
    [HideInInspector]
    public FaceSelectionMode currentFaceSelection = FaceSelectionMode.None;
    private HashSet<int> currentFacePoints = new HashSet<int>(); // Points on the currently selected face

    // 添加高亮面的颜色
    public Color highlightedFaceColor = new Color32(255, 100, 100, 120); // Red color for highlighted face

    void Awake()
    {
        FindMainCamera();
        InitializeMaterials();
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
        else
        {
            // 在运行时也需要订阅Scene GUI事件
            SceneView.duringSceneGui += OnSceneGUI;
        }
#endif

        // 确保我们始终有正确的初始化，无论是在编辑模式还是运行时
        if (!initialized)
        {
            Initialize();
        }
    }

    void Start()
    {
        previousResolution = resolution;

        // 确保我们在运行时开始时就初始化，不需要先在编辑模式下开启
        if (Application.isPlaying && !initialized)
        {
            Initialize();

            // 在运行时自动启动编辑模式
            if (deformControlPoints.Count == 0)
            {
                UpdateLattice();
            }
            isEditModeActive = true;
#if UNITY_EDITOR
            // 确保Scene视图在运行时也能看到控制点
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Repaint();
            }
#endif
        }
        
        // 初始化动态属性
        if (dynamicPropertiesCreated)
        {
            InitializeDynamicProperties();
        }
    }

    // 统一的初始化方法
    void Initialize()
    {
        InitializeMaterials();
        InitMesh();

        // 确保控制点被正确初始化
        deformControlPoints.Clear();
        originalPoints.Clear();

        // 清除选中点
        selectedPoints.Clear();
        selectedPointsOffsets.Clear();

        // 清除面选择
        currentFacePoints.Clear();
        currentFaceSelection = FaceSelectionMode.None;

        // 标记为已初始化
        initialized = true;
        permanentlySaved = false; // 初始化时重置保存状态
    }

    // 选择特定面的控制点
    public void SelectFace(FaceSelectionMode faceMode)
    {
        // 清除当前选择
        selectedPoints.Clear();
        currentFacePoints.Clear();

        // 如果选择的是None或者当前已经选择的面，则取消选择
        if (faceMode == FaceSelectionMode.None || faceMode == currentFaceSelection)
        {
            currentFaceSelection = FaceSelectionMode.None;
            return;
        }

        currentFaceSelection = faceMode;

        // 根据不同的面模式选择对应的控制点
        int i, j, k;
        switch (faceMode)
        {
            case FaceSelectionMode.Top:
                // 选择y值最大的面 (j = resolution.y - 1)
                j = resolution.y - 1;
                for (i = 0; i < resolution.x; i++)
                {
                    for (k = 0; k < resolution.z; k++)
                    {
                        int idx = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                        if (idx < deformControlPoints.Count)
                        {
                            currentFacePoints.Add(idx);
                        }
                    }
                }
                break;

            case FaceSelectionMode.Bottom:
                // 选择y值最小的面 (j = 0)
                j = 0;
                for (i = 0; i < resolution.x; i++)
                {
                    for (k = 0; k < resolution.z; k++)
                    {
                        int idx = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                        if (idx < deformControlPoints.Count)
                        {
                            currentFacePoints.Add(idx);
                        }
                    }
                }
                break;

            case FaceSelectionMode.Left:
                // 选择x值最小的面 (i = 0)
                i = 0;
                for (j = 0; j < resolution.y; j++)
                {
                    for (k = 0; k < resolution.z; k++)
                    {
                        int idx = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                        if (idx < deformControlPoints.Count)
                        {
                            currentFacePoints.Add(idx);
                        }
                    }
                }
                break;

            case FaceSelectionMode.Right:
                // 选择x值最大的面 (i = resolution.x - 1)
                i = resolution.x - 1;
                for (j = 0; j < resolution.y; j++)
                {
                    for (k = 0; k < resolution.z; k++)
                    {
                        int idx = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                        if (idx < deformControlPoints.Count)
                        {
                            currentFacePoints.Add(idx);
                        }
                    }
                }
                break;

            case FaceSelectionMode.Front:
                // 选择z值最大的面 (k = resolution.z - 1)
                k = resolution.z - 1;
                for (i = 0; i < resolution.x; i++)
                {
                    for (j = 0; j < resolution.y; j++)
                    {
                        int idx = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                        if (idx < deformControlPoints.Count)
                        {
                            currentFacePoints.Add(idx);
                        }
                    }
                }
                break;

            case FaceSelectionMode.Back:
                // 选择z值最小的面 (k = 0)
                k = 0;
                for (i = 0; i < resolution.x; i++)
                {
                    for (j = 0; j < resolution.y; j++)
                    {
                        int idx = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                        if (idx < deformControlPoints.Count)
                        {
                            currentFacePoints.Add(idx);
                        }
                    }
                }
                break;
        }

        // 将当前面点添加到选中点集合
        foreach (int pointIdx in currentFacePoints)
        {
            selectedPoints.Add(pointIdx);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }
        else
        {
            SyncViewsControlPoints();
        }
#endif
    }
#if UNITY_EDITOR
    private void OnDisable()
    {
        // 无论是编辑模式还是运行模式，都需要取消订阅Scene GUI事件
        SceneView.duringSceneGui -= OnSceneGUI;

        // 恢复原始渲染状态
        if (isSkinnedMesh && skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.enabled = true;
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = originalMeshRendererState;
        }
    }
#endif

    void FindMainCamera()
    {
        mainCamera = Camera.main;
#if UNITY_EDITOR
        if (mainCamera == null && !Application.isPlaying)
        {
            // In edit mode, use the scene view camera
            if (SceneView.lastActiveSceneView != null)
            {
                mainCamera = SceneView.lastActiveSceneView.camera;
            }
        }
#endif
    }

    void InitializeMaterials()
    {
        // Check if line material exists, otherwise create a basic one
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader != null)
            {
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }
    }

    void Update()
    {
        if (!Application.isPlaying && !isEditModeActive)
        {
            return;
        }

        FindMainCamera();

        // Check if resolution has changed
        if (resolution.x != previousResolution.x ||
            resolution.y != previousResolution.y ||
            resolution.z != previousResolution.z)
        {
            // Validate resolution values
            resolution.x = Mathf.Max(2, resolution.x);
            resolution.y = Mathf.Max(2, resolution.y);
            resolution.z = Mathf.Max(2, resolution.z);

            if (isEditModeActive || Application.isPlaying)
            {
                UpdateLattice();
            }
            previousResolution = resolution;
        }

        // Recreate Bernstein coefficient arrays if needed
        if (bernsteinCoeffsX == null || bernsteinCoeffsX.Length != resolution.x ||
            bernsteinCoeffsY == null || bernsteinCoeffsY.Length != resolution.y ||
            bernsteinCoeffsZ == null || bernsteinCoeffsZ.Length != resolution.z)
        {
            bernsteinCoeffsX = new float[resolution.x];
            bernsteinCoeffsY = new float[resolution.y];
            bernsteinCoeffsZ = new float[resolution.z];
        }

        // 确保至少有一个有效的网格和控制点集
        if ((modelMesh == null && bakedMesh == null) || deformControlPoints.Count == 0)
        {
            if (Application.isPlaying)
            {
                UpdateLattice();
            }
            return;
        }

        // For SkinnedMeshRenderer, bake the current pose into the mesh
        if (isSkinnedMesh && skinnedMeshRenderer != null && bakedMesh != null)
        {
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            UpdateMeshCoordinates(bakedMesh.vertices);
        }

        // 确保我们有可用的网格
        if (modelMesh == null)
        {
            if (isSkinnedMesh && bakedMesh != null)
            {
                modelMesh = bakedMesh;
            }
            else if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                modelMesh = meshFilter.sharedMesh;
            }
            else
            {
                return; // 没有可用的网格，退出
            }
        }
        
        // 如果已创建动态属性并启用同步，则在每帧同步动画值到实际控制点
        if (dynamicPropertiesCreated && syncFromAnimation && deformControlPoints.Count > 0)
        {
            SyncControlPointsFromProperties();
        }
        
        // 如果在手动拖动控制点，则更新动态属性
        if (isDraggingSelectedPoints && dynamicPropertiesCreated)
        {
            foreach (int index in selectedPoints)
            {
                if (index < deformControlPoints.Count)
                {
                    string propName = $"ControlPoint_{index}";
                    FieldInfo field = this.GetType().GetField(propName);
                    if (field != null)
                    {
                        field.SetValue(this, deformControlPoints[index]);
                    }
                }
            }
        }

        // 仅在需要变形的情况下执行变形操作
        if (deformEveryFrame)
        {
            // 自动变形模式
            ApplyDeformation();
        }
        else if (needsDeform && deformEveryFrame)
        {
            // 手动触发的变形，只执行一次
            ApplyDeformation();
            // 清除标志，不再自动变形直到下一次明确触发
            needsDeform = false;
        }

        // 确保网格可见
        if (isEditModeActive || Application.isPlaying)
        {
            if (isSkinnedMesh && skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.enabled = false;
            }

            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }
        }

        // Handle control point interaction only in play mode
        if (Application.isPlaying)
        {
            HandleInput();
        }
#if UNITY_EDITOR
        // Force the scene to repaint in edit mode
        if (!Application.isPlaying && isEditModeActive)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }

        // 确保Scene视图在运行时也能反映控制点变化
        if (Application.isPlaying && isEditModeActive)
        {
            SyncViewsControlPoints();
        }
#endif
    }

    private void ApplyDeformation()
    {
        Vector3[] verts;
        if (isSkinnedMesh && bakedMesh != null)
        {
            // Use the baked mesh vertices for skinned mesh
            verts = bakedMesh.vertices;
        }
        else if (modelMesh != null)
        {
            // Use the standard mesh for regular mesh filter
            verts = modelMesh.vertices;
        }
        else
        {
            return; // 没有顶点数据，退出
        }

        // 确保我们有足够的网格坐标
        if (vertexCoordinates.Count != verts.Length)
        {
            InitMesh(); // 重新初始化网格数据
        }

        Vector3[] newVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            newVerts[i] = EvalVertex(i);
        }

        // Apply the deformed vertices to the appropriate mesh
        if (isSkinnedMesh && bakedMesh != null)
        {
            if (newVerts.Length == bakedMesh.vertices.Length)
            {
                // 使用副本以避免修改原始网格
                Mesh deformedMesh = Instantiate(bakedMesh);
                deformedMesh.vertices = newVerts;
                deformedMesh.RecalculateNormals();
                deformedMesh.RecalculateBounds();

                // For skinned mesh, we need to update the mesh that's rendered
                if (meshFilter != null)
                {
                    meshFilter.mesh = deformedMesh;
                }
            }
        }
        else if (modelMesh != null)
        {
            if (newVerts.Length == modelMesh.vertices.Length)
            {
                // 创建一个副本以避免修改原始共享网格
                Mesh deformedMesh = Instantiate(modelMesh);
                deformedMesh.vertices = newVerts;
                deformedMesh.RecalculateNormals();
                deformedMesh.RecalculateBounds();

                if (meshFilter != null)
                {
                    meshFilter.mesh = deformedMesh;
                }
            }
        }
    }

#if UNITY_EDITOR
    // 添加一个新方法用于同步Scene和Game视图中的控制点
    public void SyncViewsControlPoints()
    {
        if (Application.isPlaying)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Repaint();
            }
        }
    }
#if UNITY_EDITOR
#if UNITY_EDITOR
private void OnSceneGUI(SceneView sceneView)
{
    // 这里应该只检查isEditModeActive，如果不在编辑模式中，根本不应该绘制任何内容
    if (!isEditModeActive) return;

    // Use the scene view camera for rendering
    mainCamera = sceneView.camera;

    Event e = Event.current;

    // 处理选择和拖动逻辑
    HandleBoxSelection(e, sceneView);

    // 只有当showControlPoints为true时才绘制控制点 
    // (注意：这个检查是多余的，因为如果isEditModeActive为true，我们应该总是显示控制点)
    if (showControlPoints)
    {
        for (int i = 0; i < deformControlPoints.Count; i++)
        {
            // 转换到世界空间
            Vector3 worldPos = transform.TransformPoint(deformControlPoints[i]);

            // 设置控制点颜色和大小
            Color handleColor;
            if (selectedPoints.Contains(i))
            {
                handleColor = selectedPointColor;
            }
            else if (currentFacePoints.Contains(i))
            {
                handleColor = highlightedFaceColor;
            }
            else
            {
                handleColor = (i == selectedPointIndex) ? Color.yellow : pointColor;
            }

            float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;

            // 绘制控制点并处理交互
            Handles.color = handleColor;

            // 如果点在选择集中，或者正在进行多选拖动，只绘制不交互
            if (selectedPoints.Contains(i) || isDraggingSelectedPoints)
            {
                Handles.SphereHandleCap(0, worldPos, Quaternion.identity, handleSize * 2, EventType.Repaint);
            }
            else // 如果不在选择集中，允许单点交互
            {
                EditorGUI.BeginChangeCheck();
                var fmh_328_67_638814674873568639 = Quaternion.identity; Vector3 newPos = Handles.FreeMoveHandle(worldPos, handleSize,
                    Vector3.zero, Handles.SphereHandleCap);

                // 如果位置发生变化，更新控制点
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Move Control Point");
                    deformControlPoints[i] = transform.InverseTransformPoint(newPos);
                    
                    // 如果有动态属性，同步到属性
                    if (dynamicPropertiesCreated)
                    {
                        string propName = $"ControlPoint_{i}";
                        FieldInfo field = this.GetType().GetField(propName);
                        if (field != null)
                        {
                            field.SetValue(this, deformControlPoints[i]);
                        }
                    }
                    
                    EditorUtility.SetDirty(this);
                }
            }
        }
    }

    // 只有当showConnectionLines为true时才绘制连接线
    if (showConnectionLines)
    {
        Handles.color = lineColor;

        int i, j, k;

        // S direction connections
        for (i = 0; i < resolution.x - 1; i++)
        {
            for (j = 0; j < resolution.y; j++)
            {
                for (k = 0; k < resolution.z; k++)
                {
                    int idx1 = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    int idx2 = k + (j * resolution.z) + ((i + 1) * resolution.y * resolution.z);

                    if (idx1 < deformControlPoints.Count && idx2 < deformControlPoints.Count)
                    {
                        Vector3 worldPos1 = transform.TransformPoint(deformControlPoints[idx1]);
                        Vector3 worldPos2 = transform.TransformPoint(deformControlPoints[idx2]);
                        Handles.DrawLine(worldPos1, worldPos2);
                    }
                }
            }
        }

        // T direction connections
        for (i = 0; i < resolution.x; i++)
        {
            for (j = 0; j < resolution.y - 1; j++)
            {
                for (k = 0; k < resolution.z; k++)
                {
                    int idx1 = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    int idx2 = k + ((j + 1) * resolution.z) + (i * resolution.y * resolution.z);

                    if (idx1 < deformControlPoints.Count && idx2 < deformControlPoints.Count)
                    {
                        Vector3 worldPos1 = transform.TransformPoint(deformControlPoints[idx1]);
                        Vector3 worldPos2 = transform.TransformPoint(deformControlPoints[idx2]);
                        Handles.DrawLine(worldPos1, worldPos2);
                    }
                }
            }
        }

        // U direction connections
        for (i = 0; i < resolution.x; i++)
        {
            for (j = 0; j < resolution.y; j++)
            {
                for (k = 0; k < resolution.z - 1; k++)
                {
                    int idx1 = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    int idx2 = (k + 1) + (j * resolution.z) + (i * resolution.y * resolution.z);

                    if (idx1 < deformControlPoints.Count && idx2 < deformControlPoints.Count)
                    {
                        Vector3 worldPos1 = transform.TransformPoint(deformControlPoints[idx1]);
                        Vector3 worldPos2 = transform.TransformPoint(deformControlPoints[idx2]);
                        Handles.DrawLine(worldPos1, worldPos2);
                    }
                }
            }
        }
    }

    // 强制重绘场景视图
    sceneView.Repaint();
}
#endif
#endif
#if UNITY_EDITOR
private Vector2 dragStartPosition;
private Vector2 dragCurrentPosition;
private Dictionary<int, Vector3> initialPointPositions = new Dictionary<int, Vector3>();
// 处理选择和拖动的逻辑
private void HandleBoxSelection(Event e, SceneView sceneView)
{
    // 处理已选中点的移动
    if (selectedPoints.Count > 0)
    {
        // 开始拖动选中的点
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            // 检查是否点击了选中的点中的任意一个
            bool clickedOnSelectedPoint = false;
            int clickedPointIndex = -1;
            foreach (int index in selectedPoints)
            {
                Vector3 worldPos = transform.TransformPoint(deformControlPoints[index]);
                Vector3 screenPos = sceneView.camera.WorldToScreenPoint(worldPos);
                // 转换为GUI坐标系统
                screenPos.y = sceneView.camera.pixelHeight - screenPos.y;

                float distance = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), e.mousePosition);
                float handleSize = HandleUtility.GetHandleSize(worldPos) * 20f; // 增大点击区域

                if (distance < handleSize)
                {
                    clickedOnSelectedPoint = true;
                    clickedPointIndex = index;
                    break;
                }
            }

            // 只有在没有按下Shift键的情况下，点击已选择的点才会开始拖动
            if (clickedOnSelectedPoint && !e.shift)
            {
                isDraggingSelectedPoints = true;
                selectedPointsOffsets.Clear();

                // 保存鼠标初始位置，用于计算拖动偏移
                dragStartPosition = e.mousePosition;
                dragCurrentPosition = dragStartPosition;

                // 保存所有选中点的初始位置
                initialPointPositions.Clear();
                foreach (int index in selectedPoints)
                {
                    initialPointPositions[index] = deformControlPoints[index];
                }

                e.Use();
            }
        }

        // 拖动选中的点
        if (isDraggingSelectedPoints && e.type == EventType.MouseDrag)
        {
            Undo.RecordObject(this, "Move Selected Control Points");

            // 更新当前鼠标位置
            dragCurrentPosition = e.mousePosition;

            // 计算屏幕空间的移动增量
            Vector2 mouseDelta = dragCurrentPosition - dragStartPosition;

            // 将屏幕空间增量转换为世界空间
            // 我们需要使用camera的right和up向量来确保移动方向正确
            Camera camera = sceneView.camera;

            // 选择第一个点作为参考点
            int firstIndex = selectedPoints.First();
            Vector3 referencePoint = transform.TransformPoint(deformControlPoints[firstIndex]);

            // 计算移动方向
            Vector3 moveDirection = Vector3.zero;

            // 使用射线计算移动方向
            Ray startRay = camera.ScreenPointToRay(new Vector3(dragStartPosition.x,
                                                               camera.pixelHeight - dragStartPosition.y, 0));
            Ray currentRay = camera.ScreenPointToRay(new Vector3(dragCurrentPosition.x,
                                                               camera.pixelHeight - dragCurrentPosition.y, 0));

            // 创建一个与相机方向垂直的平面
            Plane movementPlane = new Plane(-camera.transform.forward, referencePoint);

            float startDist, currentDist;
            Vector3 startWorldPos = Vector3.zero;
            Vector3 currentWorldPos = Vector3.zero;

            // 计算鼠标位置对应的世界坐标
            if (movementPlane.Raycast(startRay, out startDist) && movementPlane.Raycast(currentRay, out currentDist))
            {
                startWorldPos = startRay.GetPoint(startDist);
                currentWorldPos = currentRay.GetPoint(currentDist);
                moveDirection = currentWorldPos - startWorldPos;
            }

            // 应用移动到所有选中的点
            foreach (int index in selectedPoints)
            {
                if (initialPointPositions.ContainsKey(index))
                {
                    Vector3 initialPos = initialPointPositions[index];
                    Vector3 initialWorldPos = transform.TransformPoint(initialPos);
                    Vector3 newWorldPos = initialWorldPos + moveDirection;
                    deformControlPoints[index] = transform.InverseTransformPoint(newWorldPos);
                    
                    // 如果有动态属性，同步到属性
                    if (dynamicPropertiesCreated)
                    {
                        string propName = $"ControlPoint_{index}";
                        FieldInfo field = this.GetType().GetField(propName);
                        if (field != null)
                        {
                            field.SetValue(this, deformControlPoints[index]);
                        }
                    }
                }
            }
            
            // 如果处于动画记录模式，实时更新关键帧
            if (isRecordingAnimation && updateKeyframesWhenDragging)
            {
                UpdateAnimationKeyframes();
            }

            EditorUtility.SetDirty(this);

            e.Use();
        }

        // 结束拖动
        if (isDraggingSelectedPoints && e.type == EventType.MouseUp && e.button == 0)
        {
            isDraggingSelectedPoints = false;
            selectedPointsOffsets.Clear();
            initialPointPositions.Clear();

            // 标记需要变形
            needsDeform = true;
            
            // 确保在拖动结束时更新关键帧
            if (isRecordingAnimation)
            {
                UpdateAnimationKeyframes();
            }

            e.Use();
        }
    }

    // 处理单击选择
    if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !isDraggingSelectedPoints)
    {
        // 查找点击的点
        float closestDistance = float.MaxValue;
        int closestPointIndex = -1;

        for (int i = 0; i < deformControlPoints.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(deformControlPoints[i]);
            Vector3 screenPos = sceneView.camera.WorldToScreenPoint(worldPos);
            // 转换为GUI坐标系统
            screenPos.y = sceneView.camera.pixelHeight - screenPos.y;

            float distance = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), e.mousePosition);
            float handleSize = HandleUtility.GetHandleSize(worldPos) * 15f; // 点击区域

            if (distance < handleSize && distance < closestDistance)
            {
                closestDistance = distance;
                closestPointIndex = i;
            }
        }

        // 处理点击选择
        if (closestPointIndex != -1)
        {
            if (e.control)
            {
                // Ctrl+点击切换选择状态
                if (selectedPoints.Contains(closestPointIndex))
                {
                    selectedPoints.Remove(closestPointIndex);
                }
                else
                {
                    selectedPoints.Add(closestPointIndex);
                }
            }
            else if (e.shift)
            {
                // Shift+点击添加到当前选择，不清除已有选择
                // 关键修改：无论是否已选择，都尝试添加（如果是已选择的，不会有变化）
                if (!selectedPoints.Contains(closestPointIndex))
                {
                    selectedPoints.Add(closestPointIndex);
                }
                // 永远不要在Shift按下时开始拖动
            }
            else
            {
                // 普通点击，清除其他选择并选中当前点
                if (!selectedPoints.Contains(closestPointIndex))
                {
                    selectedPoints.Clear();
                    selectedPoints.Add(closestPointIndex);
                }
                // 如果点击的是已选中的点，不做任何操作，保持拖动的可能性
            }

            e.Use();
        }
        else if (!e.control && !e.shift)
        {
            // 点击空白区域且没有按Ctrl键或Shift键，清除所有选择
            selectedPoints.Clear();
        }
    }
}
#endif
#endif
    void OnRenderObject()
    {
        // 仅在游戏视图中渲染（编辑模式下使用Handles系统）
        if (!Application.isPlaying)
        {
            return;
        }

        // Skip drawing if both elements are hidden
        if (!showControlPoints && !showConnectionLines) return;

        if (mainCamera == null)
        {
            FindMainCamera();
            if (mainCamera == null) return;
        }

        // Draw connections between control points
        if (showConnectionLines && lineMaterial != null)
        {
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            DrawConnectors();
            GL.PopMatrix();
        }

        // Draw control points with fixed screen size
        if (showControlPoints && lineMaterial != null)
        {
            lineMaterial.SetPass(0);
            DrawFixedSizeControlPoints();
        }

        // 在游戏模式下绘制框选矩形
        if (isBoxSelecting && Application.isPlaying)
        {
            DrawBoxSelectionRect();
        }
    }

    void DrawFixedSizeControlPoints()
    {
        // 使用 GL 在屏幕空间中绘制固定大小的点
        GL.PushMatrix();
        GL.LoadOrtho(); // 加载正交投影矩阵，使我们可以在屏幕空间中绘制

        GL.Begin(GL.QUADS);

        for (int i = 0; i < deformControlPoints.Count; i++)
        {
            // 将控制点转换到世界空间
            Vector3 worldPoint = transform.TransformPoint(deformControlPoints[i]);

            // 将世界空间点转换到屏幕空间
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

            // 只绘制在相机前方的点
            if (screenPoint.z <= 0) continue;

            // 根据选择状态设置颜色
            if (selectedPoints.Contains(i))
            {
                GL.Color(selectedPointColor);
            }
            else if (i == selectedPointIndex)
            {
                GL.Color(Color.yellow);
            }
            else if (currentFacePoints.Contains(i))
            {
                GL.Color(highlightedFaceColor);
            }
            else
            {
                GL.Color(pointColor);
            }

            // 计算屏幕大小 (选中的点稍大一些)
            float size = (selectedPoints.Contains(i) || i == selectedPointIndex) ?
                         controlPointPixelSize * 1.5f : controlPointPixelSize;

            // 将屏幕坐标归一化到 [0,1] 范围
            float x = screenPoint.x / Screen.width;
            float y = screenPoint.y / Screen.height;

            // 绘制一个固定像素大小的方形
            float halfSize = size / 2f;
            float xMin = x - (halfSize / Screen.width);
            float xMax = x + (halfSize / Screen.width);
            float yMin = y - (halfSize / Screen.height);
            float yMax = y + (halfSize / Screen.height);

            GL.Vertex3(xMin, yMin, 0);
            GL.Vertex3(xMax, yMin, 0);
            GL.Vertex3(xMax, yMax, 0);
            GL.Vertex3(xMin, yMax, 0);
        }

        GL.End();
        GL.PopMatrix();
    }

    // 绘制框选矩形
    void DrawBoxSelectionRect()
    {
        if (!isBoxSelecting) return;

        GL.PushMatrix();
        GL.LoadOrtho();

        // 绘制半透明填充
        GL.Begin(GL.QUADS);
        GL.Color(new Color(0, 0.8f, 1, 0.3f));

        float x1 = Mathf.Min(boxSelectStart.x, boxSelectCurrent.x) / Screen.width;
        float x2 = Mathf.Max(boxSelectStart.x, boxSelectCurrent.x) / Screen.width;
        float y1 = Mathf.Min(boxSelectStart.y, boxSelectCurrent.y) / Screen.height;
        float y2 = Mathf.Max(boxSelectStart.y, boxSelectCurrent.y) / Screen.height;

        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x2, y1, 0);
        GL.Vertex3(x2, y2, 0);
        GL.Vertex3(x1, y2, 0);
        GL.End();

        // 绘制边框
        GL.Begin(GL.LINES);
        GL.Color(new Color(0, 0.8f, 1, 0.8f));

        // 底边
        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x2, y1, 0);

        // 右边
        GL.Vertex3(x2, y1, 0);
        GL.Vertex3(x2, y2, 0);

        // 顶边
        GL.Vertex3(x2, y2, 0);
        GL.Vertex3(x1, y2, 0);

        // 左边
        GL.Vertex3(x1, y2, 0);
        GL.Vertex3(x1, y1, 0);

        GL.End();
        GL.PopMatrix();
    }

    void DrawConnectors()
    {
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        int i, j, k;

        // S direction connections
        for (i = 0; i < resolution.x - 1; i++)
        {
            for (j = 0; j < resolution.y; j++)
            {
                for (k = 0; k < resolution.z; k++)
                {
                    int idx1 = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    int idx2 = k + (j * resolution.z) + ((i + 1) * resolution.y * resolution.z);

                    if (idx1 < deformControlPoints.Count && idx2 < deformControlPoints.Count)
                    {
                        GL.Vertex(deformControlPoints[idx1]);
                        GL.Vertex(deformControlPoints[idx2]);
                    }
                }
            }
        }

        // T direction connections
        for (i = 0; i < resolution.x; i++)
        {
            for (j = 0; j < resolution.y - 1; j++)
            {
                for (k = 0; k < resolution.z; k++)
                {
                    int idx1 = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    int idx2 = k + ((j + 1) * resolution.z) + (i * resolution.y * resolution.z);

                    if (idx1 < deformControlPoints.Count && idx2 < deformControlPoints.Count)
                    {
                        GL.Vertex(deformControlPoints[idx1]);
                        GL.Vertex(deformControlPoints[idx2]);
                    }
                }
            }
        }

        // U direction connections
        for (i = 0; i < resolution.x; i++)
        {
            for (j = 0; j < resolution.y; j++)
            {
                for (k = 0; k < resolution.z - 1; k++)
                {
                    int idx1 = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    int idx2 = (k + 1) + (j * resolution.z) + (i * resolution.y * resolution.z);

                    if (idx1 < deformControlPoints.Count && idx2 < deformControlPoints.Count)
                    {
                        GL.Vertex(deformControlPoints[idx1]);
                        GL.Vertex(deformControlPoints[idx2]);
                    }
                }
            }
        }

        GL.End();
    }
void HandleInput()
{
    // Skip interaction if control points are hidden
    if (!showControlPoints)
    {
        selectedPointIndex = -1;
        return;
    }

    // 处理单选和拖动
    if (Input.GetMouseButtonDown(0))
    {
        // 获取鼠标位置
        Vector2 mousePos = Input.mousePosition;

        float closestDistance = float.MaxValue;
        int closestPointIndex = -1;

        // 检查鼠标是否点击了任何控制点
        for (int i = 0; i < deformControlPoints.Count; i++)
        {
            Vector3 worldPoint = transform.TransformPoint(deformControlPoints[i]);
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

            // 忽略相机后方的点
            if (screenPoint.z <= 0) continue;

            // 计算屏幕空间中的距离
            float distance = Vector2.Distance(
                new Vector2(mousePos.x, mousePos.y),
                new Vector2(screenPoint.x, screenPoint.y)
            );

            // 控制点的选择区域稍大于显示大小
            float selectionSize = controlPointPixelSize * 1.2f;

            if (distance < selectionSize && distance < closestDistance)
            {
                closestDistance = distance;
                closestPointIndex = i;
            }
        }

        if (closestPointIndex != -1)
        {
            // 检查修饰键
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            
            // 如果点击了已选中的点集中的一个点，开始拖动整个选择集（但不是在按住Shift时）
            if (selectedPoints.Contains(closestPointIndex) && !isShiftPressed)
            {
                isDraggingSelectedPoints = true;
                selectedPointsOffsets.Clear();

                // 创建一个与相机方向垂直的平面，通过第一个选中点
                int firstPointIndex = selectedPoints.First();
                Vector3 planePoint = transform.TransformPoint(deformControlPoints[firstPointIndex]);
                Vector3 planeNormal = mainCamera.transform.forward;

                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                Plane plane = new Plane(planeNormal, planePoint);
                float distance;

                if (plane.Raycast(ray, out distance))
                {
                    Vector3 mouseWorldPos = ray.GetPoint(distance);

                    // 保存所有选中点的偏移量
                    foreach (int index in selectedPoints)
                    {
                        Vector3 worldPoint = transform.TransformPoint(deformControlPoints[index]);
                        selectedPointsOffsets[index] = worldPoint - mouseWorldPos;
                    }
                }
            }
            else
            {
                // 如果没有按Ctrl键或Shift键，清除之前的选择
                if (!isCtrlPressed && !isShiftPressed)
                {
                    selectedPoints.Clear();
                }

                // 添加或切换当前点的选择状态
                if (isCtrlPressed)
                {
                    if (selectedPoints.Contains(closestPointIndex))
                    {
                        selectedPoints.Remove(closestPointIndex);
                    }
                    else
                    {
                        selectedPoints.Add(closestPointIndex);
                    }
                }
                else if (isShiftPressed)
                {
                    // Shift+点击添加到选择，不清除已有选择，也不开始拖动
                    if (!selectedPoints.Contains(closestPointIndex))
                    {
                        selectedPoints.Add(closestPointIndex);
                    }
                }
                else
                {
                    selectedPoints.Add(closestPointIndex);
                    selectedPointIndex = closestPointIndex;

                    // 设置拖动
                    isDraggingSelectedPoints = true;
                    selectedPointsOffsets.Clear();

                    // 创建一个与相机方向垂直的平面，通过选中点
                    Vector3 planePoint = transform.TransformPoint(deformControlPoints[closestPointIndex]);
                    Vector3 planeNormal = mainCamera.transform.forward;

                    Ray ray = mainCamera.ScreenPointToRay(mousePos);
                    Plane plane = new Plane(planeNormal, planePoint);
                    float distance;

                    if (plane.Raycast(ray, out distance))
                    {
                        Vector3 mouseWorldPos = ray.GetPoint(distance);

                        // 设置拖动偏移
                        Vector3 worldPoint = transform.TransformPoint(deformControlPoints[closestPointIndex]);
                        selectedPointsOffsets[closestPointIndex] = worldPoint - mouseWorldPos;
                    }
                }
            }
        }
        else if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && 
                 !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            // 点击空白处，清除所有选择（仅当没有按修饰键时）
            selectedPoints.Clear();
            selectedPointIndex = -1;
        }
    }
    else if (Input.GetMouseButtonUp(0) && (isDraggingSelectedPoints || selectedPointIndex != -1))
    {
        isDraggingSelectedPoints = false;
        selectedPointsOffsets.Clear();
        needsDeform = true;
        
        // 如果有动态属性，同步到属性
        if (dynamicPropertiesCreated)
        {
            SyncPropertiesFromControlPoints();
        }
        
        #if UNITY_EDITOR
        // 确保在拖动结束时更新关键帧
        if (isRecordingAnimation)
        {
            UpdateAnimationKeyframes();
        }
        #endif
    }
    else if (Input.GetMouseButton(0) && isDraggingSelectedPoints)
    {
        // 拖动所有选中的点
        Vector3 mousePos = Input.mousePosition;

        // 创建一个与相机方向垂直的平面，通过第一个选中点
        if (selectedPoints.Count > 0)
        {
            int firstPointIndex = selectedPoints.First();
            Vector3 planePoint = transform.TransformPoint(deformControlPoints[firstPointIndex]);
            Vector3 planeNormal = mainCamera.transform.forward;

            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane plane = new Plane(planeNormal, planePoint);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                Vector3 mouseWorldPos = ray.GetPoint(distance);

                foreach (int index in selectedPoints)
                {
                    if (selectedPointsOffsets.ContainsKey(index))
                    {
                        Vector3 newWorldPos = mouseWorldPos + selectedPointsOffsets[index];
                        deformControlPoints[index] = transform.InverseTransformPoint(newWorldPos);
                        
                        // 如果有动态属性，同步到属性
                        if (dynamicPropertiesCreated)
                        {
                            string propName = $"ControlPoint_{index}";
                            FieldInfo field = this.GetType().GetField(propName);
                            if (field != null)
                            {
                                field.SetValue(this, deformControlPoints[index]);
                            }
                        }
                    }
                }

                // 标记需要变形
                if (deformEveryFrame)
                {
                    needsDeform = true;
                }
                
                #if UNITY_EDITOR
                // 如果处于动画记录模式，实时更新关键帧
                if (isRecordingAnimation && updateKeyframesWhenDragging)
                {
                    UpdateAnimationKeyframes();
                }
                #endif
            }
        }

#if UNITY_EDITOR
        // 确保Scene视图实时更新
        if (Application.isPlaying)
        {
            SyncViewsControlPoints();
        }
#endif
    }
}
    // 新增选择控制方法
    public void SelectAllControlPoints()
    {
        selectedPoints.Clear();
        for (int i = 0; i < deformControlPoints.Count; i++)
        {
            selectedPoints.Add(i);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }
        else
        {
            SyncViewsControlPoints();
        }
#endif
    }

    public void ClearSelection()
    {
        selectedPoints.Clear();
        selectedPointIndex = -1;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }
        else
        {
            SyncViewsControlPoints();
        }
#endif
    }

    public void CancelSelection()
    {
        isBoxSelecting = false;
        isDraggingSelectedPoints = false;
        selectedPointsOffsets.Clear();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }
        else
        {
            SyncViewsControlPoints();
        }
#endif
    }

    // Update mesh coordinates when the skinned mesh changes
    void UpdateMeshCoordinates(Vector3[] vertices)
    {
        if (vertexCoordinates.Count != vertices.Length)
        {
            vertexCoordinates.Clear();
            for (int i = 0; i < vertices.Length; i++)
            {
                float s = ((vertices[i].x - minPoint.x) / (maxPoint.x - minPoint.x));
                float t = ((vertices[i].y - minPoint.y) / (maxPoint.y - minPoint.y));
                float u = ((vertices[i].z - minPoint.z) / (maxPoint.z - minPoint.z));
                // Clamp values to prevent errors due to vertices outside the bounding box
                s = Mathf.Clamp01(s);
                t = Mathf.Clamp01(t);
                u = Mathf.Clamp01(u);
                vertexCoordinates.Add(new Vector3(s, t, u));
            }
        }
    }

    // Update the entire lattice structure
    public void UpdateLattice()
    {
        deformControlPoints.Clear();
        originalPoints.Clear();

        // 确保我们有网格数据
        if ((modelMesh == null && bakedMesh == null) ||
            (vertexCoordinates.Count == 0 && modelMesh != null && modelMesh.vertexCount > 0))
        {
            InitMesh();
        }

        InitCPs();

        // 保存原始控制点位置用于重置
        originalPoints.Clear();
        originalPoints.AddRange(deformControlPoints);

        // 清除所有选择
        selectedPoints.Clear();
        selectedPointIndex = -1;
        
        // 如果已经创建了动态属性，则更新它们
        if (dynamicPropertiesCreated)
        {
            SyncPropertiesFromControlPoints();
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    // Prepare the mesh of the model
    public void InitMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            originalMeshRendererState = meshRenderer.enabled;
        }

        if (skinnedMeshRenderer != null)
        {
            // We're dealing with a skinned mesh
            isSkinnedMesh = true;

            // Create a new mesh to hold the baked vertices
            bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            // Add a mesh filter if it doesn't exist already to show our deformed mesh
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            // Use the baked mesh for our calculations
            modelMesh = bakedMesh;

            // 保存一个原始网格副本
            if (originalMesh == null)
            {
                originalMesh = Instantiate(bakedMesh);
            }

            // Add a mesh renderer if necessary
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
               meshRenderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials;
            }

            // 保存原始渲染状态，但不立即修改
            originalMeshRendererState = meshRenderer.enabled;
        }
        else if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            // Normal mesh
            isSkinnedMesh = false;
            modelMesh = meshFilter.sharedMesh;

            // 保存一个原始网格副本
            if (originalMesh == null)
            {
                originalMesh = Instantiate(modelMesh);
            }
        }
        else
        {
            Debug.LogError(" requires either a MeshFilter or SkinnedMeshRenderer component!");
            return;
        }

        vertexCoordinates.Clear();

        // 确保mesh存在
        if (modelMesh == null)
        {
            Debug.LogError("No valid mesh found!");
            return;
        }

        // get width, depth, height for scaling
        width = modelMesh.bounds.size.x;
        height = modelMesh.bounds.size.y;
        depth = modelMesh.bounds.size.z;

        // get min and max points of the model (estimation)
        minPoint = -new Vector3(width / 2, height / 2, depth / 2) + modelMesh.bounds.center;
        maxPoint = new Vector3(width / 2, height / 2, depth / 2) + modelMesh.bounds.center;

        // for every vertex save its s,t,u as a ratio across the lattice space
        Vector3[] vertices = modelMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float s = ((vertices[i].x - minPoint.x) / (maxPoint.x - minPoint.x));
            float t = ((vertices[i].y - minPoint.y) / (maxPoint.y - minPoint.y));
            float u = ((vertices[i].z - minPoint.z) / (maxPoint.z - minPoint.z));

            // Clamp values to prevent errors due to vertices outside the bounding box
            s = Mathf.Clamp01(s);
            t = Mathf.Clamp01(t);
            u = Mathf.Clamp01(u);

            vertexCoordinates.Add(new Vector3(s, t, u));
        }
    }

    // Place control points around the object
    public void InitCPs()
    {
        float x, y, z;

        // place n control points across the object at appropriate intervals
        for (int s = 0; s < resolution.x; s++)
        {
            x = s / (float)(resolution.x - 1);
            for (int t = 0; t < resolution.y; t++)
            {
                y = t / (float)(resolution.y - 1);
                for (int u = 0; u < resolution.z; u++)
                {
                    z = u / (float)(resolution.z - 1);
                    // Store control point position directly instead of creating GameObjects
                    deformControlPoints.Add(minPoint + new Vector3(x * width, y * height, z * depth));
                }
            }
        }
    }

    // for the given index compute the bernstein coefficients
    void MakeBernsteinCoefficients(int index)
    {
        if (index >= vertexCoordinates.Count)
        {
            return;
        }

        float s = vertexCoordinates[index].x;
        float t = vertexCoordinates[index].y;
        float u = vertexCoordinates[index].z;

        // Calculate Bernstein coefficients based on resolution
        if (resolution.x == 2)
        {
            bernsteinCoeffsX[0] = 1.0f - s;
            bernsteinCoeffsX[1] = s;
        }
        else if (resolution.x == 3)
        {
            bernsteinCoeffsX[0] = (1.0f - s) * (1.0f - s);
            bernsteinCoeffsX[1] = 2.0f * s * (1.0f - s);
            bernsteinCoeffsX[2] = s * s;
        }
        else
        {
            // For higher resolution, use generalized Bernstein basis
            for (int i = 0; i < resolution.x; i++)
            {
                float coef = BinomialCoefficient(resolution.x - 1, i);
                bernsteinCoeffsX[i] = coef * Mathf.Pow(s, i) * Mathf.Pow(1.0f - s, resolution.x - 1 - i);
            }
        }

        if (resolution.y == 2)
        {
            bernsteinCoeffsY[0] = 1.0f - t;
            bernsteinCoeffsY[1] = t;
        }
        else if (resolution.y == 3)
        {
            bernsteinCoeffsY[0] = (1.0f - t) * (1.0f - t);
            bernsteinCoeffsY[1] = 2.0f * t * (1.0f - t);
            bernsteinCoeffsY[2] = t * t;
        }
        else if (resolution.y == 4)
        {
            bernsteinCoeffsY[0] = (1.0f - t) * (1.0f - t) * (1.0f - t);
            bernsteinCoeffsY[1] = 3.0f * t * (1.0f - t) * (1.0f - t);
            bernsteinCoeffsY[2] = 3.0f * t * t * (1.0f - t);
            bernsteinCoeffsY[3] = t * t * t;
        }
        else
        {
            // For higher resolution, use generalized Bernstein basis
            for (int i = 0; i < resolution.y; i++)
            {
                float coef = BinomialCoefficient(resolution.y - 1, i);
                bernsteinCoeffsY[i] = coef * Mathf.Pow(t, i) * Mathf.Pow(1.0f - t, resolution.y - 1 - i);
            }
        }

        if (resolution.z == 2)
        {
            bernsteinCoeffsZ[0] = 1.0f - u;
            bernsteinCoeffsZ[1] = u;
        }
        else if (resolution.z == 3)
        {
            bernsteinCoeffsZ[0] = (1.0f - u) * (1.0f - u);
            bernsteinCoeffsZ[1] = 2.0f * u * (1.0f - u);
            bernsteinCoeffsZ[2] = u * u;
        }
        else if (resolution.z == 4)
        {
            bernsteinCoeffsZ[0] = (1.0f - u) * (1.0f - u) * (1.0f - u);
            bernsteinCoeffsZ[1] = 3.0f * u * (1.0f - u) * (1.0f - u);
            bernsteinCoeffsZ[2] = 3.0f * u * u * (1.0f - u);
            bernsteinCoeffsZ[3] = u * u * u;
        }
        else
        {
            // For higher resolution, use generalized Bernstein basis
            for (int i = 0; i < resolution.z; i++)
            {
                float coef = BinomialCoefficient(resolution.z - 1, i);
                bernsteinCoeffsZ[i] = coef * Mathf.Pow(u, i) * Mathf.Pow(1.0f - u, resolution.z - 1 - i);
            }
        }
    }

    // Calculate binomial coefficient (n choose k)
    float BinomialCoefficient(int n, int k)
    {
        float result = 1;
        for (int i = 1; i <= k; i++)
        {
            result *= (n - (k - i));
            result /= i;
        }
        return result;
    }

    // sum up the control points position * bernstein coefficients. return the new vertex position.
    public Vector3 EvalVertex(int index)
    {
        if (index >= vertexCoordinates.Count || deformControlPoints.Count == 0)
        {
            return Vector3.zero;
        }

        MakeBernsteinCoefficients(index);

        Vector3 point = Vector3.zero;

        for (int i = 0; i < resolution.x; i++)
        {
            for (int j = 0; j < resolution.y; j++)
            {
                for (int k = 0; k < resolution.z; k++)
                {
                    int cpIndex = k + (j * resolution.z) + (i * resolution.y * resolution.z);
                    if (cpIndex < deformControlPoints.Count)
                    {
                        point += deformControlPoints[cpIndex] * (bernsteinCoeffsX[i] * bernsteinCoeffsY[j] * bernsteinCoeffsZ[k]);
                    }
                }
            }
        }

        return point;
    }

    // Public method to reset the original mesh when needed
    // 修改ResetMesh方法
    public void ResetMesh()
    {
        // 如果已经永久保存了变形，需要完全重置
        if (permanentlySaved)
        {
            // 重新初始化网格和控制点
            Initialize();
            // 在初始化后，我们需要重新创建格子
            UpdateLattice();
            permanentlySaved = false; // 重置保存状态
        }
        else
        {
            // 正常重置行为：恢复到原始控制点位置
            if (originalPoints.Count > 0 && deformControlPoints.Count == originalPoints.Count)
            {
                for (int i = 0; i < deformControlPoints.Count; i++)
                {
                    deformControlPoints[i] = originalPoints[i];
                }
                
                // 如果有动态属性，同步到属性
                if (dynamicPropertiesCreated)
                {
                    SyncPropertiesFromControlPoints();
                }
            }
            else
            {
                // 如果没有有效的原始控制点，重新生成一个新的格子
                UpdateLattice();
            }
        }

        // 清除所有选择
        selectedPoints.Clear();
        selectedPointIndex = -1;
        isBoxSelecting = false;
        isDraggingSelectedPoints = false;

        // 恢复原始渲染状态，但仅当编辑模式关闭时
        if (!isEditModeActive)
        {
            if (isSkinnedMesh && skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.enabled = true;
            }

            if (meshRenderer != null)
            {
                meshRenderer.enabled = originalMeshRendererState;
            }
        }

        // 强制标记需要变形，无论deformEveryFrame是什么值
        needsDeform = true;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            // 如果在编辑模式且deformEveryFrame为false，立即应用变形
            if (!deformEveryFrame)
            {
                EditorApplication.delayCall += () =>
                {
                    // 创建一个辅助类实例来访问编辑器方法
                    var editorInstance = UnityEditor.Editor.CreateEditor(this) as LatticeEditor;
                    if (editorInstance != null)
                    {
                        // 使用编辑器类中的方法应用变形
                        editorInstance.SimulateDeformInEditor(this);
                        UnityEditor.Editor.DestroyImmediate(editorInstance);
                    }
                    SceneView.RepaintAll();
                };
            }
            SceneView.RepaintAll();
        }
        else
        {
            // 在运行时更新Scene视图
            SyncViewsControlPoints();
        }
#endif
    }

    // 永久保存当前变形状态
    public void SaveCurrentDeformation()
    {
        if (meshFilter != null)
        {
            // 根据当前模式选择正确的网格引用
            Mesh currentMesh;
            if (Application.isPlaying)
            {
                // 运行时使用mesh
                currentMesh = meshFilter.mesh;
            }
            else
            {
                // 编辑模式使用sharedMesh
                currentMesh = meshFilter.sharedMesh;
            }

            if (currentMesh != null)
            {
                // 创建当前网格的副本
                originalMesh = Instantiate(currentMesh);

                // 更新控制点的原始位置
                originalPoints.Clear();
                originalPoints.AddRange(deformControlPoints);

                permanentlySaved = true;

                Debug.Log("Current deformation saved permanently!");
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                }
#endif
            }
        }
    }

    public void ExportDeformedModelAsNewPrefab()
    {
#if UNITY_EDITOR
        if (meshFilter != null)
        {
            Mesh currentMesh;
            if (Application.isPlaying)
                currentMesh = meshFilter.mesh;
            else
                currentMesh = meshFilter.sharedMesh;

            if (currentMesh != null)
            {
                // Create a copy of the current mesh
                Mesh deformedMeshCopy = Object.Instantiate(currentMesh);
                deformedMeshCopy.name = currentMesh.name + "_Deformed";

                // Ensure the directory exists
                string folderPath = "Assets/DeformedMeshes";
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "DeformedMeshes");
                }

                // Save mesh asset
                string meshPath = Path.Combine(folderPath, deformedMeshCopy.name + ".asset");
                AssetDatabase.CreateAsset(deformedMeshCopy, meshPath);
                AssetDatabase.SaveAssets();

                // Create a new GameObject (or reuse this one)
                GameObject newObj = new GameObject(meshFilter.gameObject.name + "_Deformed");
                MeshFilter newMeshFilter = newObj.AddComponent<MeshFilter>();
                MeshRenderer newMeshRenderer = newObj.AddComponent<MeshRenderer>();

                newMeshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                newMeshRenderer.sharedMaterials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;

                // Save prefab
                string prefabPath = Path.Combine(folderPath, newObj.name + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(newObj, prefabPath);

                // Cleanup
                Object.DestroyImmediate(newObj);

                Debug.Log("New prefab created with deformed model");

                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }

    // Public method to toggle between FFD mode and original skinned mesh animation
    public void ToggleFFDMode(bool enableFFD)
    {
        if (!isSkinnedMesh || skinnedMeshRenderer == null) return;

        skinnedMeshRenderer.enabled = !enableFFD;

        if (meshRenderer != null)
        {
            meshRenderer.enabled = enableFFD;
        }

        if (enableFFD)
        {
            UpdateLattice();
        }
        else
        {
            deformControlPoints.Clear();
            originalPoints.Clear();
            selectedPoints.Clear();
            selectedPointIndex = -1;
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    // Turn on edit mode in the editor
    public void StartEditMode()
    {
        isEditModeActive = true;

        // 确保我们已初始化
        if (!initialized)
        {
            Initialize();
        }

        if (deformControlPoints.Count == 0)
        {
            UpdateLattice();
        }

        // 确保网格可见
        if (isSkinnedMesh && skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.enabled = false;
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }
        else
        {
            // 在运行时更新Scene视图
            SyncViewsControlPoints();
        }
#endif
    }

    // Turn off edit mode in the editor
    public void StopEditMode()
    {
        isEditModeActive = false;

        // 清除所有选择
        selectedPoints.Clear();
        selectedPointIndex = -1;
        isBoxSelecting = false;
        isDraggingSelectedPoints = false;

        // 恢复原始渲染器状态
        if (isSkinnedMesh && skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.enabled = true;
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = originalMeshRendererState;
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
        }
#endif
    }

    // 为了保持与原始API兼容的方法
    public void ClearLattice()
    {
        deformControlPoints.Clear();
        originalPoints.Clear();
        selectedPoints.Clear();
        selectedPointIndex = -1;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    public void InitConnectors()
    {
        // 现在由 DrawConnectors() 处理
        // 保留此方法以保持API兼容性
    }
    
    // 新增方法：创建动态属性
    public void CreateDynamicControlPointProperties()
    {
#if UNITY_EDITOR
        // 创建/打开脚本模板
        string scriptPath = GetScriptPath();
        if (string.IsNullOrEmpty(scriptPath))
        {
            Debug.LogError("Could not find script path");
            return;
        }
        
        // 读取脚本内容
        string scriptContent = File.ReadAllText(scriptPath);
        
        // 检查是否已经存在动态属性区域
        int startIndex = scriptContent.IndexOf("// BEGIN DYNAMIC PROPERTIES");
        int endIndex = scriptContent.IndexOf("// END DYNAMIC PROPERTIES");
        
        string dynamicPropertiesCode = GenerateDynamicPropertiesCode();
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            // 替换现有区域
            scriptContent = scriptContent.Substring(0, startIndex) + 
                            "// BEGIN DYNAMIC PROPERTIES\n" + 
                            dynamicPropertiesCode + 
                            "    // END DYNAMIC PROPERTIES" + 
                            scriptContent.Substring(endIndex + "// END DYNAMIC PROPERTIES".Length);
        }
        else
        {
            // 在类的开头添加新区域
            int classIndex = scriptContent.IndexOf("public class Lattice : MonoBehaviour");
            if (classIndex >= 0)
            {
                int openBraceIndex = scriptContent.IndexOf("{", classIndex);
                if (openBraceIndex >= 0)
                {
                    scriptContent = scriptContent.Substring(0, openBraceIndex + 1) + 
                                    "\n    // BEGIN DYNAMIC PROPERTIES\n" + 
                                    dynamicPropertiesCode + 
                                    "    // END DYNAMIC PROPERTIES\n" + 
                                    scriptContent.Substring(openBraceIndex + 1);
                }
            }
        }
        
        // 写回文件
        File.WriteAllText(scriptPath, scriptContent);
        
        // 记录创建的属性数量
        createdPropertyCount = deformControlPoints.Count;
        dynamicPropertiesCreated = true;
        
        // 重要：将当前值保存到临时文件以便在重新编译后恢复
        string tempDataPath = Path.Combine(Application.temporaryCachePath, "LatticeControlPointsTemp.json");
        SaveControlPointsToTempFile(tempDataPath);
        
        // 通知用户重新编译
        EditorUtility.DisplayDialog("Dynamic Properties Created", 
            "Dynamic properties have been added to the script with current control point values. Unity will now recompile the script.\n\n" +
            "After compilation, the values will be restored automatically.", "OK");
        
        // 触发脚本编译
        AssetDatabase.Refresh();
        
        // 在编译完成后恢复值的延迟调用
        EditorApplication.delayCall += () => {
            LoadControlPointsFromTempFile(tempDataPath);
        };
#endif
    }
    
    // 生成动态属性代码
    private string GenerateDynamicPropertiesCode()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        for (int i = 0; i < deformControlPoints.Count; i++)
        {
            // 使用当前控制点的实际位置来初始化变量
            Vector3 position = deformControlPoints[i];
            sb.AppendLine($"    [HideInInspector]");
            sb.AppendLine($"    public Vector3 ControlPoint_{i} = new Vector3({position.x}f, {position.y}f, {position.z}f);");
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    // 添加临时保存/加载控制点数据的函数
    private void SaveControlPointsToTempFile(string path)
    {
#if UNITY_EDITOR
        // 创建要保存的数据结构
        List<SerializableVector3> points = new List<SerializableVector3>();
        foreach (Vector3 point in deformControlPoints)
        {
            points.Add(new SerializableVector3(point));
        }
        
        // 将数据序列化为JSON
        string json = JsonUtility.ToJson(new ControlPointsData { points = points });
        
        // 写入文件
        File.WriteAllText(path, json);
#endif
    }

    private void LoadControlPointsFromTempFile(string path)
    {
#if UNITY_EDITOR
        if (!File.Exists(path)) return;
        
        try
        {
            // 读取并解析JSON
            string json = File.ReadAllText(path);
            ControlPointsData data = JsonUtility.FromJson<ControlPointsData>(json);
            
            // 如果点数匹配，恢复数据
            if (data.points.Count == deformControlPoints.Count)
            {
                for (int i = 0; i < data.points.Count; i++)
                {
                    deformControlPoints[i] = data.points[i].ToVector3();
                    
                    // 同时使用反射更新动态属性
                    string propName = $"ControlPoint_{i}";
                    FieldInfo field = this.GetType().GetField(propName);
                    if (field != null)
                    {
                        field.SetValue(this, deformControlPoints[i]);
                    }
                }
                
                // 标记需要变形
                needsDeform = true;
                EditorUtility.SetDirty(this);
            }
            
            // 清理临时文件
            File.Delete(path);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading control points data: {e.Message}");
        }
#endif
    }

    // 可序列化的Vector3结构
    [System.Serializable]
    private struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        
        public SerializableVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    // 用于JSON序列化的控制点数据容器类
    [System.Serializable]
    private class ControlPointsData
    {
        public List<SerializableVector3> points = new List<SerializableVector3>();
    }
    
    // 获取脚本路径
    private string GetScriptPath()
    {
#if UNITY_EDITOR
        MonoScript script = MonoScript.FromMonoBehaviour(this);
        return AssetDatabase.GetAssetPath(script);
#else
        return string.Empty;
#endif
    }
    
    // 同步方法：从动态属性到控制点
    public void SyncControlPointsFromProperties()
    {

        // 使用反射获取所有控制点属性
        FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (FieldInfo field in fields)
        {
            if (field.Name.StartsWith("ControlPoint_") && field.FieldType == typeof(Vector3))
            {
                // 解析索引
                string indexStr = field.Name.Substring("ControlPoint_".Length);
                if (int.TryParse(indexStr, out int index) && index < deformControlPoints.Count)
                {
                    // 获取属性值并更新控制点
                    Vector3 value = (Vector3)field.GetValue(this);
                    deformControlPoints[index] = value;
                }
            }
        }

        // 标记需要变形
        if (deformEveryFrame)
        {
            needsDeform = true;
        }
    }
    
    // 同步方法：从控制点到动态属性
    public void SyncPropertiesFromControlPoints()
    {
        // 使用反射获取所有控制点属性
        FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (FieldInfo field in fields)
        {
            if (field.Name.StartsWith("ControlPoint_") && field.FieldType == typeof(Vector3))
            {
                // 解析索引
                string indexStr = field.Name.Substring("ControlPoint_".Length);
                if (int.TryParse(indexStr, out int index) && index < deformControlPoints.Count)
                {
                    // 更新属性值
                    field.SetValue(this, deformControlPoints[index]);
                }
            }
        }
        
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
    
    // 初始化动态属性值
    public void InitializeDynamicProperties()
    {
        if (!dynamicPropertiesCreated)
            return;
            
        // 确保动态属性与控制点同步
        SyncPropertiesFromControlPoints();
    }
}

#if UNITY_EDITOR
// Custom editor for the Lattice class
[CustomEditor(typeof(Lattice))]
public class LatticeEditor : Editor
{
public override void OnInspectorGUI()
{
    Lattice lattice = (Lattice)target;

    EditorGUILayout.HelpBox(
        "Mesh Field Description:\n" +
        "- Model Mesh: The current working mesh (either static or baked skinned mesh).\n" +
        "- Baked Mesh: If using a Skinned Mesh Renderer, the baked pose mesh during play mode.\n" +
        "- Original Mesh: The initially saved copy of the mesh, used for resets.",
        MessageType.Info);

    // Draw default inspector properties
    DrawDefaultInspector();

    EditorGUILayout.Space();

    // Hide Edit Mode controls in Play mode
    if (!Application.isPlaying)
    {
        // 添加是否每帧变形的选项
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Deformation Settings", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        bool deformEveryFrame = EditorGUILayout.Toggle("Deform Every Frame", lattice.deformEveryFrame);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(lattice, "Change Deform Setting");
            lattice.deformEveryFrame = deformEveryFrame;
            EditorUtility.SetDirty(lattice);
        }

        // ✨新增警告：如果没有开启 deform every frame
        if (!lattice.deformEveryFrame)
        {
            EditorGUILayout.HelpBox("Warning: Deform Every Frame is disabled. Object will not deform automatically!", MessageType.Warning);
        }

        // 只有当deformEveryFrame为false且Edit Mode激活时显示应用变形按钮
        if (!lattice.deformEveryFrame && lattice.isEditModeActive)
        {
            if (GUILayout.Button("Apply Deformation", GUILayout.Height(30)))
            {
                Undo.RecordObject(lattice, "Apply Deformation");
                lattice.needsDeform = true; // 标记需要变形
                EditorUtility.SetDirty(lattice);

                // 如果在编辑模式下，我们需要强制立即更新
                if (!Application.isPlaying)
                {
                    // 创建一个EditorApplication.delayCall来在下一帧执行变形
                    // 这是因为我们不能在这里直接调用Update方法
                    EditorApplication.delayCall += () =>
                    {
                        // 模拟Update方法中的变形部分
                        SimulateDeformInEditor(lattice);
                        // 强制场景视图重绘
                        SceneView.RepaintAll();
                    };
                }
            }

            EditorGUILayout.HelpBox("Click 'Apply Deformation' to update the mesh after moving control points.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Edit Mode Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (!lattice.isEditModeActive)
        {
            if (GUILayout.Button("Start Edit Mode", GUILayout.Height(30)))
            {
                Undo.RecordObject(lattice, "Start Lattice Edit Mode");
                lattice.StartEditMode();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Edit Mode", GUILayout.Height(30)))
            {
                Undo.RecordObject(lattice, "Stop Lattice Edit Mode");
                lattice.StopEditMode();
            }
        }

        EditorGUILayout.EndHorizontal(); // 先收掉横向布局

        EditorGUILayout.HelpBox(
                "Reset Lattice: Revert to saved or original control points.\n" +
                "Regenerate Lattice: Rebuild a brand new lattice, clearing previous deformations.",
                MessageType.Info);

        EditorGUILayout.BeginHorizontal(); // 再开新的横向布局

        if (GUILayout.Button("Reset Lattice", GUILayout.Height(30)))
        {
            Undo.RecordObject(lattice, "Reset Lattice");
            lattice.ResetMesh();
        }

        EditorGUILayout.EndHorizontal();

        if (lattice.isEditModeActive)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Edit Mode is active. You can select and move control points in the Scene view.", MessageType.Info);

            // 添加动画控制部分
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Controls", EditorStyles.boldLabel);
            
            GUIStyle warningStyle = new GUIStyle(EditorStyles.helpBox);
            warningStyle.normal.textColor = new Color(0.9f, 0.4f, 0.1f);
            
            if (!lattice.dynamicPropertiesCreated)
            {
                EditorGUILayout.HelpBox(
                    "Warning: This operation will modify the Lattice script by adding Vector3 properties for each control point.\n" +
                    "Make sure to backup your script before proceeding.", MessageType.Warning);
                
                if (GUILayout.Button("Create Animation Properties", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Script Modification",
                        "This will modify the Lattice script by adding Vector3 properties for each control point.\n\n" +
                        "Make sure to backup your script before proceeding.\n\n" +
                        "Current control point count: " + lattice.deformControlPoints.Count,
                        "Proceed", "Cancel"))
                    {
                        lattice.CreateDynamicControlPointProperties();
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Sync To Properties", GUILayout.Height(25)))
                {
                    Undo.RecordObject(lattice, "Sync To Properties");
                    lattice.SyncPropertiesFromControlPoints();
                    EditorUtility.SetDirty(lattice);
                }
                
                if (GUILayout.Button("Sync From Properties", GUILayout.Height(25)))
                {
                    Undo.RecordObject(lattice, "Sync From Properties");
                    lattice.SyncControlPointsFromProperties();
                    EditorUtility.SetDirty(lattice);
                }
                EditorGUILayout.EndHorizontal();
                
                // 显示同步选项
                EditorGUI.BeginChangeCheck();
                bool syncFromAnim = EditorGUILayout.Toggle("Sync From Animation", lattice.syncFromAnimation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(lattice, "Change Animation Sync Setting");
                    lattice.syncFromAnimation = syncFromAnim;
                    EditorUtility.SetDirty(lattice);
                }
                
                EditorGUILayout.Space();
                
                // 是否处于动画记录模式
                EditorGUI.BeginChangeCheck();
                bool isRecording = EditorGUILayout.Toggle("Animation Recording Mode", lattice.isRecordingAnimation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(lattice, "Toggle Animation Recording");
                    lattice.isRecordingAnimation = isRecording;
                    EditorUtility.SetDirty(lattice);
                }
                
                // 拖动时是否自动更新关键帧
                if (lattice.isRecordingAnimation)
                {
                    EditorGUI.BeginChangeCheck();
                    bool updateKeys = EditorGUILayout.Toggle("Update Keyframes When Dragging", lattice.updateKeyframesWhenDragging);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(lattice, "Toggle Keyframe Updating");
                        lattice.updateKeyframesWhenDragging = updateKeys;
                        EditorUtility.SetDirty(lattice);
                    }
                    
                    if (GUILayout.Button("Update Keyframes for Selected Points", GUILayout.Height(25)))
                    {
                        lattice.UpdateAnimationKeyframes();
                    }
                    
                    EditorGUILayout.HelpBox(
                        "Recording Mode: When enabled, dragging control points will update animation keyframes at the current time.\n\n" +
                        "1. Open Animation window\n" +
                        "2. Create/select an Animation clip\n" +
                        "3. Enable recording (red button)\n" +
                        "4. Set the time where you want keyframes\n" +
                        "5. Select and drag control points to create keyframes",
                        MessageType.Info);
                }
                
                EditorGUILayout.HelpBox(
                    $"Created {lattice.createdPropertyCount} animation properties.\n" +
                    "If you change the resolution or regenerate the lattice, you'll need to recreate the animation properties.",
                    MessageType.Info);
            }

            // Selection controls
            EditorGUILayout.LabelField("Selection Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All", GUILayout.Height(25)))
            {
                Undo.RecordObject(lattice, "Select All Control Points");
                lattice.SelectAllControlPoints();
            }

            if (GUILayout.Button("Clear Selection", GUILayout.Height(25)))
            {
                Undo.RecordObject(lattice, "Clear Selection");
                lattice.ClearSelection();
            }

            if (GUILayout.Button("Cancel Current Operation", GUILayout.Height(25)))
            {
                Undo.RecordObject(lattice, "Cancel Selection");
                lattice.CancelSelection();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Box Selection: Hold Shift + Left Mouse Button to draw selection box\n" +
                "Multi-Selection: Hold Ctrl while clicking or box-selecting to add/remove from selection",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Edit Mode is active. You can select and move control points in the Scene view.", MessageType.Info);

            // 在这里添加面选择UI代码
            // 添加面选择的UI
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Face Selection", EditorStyles.boldLabel);
            // 创建面选择按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Top"))
            {
                Undo.RecordObject(lattice, "Select Top Face");
                lattice.SelectFace(Lattice.FaceSelectionMode.Top);
            }
            if (GUILayout.Button("Bottom"))
            {
                Undo.RecordObject(lattice, "Select Bottom Face");
                lattice.SelectFace(Lattice.FaceSelectionMode.Bottom);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Left"))
            {
                Undo.RecordObject(lattice, "Select Left Face");
                lattice.SelectFace(Lattice.FaceSelectionMode.Left);
            }
            if (GUILayout.Button("Right"))
            {
                Undo.RecordObject(lattice, "Select Right Face");
                lattice.SelectFace(Lattice.FaceSelectionMode.Right);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Front"))
            {
                Undo.RecordObject(lattice, "Select Front Face");
                lattice.SelectFace(Lattice.FaceSelectionMode.Front);
            }
            if (GUILayout.Button("Back"))
            {
                Undo.RecordObject(lattice, "Select Back Face");
                lattice.SelectFace(Lattice.FaceSelectionMode.Back);
            }
            EditorGUILayout.EndHorizontal();
            // 添加清除选择按钮
            if (lattice.currentFaceSelection != Lattice.FaceSelectionMode.None)
            {
                if (GUILayout.Button("Clear Face Selection"))
                {
                    Undo.RecordObject(lattice, "Clear Face Selection");
                    lattice.SelectFace(Lattice.FaceSelectionMode.None);
                }
            }

            // 在面选择UI之后是原有的选择控件
            EditorGUILayout.LabelField("Selection Controls", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Regenerate Lattice", GUILayout.Height(25)))
            {
                Undo.RecordObject(lattice, "Regenerate Lattice");
                lattice.UpdateLattice();
            }

            // 只有当不是 SkinnedMeshRenderer 才显示 "Save Deformation" 按钮
            if (!lattice.isSkinnedMesh)
            {
                if (GUILayout.Button("Save Deformation", GUILayout.Height(25)))
                {
                    Undo.RecordObject(lattice, "Save Mesh Deformation");
                    lattice.SaveCurrentDeformation();
                    EditorUtility.DisplayDialog("Deformation Saved",
                        "Current mesh deformation has been permanently saved. Reset will no longer return to the original mesh state, but to this saved state.", "OK");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Skinned Mesh deformation cannot be permanently saved. Export as prefab instead.", MessageType.Warning);
            }

            // 先准备一个红色的按钮样式
            GUIStyle redButton = new GUIStyle(GUI.skin.button);
            redButton.normal.textColor = Color.white;    // 文字颜色
            redButton.normal.background = MakeTex(600, 1, new Color(0.8f, 0.2f, 0.2f)); // 浅红背景
            if (GUILayout.Button("Export Deformed Mesh as New Prefab", redButton, GUILayout.Height(25)))
            {
                Undo.RecordObject(lattice, "Save as New Prefab");
                lattice.ExportDeformedModelAsNewPrefab();
                EditorUtility.DisplayDialog("Deformed Mesh exported to new prefab",
                    "Current mesh deformation has successfully exported to a new prefab. Please check Assets/DeformedMeshes.", "OK");
            }

            EditorGUILayout.EndHorizontal();
        }
    }
    else
    {
        // In Play mode, show selection controls and a reset button
        EditorGUILayout.HelpBox("Selection Controls", MessageType.None);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Select All", GUILayout.Height(25)))
        {
            lattice.SelectAllControlPoints();
        }

        if (GUILayout.Button("Clear Selection", GUILayout.Height(25)))
        {
            lattice.ClearSelection();
        }

        if (GUILayout.Button("Cancel Operation", GUILayout.Height(25)))
        {
            lattice.CancelSelection();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Box Selection: Hold Shift + Left Mouse Button to draw selection box\n" +
            "Multi-Selection: Hold Ctrl while clicking to add/remove from selection",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Reset Lattice", GUILayout.Height(30)))
        {
            lattice.ResetMesh();
        }
        
        // 添加运行时动画控件
        if (lattice.dynamicPropertiesCreated)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Sync To Properties", GUILayout.Height(25)))
            {
                lattice.SyncPropertiesFromControlPoints();
            }
            
            if (GUILayout.Button("Sync From Properties", GUILayout.Height(25)))
            {
                lattice.SyncControlPointsFromProperties();
            }
            EditorGUILayout.EndHorizontal();
            
            // 显示同步选项
            EditorGUI.BeginChangeCheck();
            bool syncFromAnim = EditorGUILayout.Toggle("Sync From Animation", lattice.syncFromAnimation);
            if (EditorGUI.EndChangeCheck())
            {
                lattice.syncFromAnimation = syncFromAnim;
            }
            
            // 是否处于动画记录模式
            EditorGUI.BeginChangeCheck();
            bool isRecording = EditorGUILayout.Toggle("Animation Recording Mode", lattice.isRecordingAnimation);
            if (EditorGUI.EndChangeCheck())
            {
                lattice.isRecordingAnimation = isRecording;
            }
            
            // 拖动时是否自动更新关键帧
            if (lattice.isRecordingAnimation)
            {
                EditorGUI.BeginChangeCheck();
                bool updateKeys = EditorGUILayout.Toggle("Update Keyframes When Dragging", lattice.updateKeyframesWhenDragging);
                if (EditorGUI.EndChangeCheck())
                {
                    lattice.updateKeyframesWhenDragging = updateKeys;
                }
                
                if (GUILayout.Button("Update Keyframes for Selected Points", GUILayout.Height(25)))
                {
                    lattice.UpdateAnimationKeyframes();
                }
                
                EditorGUILayout.HelpBox(
                    "Recording Mode: When enabled, dragging control points will update animation keyframes at the current time.\n\n" +
                    "1. Open Animation window\n" +
                    "2. Create/select an Animation clip\n" +
                    "3. Enable recording (red button)\n" +
                    "4. Set the time where you want keyframes\n" +
                    "5. Select and drag control points to create keyframes",
                    MessageType.Info);
            }
        }
    }
}
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    // 在编辑器模式下模拟变形操作的辅助方法
    public void SimulateDeformInEditor(Lattice lattice)
    {
        // 确保我们有可用的网格
        if (lattice.modelMesh == null)
        {
            if (lattice.isSkinnedMesh && lattice.bakedMesh != null)
            {
                lattice.modelMesh = lattice.bakedMesh;
            }
            else if (lattice.GetComponent<MeshFilter>() != null && lattice.GetComponent<MeshFilter>().sharedMesh != null)
            {
                lattice.modelMesh = lattice.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                return; // 没有可用的网格，退出
            }
        }

        Vector3[] verts;

        if (lattice.isSkinnedMesh && lattice.bakedMesh != null)
        {
            // 使用烘焙网格的顶点
            verts = lattice.bakedMesh.vertices;
        }
        else if (lattice.modelMesh != null)
        {
            // 使用标准网格的顶点
            verts = lattice.modelMesh.vertices;
        }
        else
        {
            return; // 没有顶点数据，退出
        }

        // 确保我们有足够的网格坐标
        if (lattice.vertexCoordinates.Count != verts.Length)
        {
            lattice.InitMesh(); // 重新初始化网格数据
        }

        // 创建新的顶点数组
        Vector3[] newVerts = new Vector3[verts.Length];

        // 计算每个顶点的新位置
        for (int i = 0; i < verts.Length; i++)
        {
            newVerts[i] = lattice.EvalVertex(i);
        }

        // 应用新顶点到适当的网格
        MeshFilter meshFilter = lattice.GetComponent<MeshFilter>();

        if (lattice.isSkinnedMesh && lattice.bakedMesh != null)
        {
            if (newVerts.Length == lattice.bakedMesh.vertices.Length)
            {
                // 为了避免修改原始网格，创建一个副本
                Mesh deformedMesh = Instantiate(lattice.bakedMesh);
                deformedMesh.vertices = newVerts;
                deformedMesh.RecalculateNormals();
                deformedMesh.RecalculateBounds();

                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = deformedMesh;
                }
            }
        }
        else if (lattice.modelMesh != null)
        {
            if (newVerts.Length == lattice.modelMesh.vertices.Length)
            {
                // 创建一个副本以避免修改原始共享网格
                Mesh deformedMesh = Instantiate(lattice.modelMesh);
                deformedMesh.vertices = newVerts;
                deformedMesh.RecalculateNormals();
                deformedMesh.RecalculateBounds();

                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = deformedMesh;
                }
            }
        }

        // 重置标记
        lattice.needsDeform = false;
    }
}
#endif