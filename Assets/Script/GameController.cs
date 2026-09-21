using UnityEngine;

public class GameController : MonoBehaviour
{
    // Visual settings for the lattice
    [Header("Visual Settings")]
    public bool showControlPoints = true;
    public bool showConnectionLines = true;

    [Header("Face Selection")]
    public string currentSelectedFace = "None";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize with current visibility settings
        UpdateVisibility();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Randomize all FFD control points (small disturbance)
    public void Randomise()
    {
        // Find all Lattice components in the scene
        Lattice[] latticeComponents = FindObjectsOfType<Lattice>();
        
        // Small random range for adjustments
        float randomRange = 0.3f;
        
        foreach (Lattice lattice in latticeComponents)
        {
            // Apply random disturbance to each control point
            for (int i = 0; i < lattice.deformControlPoints.Count; i++)
            {
                // Get original position
                Vector3 originalPos = lattice.deformControlPoints[i];
                
                // Apply small random offset
                Vector3 randomOffset = new Vector3(
                    Random.Range(-randomRange, randomRange),
                    Random.Range(-randomRange, randomRange),
                    Random.Range(-randomRange, randomRange)
                );
                
                // Set new position
                lattice.deformControlPoints[i] = originalPos + randomOffset;
            }
        }
    }


    public void SelectTopFace()
    {
        SelectFace(Lattice.FaceSelectionMode.Top);
        currentSelectedFace = "Top";
    }

    public void SelectBottomFace()
    {
        SelectFace(Lattice.FaceSelectionMode.Bottom);
        currentSelectedFace = "Bottom";
    }

    public void SelectLeftFace()
    {
        SelectFace(Lattice.FaceSelectionMode.Left);
        currentSelectedFace = "Left";
    }

    public void SelectRightFace()
    {
        SelectFace(Lattice.FaceSelectionMode.Right);
        currentSelectedFace = "Right";
    }

    public void SelectFrontFace()
    {
        SelectFace(Lattice.FaceSelectionMode.Front);
        currentSelectedFace = "Front";
    }

    public void SelectBackFace()
    {
        SelectFace(Lattice.FaceSelectionMode.Back);
        currentSelectedFace = "Back";
    }

    public void ClearFaceSelection()
    {
        SelectFace(Lattice.FaceSelectionMode.None);
        currentSelectedFace = "None";
    }

    // 通用的面选择方法
    private void SelectFace(Lattice.FaceSelectionMode faceMode)
    {
        Lattice[] latticeComponents = FindObjectsOfType<Lattice>();

        foreach (Lattice lattice in latticeComponents)
        {
            if (lattice != null)
            {
                lattice.SelectFace(faceMode);
            }
        }
    }

    // Reset all FFD control points to initial state
    public void Reset()
    {
        Lattice[] latticeComponents = FindObjectsOfType<Lattice>();
        
        foreach (Lattice lattice in latticeComponents)
        {
            // 调用ResetMesh替代UpdateLattice，确保行为一致
            lattice.ResetMesh();
        }
    }

    // Toggle connection lines visibility
    public void HideConnectionLines()
    {
        showConnectionLines = !showConnectionLines;
        UpdateVisibility();
    }

    // Toggle control points visibility
    public void HideConnectionNodes()
    {
        showControlPoints = !showControlPoints;
        UpdateVisibility();
    }
    
    // Update visibility settings on all lattice components
    private void UpdateVisibility()
    {
        Lattice[] latticeComponents = FindObjectsOfType<Lattice>();
        
        foreach (Lattice lattice in latticeComponents)
        {
            // Update the visibility settings
            if (lattice != null)
            {
                lattice.showControlPoints = showControlPoints;
                lattice.showConnectionLines = showConnectionLines;
            }
        }
    }
    public void ExportDeformedMeshes()
    {
        Lattice[] latticeComponents = FindObjectsOfType<Lattice>();

        foreach (Lattice lattice in latticeComponents)
        {
            if (lattice != null)
            {
                lattice.ExportDeformedModelAsNewPrefab();
            }
        }
    }

}