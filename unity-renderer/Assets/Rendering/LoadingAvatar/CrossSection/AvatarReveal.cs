using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using DCL;
using AvatarSystem;

public class AvatarReveal : MonoBehaviour
{
    [SerializeField] private Animation animation;
    [SerializeField] private List<GameObject> particleEffects;

    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    public Color baseColor;
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    public Color maxColor;

    private Gradient colorGradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;

    public SkinnedMeshRenderer meshRenderer;
    public bool avatarLoaded;
    public GameObject revealer;

    private ILOD lod;

    public float revealSpeed;

    public float fadeInSpeed;
    Material _ghostMaterial;
    
    public List<Renderer> targets = new List<Renderer>();
    List<Material> _materials = new List<Material>();

    private void Start()
    {
        _ghostMaterial = meshRenderer.material;
        InitializeColorGradient();
        _ghostMaterial.SetColor("_Color", colorGradient.Evaluate(Random.Range(0, 1f)));
        foreach (Renderer r in targets)
        {
            _materials.Add(r.material);
        }
    }

    private void InitializeColorGradient()
    {
        colorGradient = new Gradient();
        colorKey = new GradientColorKey[2];
        colorKey[0].color = baseColor;
        colorKey[0].time = 0.0f;
        colorKey[1].color = maxColor;
        colorKey[1].time = 1.0f;

        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = baseColor.a;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = baseColor.a;
        alphaKey[1].time = 1.0f;
        colorGradient.SetKeys(colorKey, alphaKey);
    }

    public void InjectLodSystem(ILOD lod)
    {
        this.lod = lod;
    }

    public void AddTarget(MeshRenderer newTarget)
    {
        if (newTarget == null)
            return;

        targets.Add(newTarget);
        newTarget?.material.SetVector("_RevealPosition", Vector3.zero);
        newTarget?.material.SetVector("_RevealNormal", new Vector3(0, -1, 0));
        _materials.Add(newTarget.material);
    }

    private void Update()
    {
        if (lod.lodIndex >= 2)
            SetFullRendered();

        UpdateMaterials();
    }

    void UpdateMaterials()
    {
        if (avatarLoaded)
            return;

        if(_ghostMaterial.GetColor("_Color").a < 0.9f)
        {
            Color gColor = _ghostMaterial.GetColor("_Color");
            Color tempColor = new Color(gColor.r, gColor.g, gColor.b, gColor.a + Time.deltaTime * fadeInSpeed);
            _ghostMaterial.SetColor("_Color", tempColor);
        }

        _ghostMaterial.SetVector("_RevealPosition", revealer.transform.localPosition);

        foreach (Material m in _materials)
        {
            m.SetVector("_RevealPosition", -revealer.transform.localPosition);
        }
    }

    public void StartAvatarRevealAnimation(bool closeby)
    {
        if (closeby)
            animation.Play();
        else
            SetFullRendered();
    }

    public void SetFullRendered()
    {
        animation.Stop();
        foreach (Material m in _materials)
        {
            m.SetVector("_RevealPosition", new Vector3(0, -2.5f, 0));
        }
        _ghostMaterial.SetVector("_RevealPosition", new Vector3(0, 2.5f, 0));
        DisableParticleEffects();
        avatarLoaded = true;
    }

    public void Reset()
    {
        Color gColor = _ghostMaterial.GetColor("_Color");
        Color tempColor = new Color(gColor.r, gColor.g, gColor.b, 0);
        _ghostMaterial.SetColor("_Color", tempColor);
        avatarLoaded = false;
        meshRenderer.enabled = true;
        targets = new List<Renderer>();
        _materials = new List<Material>();
        revealer.transform.position = Vector3.zero;
    }

    private void DisableParticleEffects()
    {
        foreach (GameObject p in particleEffects)
        {
            p.SetActive(false);
        }
    }

    public void OnDisable()
    {
        SetFullRendered();
    }
}
