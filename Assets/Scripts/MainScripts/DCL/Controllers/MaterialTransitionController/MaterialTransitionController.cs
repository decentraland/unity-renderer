using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

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

    [System.NonSerialized] public float delay = 0.5f;
    [System.NonSerialized] public bool useHologram = true;
    [System.NonSerialized] public float fadeThickness = 10;

    public Material hologramMaterial;
    public Material loadingMaterial;
    public Material[] finalMaterials;

    Renderer targetRendererValue;

    Renderer targetRenderer
    {
        get
        {
            if (targetRendererValue == null)
                targetRendererValue = GetComponent<Renderer>();

            return targetRendererValue;
        }
    }

    public GameObject placeholder { get; private set; }

    float lowerYRendererBounds;
    float topYRendererBounds;
    float currentCullYPlane;
    float time;

    public bool materialReady { get; private set; }
    public bool canSwitchMaterial { get { return materialReady && state != State.FINISHED; } }
    
    public void PopulateLoadingMaterialWithFinalMaterial()
    {
        List<Material> newMats = new List<Material>();

        for (int i = 0; i < finalMaterials.Length; i++)
        {
            Material material = new Material( loadingMaterial );
            material.CopyPropertiesFromMaterial(finalMaterials[i]);

            material.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
            material.SetColor(ShaderId_LoadingColor, Color.clear);
            material.SetFloat(ShaderId_FadeDirection, 0);
            material.SetFloat(ShaderId_FadeThickness, fadeThickness);

            newMats.Add(material);
        }

        targetRenderer.materials = newMats.ToArray();
    }

    private void Awake()
    {
        state = State.NOT_LOADED;
        time = 0;
        materialReady = false;
    }

    private void Start()
    {
        targetRenderer.enabled = false;

        if ( useHologram )
            InitHologram();

        lowerYRendererBounds = GetLowerBoundsY(targetRenderer);
        topYRendererBounds = GetTopBoundsY(targetRenderer);
        currentCullYPlane = lowerYRendererBounds;
    }

    void InitHologram()
    {
        placeholder = new GameObject("Load Placeholder");

        placeholder.transform.SetParent(transform, false);
        placeholder.transform.ResetLocalTRS();

        MeshRenderer newRenderer = placeholder.AddComponent<MeshRenderer>();
        MeshFilter newMeshFilter = placeholder.AddComponent<MeshFilter>();
        newMeshFilter.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        hologramMaterial = new Material(Resources.Load("Materials/HologramMaterial") as Material);
        newRenderer.materials = new Material[] { hologramMaterial };
    }



    private void Update()
    {
        if (targetRenderer == null)
        {
            if ( placeholder != null )
                Destroy(placeholder);

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

                    if ( hologramMaterial != null )
                        hologramMaterial.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
                    
                    if (materialReady && time > delay)
                    {
                        currentCullYPlane = topYRendererBounds;
                        targetRenderer.enabled = true;
                        PopulateLoadingMaterialWithFinalMaterial();

                        state = State.SHOWING_LOADED;
                    }
                    break;
                }
            case State.SHOWING_LOADED:
                {
                    currentCullYPlane += (lowerYRendererBounds - currentCullYPlane) * 0.1f;
                    currentCullYPlane = Mathf.Clamp(currentCullYPlane, lowerYRendererBounds, topYRendererBounds);

                    for (int i = 0; i < targetRenderer.materials.Length; i++)
                    {
                        Material material = targetRenderer.materials[i];
                        material.SetFloat(ShaderId_CullYPlane, currentCullYPlane);
                    }

                    if (hologramMaterial != null)
                        hologramMaterial.SetFloat(ShaderId_CullYPlane, currentCullYPlane);

                    if (currentCullYPlane <= lowerYRendererBounds + 0.1f)
                    {
                        Destroy(placeholder);
                        targetRenderer.materials = finalMaterials;
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

    public void OnDidFinishLoading(Material finishMaterial)
    {
        if ( finishMaterial.shader.name.Contains("Simple") )
            loadingMaterial = Resources.Load("Materials/LoadingTextureMaterial_LitSimple") as Material;
        else
            loadingMaterial = Resources.Load("Materials/LoadingTextureMaterial_Lit") as Material;

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



}
