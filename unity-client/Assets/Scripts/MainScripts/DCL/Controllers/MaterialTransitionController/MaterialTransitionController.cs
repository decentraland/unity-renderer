using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTransitionController : MonoBehaviour
{
    enum State
    {
        NOT_LOADED,
        SHOWING_LOADED,
        FINISHED,
        INVALID,
    }

    State state = State.NOT_LOADED;

    private static int ShaderId_CullYPlane = Shader.PropertyToID("_CullYPlane");
    private static int ShaderId_FadeThickness = Shader.PropertyToID("_FadeThickness");
    private static int ShaderId_FadeDirection = Shader.PropertyToID("_FadeDirection");
    private static int ShaderId_LoadingColor = Shader.PropertyToID("_LoadingColor");

    Material loadingMaterial;

    [System.NonSerialized] public float delay = 0.5f;
    [System.NonSerialized] public bool useHologram = true;
    [System.NonSerialized] public float fadeThickness = 10;
    [System.NonSerialized] public System.Action onFinishedLoading;

    static Material hologramMaterial;

    List<Material> loadingMaterialCopies;
    Material hologramMaterialCopy;

    public Material[] finalMaterials;
    Material[] cullingFXMaterials;

    Renderer targetRendererValue;

    Renderer targetRenderer
    {
        get
        {
            if (targetRendererValue == null)
            {
                targetRendererValue = GetComponent<Renderer>();
            }

            return targetRendererValue;
        }
    }

    public GameObject placeholder { get; private set; }
    public Renderer placeholderRenderer { get; private set; }

    float lowerYRendererBounds;
    float topYRendererBounds;
    float currentCullYPlane;
    float time;

    public bool materialReady { get; private set; }
    public bool canSwitchMaterial { get { return materialReady && state != State.FINISHED; } }

    public void PopulateTargetRendererWithMaterial(Material[] newMaterials, bool updateCulling = false)
    {
        if (newMaterials == null) return;

        Material material;
        for (int i = 0; i < newMaterials.Length; i++)
        {
            material = newMaterials[i];

            material.SetColor(ShaderId_LoadingColor, Color.clear);
            material.SetFloat(ShaderId_FadeDirection, 0);
            material.SetFloat(ShaderId_FadeThickness, fadeThickness);

            if (updateCulling)
                material.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
        }

        targetRenderer.sharedMaterials = newMaterials;
    }

    void UpdateCullYValueFXMaterial()
    {
        if (cullingFXMaterials != null)
        {
            for (int i = 0; i < cullingFXMaterials.Length; i++)
            {
                Material material = cullingFXMaterials[i];
                material.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
            }
        }
    }

    void UpdateCullYValueHologram()
    {
        if (hologramMaterialCopy != null)
            hologramMaterialCopy.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
    }

    void PrepareCullingFXMaterials()
    {
        cullingFXMaterials = new Material[finalMaterials.Length];
        for (int i = 0; i < finalMaterials.Length; i++)
        {
            cullingFXMaterials[i] = new Material(finalMaterials[i]);
        }
    }

    private void Awake()
    {
        state = State.NOT_LOADED;
        time = 0;
        materialReady = false;
    }

    private void Start()
    {
        Renderer tr = targetRenderer;

        if (float.IsInfinity(tr.bounds.min.y) || float.IsNaN(tr.bounds.min.y))
        {
            Destroy(this);
            return;
        }

        if (float.IsInfinity(tr.bounds.max.y) || float.IsNaN(tr.bounds.max.y))
        {
            Destroy(this);
            return;
        }

        tr.enabled = false;

        if (useHologram)
        {
            InitHologram();
        }

        lowerYRendererBounds = GetLowerBoundsY(tr);
        topYRendererBounds = GetTopBoundsY(tr);
        currentCullYPlane = lowerYRendererBounds;
    }

    void InitHologram()
    {
        placeholder = new GameObject("Load Placeholder");

        placeholder.transform.SetParent(transform, false);
        placeholder.transform.ResetLocalTRS();

        placeholderRenderer = placeholder.AddComponent<MeshRenderer>();
        MeshFilter newMeshFilter = placeholder.AddComponent<MeshFilter>();
        newMeshFilter.sharedMesh = GetComponent<MeshFilter>().sharedMesh;

        if (hologramMaterial == null)
            hologramMaterial = Resources.Load("Materials/HologramMaterial") as Material;

        hologramMaterialCopy = new Material(hologramMaterial);
        placeholderRenderer.sharedMaterials = new Material[] { hologramMaterialCopy };
    }

    private void Update()
    {
        if (targetRendererValue == null)
        {
            DestroyPlaceholder();
            state = State.INVALID;
            return;
        }

        switch (state)
        {
            case State.NOT_LOADED:
                {
                    currentCullYPlane += (topYRendererBounds - currentCullYPlane) * 0.1f;
                    currentCullYPlane = Mathf.Clamp(currentCullYPlane, lowerYRendererBounds, topYRendererBounds);
                    time += Time.deltaTime;

                    UpdateCullYValueHologram();

                    if (materialReady && time > delay)
                    {
                        currentCullYPlane = topYRendererBounds;
                        targetRendererValue.enabled = true;

                        PrepareCullingFXMaterials();
                        PopulateTargetRendererWithMaterial(cullingFXMaterials, true);

                        state = State.SHOWING_LOADED;
                    }

                    break;
                }

            case State.SHOWING_LOADED:
                {
                    currentCullYPlane += (lowerYRendererBounds - currentCullYPlane) * 0.1f;
                    currentCullYPlane = Mathf.Clamp(currentCullYPlane, lowerYRendererBounds, topYRendererBounds);

                    UpdateCullYValueHologram();
                    UpdateCullYValueFXMaterial();

                    if (currentCullYPlane <= lowerYRendererBounds + 0.1f)
                    {
                        // We don't update the culling value in the final material to avoid affecting the already-loaded meshes
                        PopulateTargetRendererWithMaterial(finalMaterials);

                        DestroyPlaceholder();
                        state = State.FINISHED;
                    }

                    break;
                }

            case State.FINISHED:
                {
                    onFinishedLoading?.Invoke();
                    Destroy(this);
                    break;
                }
        }
    }

    private void OnDestroy()
    {
        DestroyPlaceholder();

        if (loadingMaterialCopies != null)
        {
            for (int i = 0; i < loadingMaterialCopies.Count; i++)
            {
                Material m = loadingMaterialCopies[i];
                if (m != null)
                {
                    Destroy(m);
                }
            }
        }
    }

    void DestroyPlaceholder()
    {
        Destroy(hologramMaterialCopy);
        hologramMaterialCopy = null;

        if (placeholder != null)
        {
            Destroy(placeholder);
        }

        if (cullingFXMaterials != null)
        {
            for (int i = 0; i < cullingFXMaterials.Length; i++)
            {
                Destroy(cullingFXMaterials[i]);
            }
        }
    }

    public void OnDidFinishLoading(Material finishMaterial)
    {
        finalMaterials = new Material[] { finishMaterial };
        materialReady = true;
    }

    public float GetLowerBoundsY(Renderer targetRenderer)
    {
        return targetRenderer.bounds.min.y - fadeThickness;
    }

    public float GetTopBoundsY(Renderer targetRenderer)
    {
        return targetRenderer.bounds.max.y + fadeThickness;
    }

    public static void ApplyToLoadedObject(GameObject meshContainer, bool useHologram = true, float fadeThickness = 20, float delay = 0)
    {
        Renderer[] renderers = meshContainer.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];

            if (r.gameObject.GetComponent<MaterialTransitionController>() != null)
                continue;

            MaterialTransitionController transition = r.gameObject.AddComponent<MaterialTransitionController>();
            Material finalMaterial = r.sharedMaterial;
            transition.delay = delay;
            transition.useHologram = useHologram;
            transition.fadeThickness = fadeThickness;
            transition.OnDidFinishLoading(finalMaterial);
        }
    }
}
