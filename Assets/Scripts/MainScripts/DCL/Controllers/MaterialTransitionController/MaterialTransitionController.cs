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

    public Material hologramMaterial;
    List<Material> loadingMaterialCopies;

    public Material[] finalMaterials;

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
    MaterialPropertyBlock placeholderPropertyBlock;

    float lowerYRendererBounds;
    float topYRendererBounds;
    float currentCullYPlane;
    float time;

    public bool materialReady { get; private set; }
    public bool canSwitchMaterial { get { return materialReady && state != State.FINISHED; } }

    public void PopulateLoadingMaterialWithFinalMaterial()
    {
        for (int i = 0; i < finalMaterials.Length; i++)
        {
            Material material = finalMaterials[i];

            material.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
            material.SetColor(ShaderId_LoadingColor, Color.clear);
            material.SetFloat(ShaderId_FadeDirection, 0);
            material.SetFloat(ShaderId_FadeThickness, fadeThickness);
        }

        targetRenderer.sharedMaterials = finalMaterials;
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

        placeholderPropertyBlock = new MaterialPropertyBlock();
    }

    void InitHologram()
    {
        placeholder = new GameObject("Load Placeholder");

        placeholder.transform.SetParent(transform, false);
        placeholder.transform.ResetLocalTRS();

        placeholderRenderer = placeholder.AddComponent<MeshRenderer>();
        MeshFilter newMeshFilter = placeholder.AddComponent<MeshFilter>();
        newMeshFilter.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        hologramMaterial = Resources.Load("Materials/HologramMaterial") as Material;
        placeholderRenderer.sharedMaterials = new Material[] { hologramMaterial };
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

                    if (placeholderRenderer != null)
                    {
                        placeholderRenderer.GetPropertyBlock(placeholderPropertyBlock);
                        placeholderPropertyBlock.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
                        placeholderRenderer.SetPropertyBlock(placeholderPropertyBlock);
                    }

                    if (materialReady && time > delay)
                    {
                        currentCullYPlane = topYRendererBounds;
                        targetRendererValue.enabled = true;
                        PopulateLoadingMaterialWithFinalMaterial();

                        state = State.SHOWING_LOADED;
                    }

                    break;
                }

            case State.SHOWING_LOADED:
                {
                    currentCullYPlane += (lowerYRendererBounds - currentCullYPlane) * 0.1f;
                    currentCullYPlane = Mathf.Clamp(currentCullYPlane, lowerYRendererBounds, topYRendererBounds);

                    for (int i = 0; i < targetRendererValue.sharedMaterials.Length; i++)
                    {
                        if (targetRendererValue != null)
                        {
                            targetRendererValue.GetPropertyBlock(placeholderPropertyBlock);
                            placeholderPropertyBlock.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
                            targetRendererValue.SetPropertyBlock(placeholderPropertyBlock, i);
                        }
                    }

                    if (placeholderRenderer != null)
                    {
                        placeholderRenderer.GetPropertyBlock(placeholderPropertyBlock);
                        placeholderPropertyBlock.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
                        placeholderRenderer.SetPropertyBlock(placeholderPropertyBlock);
                    }

                    if (currentCullYPlane <= lowerYRendererBounds + 0.1f)
                    {
                        DestroyPlaceholder();
                        state = State.FINISHED;
                    }

                    break;
                }

            case State.FINISHED:
                {
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
            foreach (Material m in loadingMaterialCopies)
            {
                if (m != null)
                {
                    Destroy(m);
                }
            }
        }
    }

    void DestroyPlaceholder()
    {
        if (placeholder != null)
        {
            Destroy(placeholder);
        }
    }

    public void OnDidFinishLoading(Material finishMaterial)
    {
        if (finishMaterial.shader.name.Contains("Simple"))
        {
            loadingMaterial = Utils.EnsureResourcesMaterial("Materials/LoadingTextureMaterial_LitSimple");
        }
        else
        {
            loadingMaterial = Utils.EnsureResourcesMaterial("Materials/LoadingTextureMaterial_Lit");
        }

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

    static MaterialPropertyBlock block = null;

    public static void ApplyToLoadedObjectFast(GameObject meshContainer)
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        Renderer[] renderers = meshContainer.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            float lowBounds = r.bounds.min.y - 20;
            r.GetPropertyBlock(block);
            block.SetFloat(ShaderId_CullYPlane, lowBounds);
            r.SetPropertyBlock(block);
        }
    }


    public static void ApplyToLoadedObject(GameObject meshContainer, bool useHologram = true, float fadeThickness = 20,
        float delay = 0)
    {
        Renderer[] renderers = meshContainer.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            MaterialTransitionController transition = r.gameObject.AddComponent<MaterialTransitionController>();
            Material finalMaterial = r.sharedMaterial;
            transition.delay = delay;
            transition.useHologram = useHologram;
            transition.fadeThickness = fadeThickness;
            transition.OnDidFinishLoading(finalMaterial);
        }
    }
}