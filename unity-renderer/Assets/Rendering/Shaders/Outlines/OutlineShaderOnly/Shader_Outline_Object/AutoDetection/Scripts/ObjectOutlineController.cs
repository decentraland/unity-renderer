using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// force single component
[DisallowMultipleComponent]
public class ObjectOutlineController : MonoBehaviour
{

    // Class specifics

    public enum OutMode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlinePlusOccluded,
        OccludedOnly,
        CompatibleWithPlaneSurfaces
    }

    // outline mode
    public OutMode OutlineOutMode
    {

        // getters and setters

        get { return outlineOutMode; }
        
        set
        {
            outlineOutMode = value;
            isDuringUpdate = true;
        }
    }

    // outline color
    public Color OutlineColor
    {
        get { return outlineColor; }
        
        set
        {
            outlineColor = value;
            isDuringUpdate = true;
        }
    }

    // outline Fill color
    public Color OutlineFillColor
    {
        get { return outlineFillColor; }
        
        set
        {
            outlineFillColor = value;
            isDuringUpdate = true;
        }
    }

    // outline width
    public float OutlineWidth
    {
        get { return outlineWidth; }
        
        set
        {
            outlineWidth = value;
            isDuringUpdate = true;
        }
    }

    private static HashSet<Mesh> _registeredMeshes = new HashSet<Mesh>();

    // Mesh data 
    [Serializable] // must be serializable to keep track of the data
    private class ListVector3
    {
        public List<Vector3> data;
    }

    // variables
    [FormerlySerializedAs("outlineOutlinerMode")] [FormerlySerializedAs("outlineMode")]
    
    [Tooltip("Use OutlineAll combined with [ isOptimised ] and [ precomputeOutline ]  to get the best performance")]
    [SerializeField] private OutMode outlineOutMode;

    [SerializeField] private bool isOutlineEnabled = false; // will be mouse over enabled

    [SerializeField] private Color outlineColor = Color.white;

    [SerializeField] private Color outlineFillColor = Color.white;

    [SerializeField, Range(0f, 10f)] private float outlineWidth = 3f;

    [SerializeField] private bool isOptimized = true;

    [Header("For Editor and Runtime use")]
    [Tooltip("On enable outline wll be computed in editor , this is for testing only")]
    [SerializeField] private bool precomputeOutline = true;

    [Space]
    [SerializeField] private bool isDebug;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] renderers;
    private Material outlineMaskMaterial; // material for outline mask
    private Material outlineFillMaterial; // material for outline fill
    private Material outlinePlaneCompatibleMaterial; // material for outline compatible

    private bool isDuringUpdate;

    #region  Awake - Update - OnEnable - OnDisable

    void Awake()
    {
        PreBurn();
    }

    void OnEnable()
    {
        EnableOutline();
    }

    void Update()
    {

        if (isDuringUpdate == true && isOutlineEnabled == true)
        {
            isDuringUpdate = false;

            UpdateMaterialProperties();
        }

        if (isOutlineEnabled == false)
        {
            // change flag to not be renderable
            SetMatFlagsOff();
        }
        else if (isOutlineEnabled == true && isDuringUpdate == true)
        {
            // change flag to renderable
            SetMatFlags();
        }

    }

    void OnDisable()
    {
        DisableOutline();
    }

    #endregion

    #region On Destroy

    void OnDestroy()
    {
        DestroyMat();
    }

    #endregion

    #region In editor - Validate

    void OnValidate()
    {
        Initialize();
    }

    #endregion

    #region API

    public void EnableOutline()
    {
        if (outlineOutMode != OutMode.CompatibleWithPlaneSurfaces)
        {
            foreach (Renderer renderer in renderers)
            {

                // Append outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Add(outlineMaskMaterial);
                materials.Add(outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }
        }
        else // for plane surfaces
        {
            foreach (Renderer renderer in renderers)
            {

                // Append outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Add(outlinePlaneCompatibleMaterial);

                renderer.materials = materials.ToArray();
            }
        }

    }

    public void DisableOutline()
    {
        if (outlineOutMode != OutMode.CompatibleWithPlaneSurfaces)
        {
            foreach (Renderer renderer in renderers)
            {

                // Remove outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Remove(outlineMaskMaterial);
                materials.Remove(outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }
        }
        else // for plane surfaces
        {
            foreach (Renderer renderer in renderers)
            {

                // Remove outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Remove(outlinePlaneCompatibleMaterial);

                renderer.materials = materials.ToArray();
            }
        }
    }

    public void DestroyMat()
    {
        if (outlineOutMode != OutMode.CompatibleWithPlaneSurfaces)
        {
            // Destroy material instances
            Destroy(outlineMaskMaterial);
            Destroy(outlineFillMaterial);
        }
        else // for plane surfaces
        {
            Destroy(outlinePlaneCompatibleMaterial);
        }

    }

    #endregion

    #region Initialization

    void PreBurn()
    {
        // Cache renderers
        renderers = GetComponentsInChildren<Renderer>();

        // Instantiate outline materials
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        if (outlineOutMode == OutMode.CompatibleWithPlaneSurfaces)
        {
            // plane compatible
            outlinePlaneCompatibleMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlinePlaneCompatible"));
        }

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        if (outlineOutMode == OutMode.CompatibleWithPlaneSurfaces)
        {
            // plane compatible
            outlinePlaneCompatibleMaterial.name = "OutlinePlaneCompatible (Instance)";
        }

        if (isOptimized == false)
        {
            // Retrieve or generate smooth normals
            LoadSmoothNormals();
        }

        // Apply material properties immediately
        isDuringUpdate = true;
    }

    void Initialize()
    {
        // Update material properties
        isDuringUpdate = true;

        if (isOptimized == false)
        {
            // Clear cache when baking is disabled or corrupted
            if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
            {
                bakeKeys.Clear();
                bakeValues.Clear();
            }

            // Generate smooth normals when baking is enabled
            if (precomputeOutline && bakeKeys.Count == 0)
            {
                GenerateSmoothNormals();
            }
        }

    }

    #endregion

    #region Normals Procedures

    void GenerateSmoothNormals()
    {

        // Generate smooth normals 
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {

            // Skip duplicates
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Serialize smooth normals
            List<Vector3> smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    void LoadSmoothNormals()
    {

        // Retrieve or generate smooth normals
        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            // Skip if processed before
            if (!_registeredMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Retrieve or generate smooth normals
            int index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            List<Vector3> smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            // Store smooth normals in UV3
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);

            // Combine submeshes
            Renderer renderer = meshFilter.GetComponent<Renderer>();

            if (renderer != null)
            {
                CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
            }
        }

        // Clear UV3 on skinned mesh renderers
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {

            // Skip if UV3 has already been reset
            if (!_registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                continue;
            }

            // Clear UV3
            skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

            // Combine submeshes
            CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }

    List<Vector3> SmoothNormals(Mesh mesh)
    {

        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        // Copy normals to a new list
        List<Vector3> smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups)
        {

            // Skip single vertices
            if (group.Count() == 1)
            {
                continue;
            }

            // Calculate the average normal
            Vector3 smoothNormal = Vector3.zero;

            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        if (isDebug)
        {
            // mesh normals debug check
            Debug.Log("Mesh normals have been smoothed out");
        }
        

        return smoothNormals;
    }

    #endregion

    #region Meshes Procedures

    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {

        // Skip one submesh scenario
        if (mesh.subMeshCount == 1)
        {
            return;
        }

        // Skip if submesh count exceeds material count
        if (mesh.subMeshCount > materials.Length)
        {
            return;
        }

        // Append combined submesh
        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        
        if (isDebug)
        {
            // mesh normals debug check
            Debug.Log("Meshes have been combined");
        }
    }

    void UpdateMaterialProperties()
    {

        if (outlineOutMode != OutMode.CompatibleWithPlaneSurfaces)
        {
            // Apply properties according to mode
            outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
            outlineFillMaterial.SetColor("_OutlineFillColor", outlineColor);
        }
        else // for plane surfaces
        {
            // plane compatible
            outlinePlaneCompatibleMaterial.SetColor("_OutlineColor", outlineColor);
            outlinePlaneCompatibleMaterial.SetColor("FillColor", outlineFillColor);
        }

        SetMatFlags();

    }

    #endregion

    #region Material Flags

    void SetMatFlags()
    {
        switch (outlineOutMode)
        {

            case OutMode.OutlineAll:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OutlineVisible:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OutlineHidden:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OutlinePlusOccluded:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);

                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OccludedOnly:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);

                outlineFillMaterial.SetFloat("_OutlineWidth", 0f);

                break;

            case OutMode.CompatibleWithPlaneSurfaces:

                outlinePlaneCompatibleMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);

                outlinePlaneCompatibleMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;
        }
    }

    void SetMatFlagsOff()
    {
        switch (outlineOutMode)
        {

            case OutMode.OutlineAll:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OutlineVisible:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OutlineHidden:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OutlinePlusOccluded:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);

                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

                break;

            case OutMode.OccludedOnly:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);

                outlineFillMaterial.SetFloat("_OutlineWidth", 0f);

                break;

            case OutMode.CompatibleWithPlaneSurfaces:

                outlinePlaneCompatibleMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Never);

                break;
        }
    }

    #endregion

    #region Helper Functions

    void AddMatsBack()
    {
        foreach (var renderer in renderers)
        {

            // Remove outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }

    }

    #endregion

}