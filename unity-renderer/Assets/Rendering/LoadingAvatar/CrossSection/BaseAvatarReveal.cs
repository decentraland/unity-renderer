using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DCL.Helpers;
using DCL;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class BaseAvatarReveal : MonoBehaviour, IBaseAvatarRevealer
{
    [SerializeField] private Animation animation;
    [SerializeField] private List<GameObject> particleEffects;

    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    public Color baseColor;
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    public Color maxColor;

    public SkinnedMeshRenderer meshRenderer;
    public bool avatarLoaded;
    public GameObject revealer;

    private ILOD lod;

    public float revealSpeed;

    public float fadeInSpeed;
    Material _ghostMaterial;

    private float startH;
    private float startS;
    private float startV;
    private float endH;
    private float endS;
    private float endV;
    private bool isRevealing;

    public List<Renderer> targets = new List<Renderer>();
    List<Material> _materials = new List<Material>();

    private void Start()
    {
        _ghostMaterial = meshRenderer.material;
        InitializeColorGradient();
        foreach (Renderer r in targets)
        {
            _materials.Add(r.material);
        }
    }

    public SkinnedMeshRenderer GetMainRenderer()
    {
        return meshRenderer;
    }

    private void InitializeColorGradient()
    {
        Color.RGBToHSV(baseColor, out startH, out startS, out startV);
        Color.RGBToHSV(maxColor, out endH, out endS, out endV);
        Color newColor = Color.HSVToRGB(Random.Range(startH, endH), startS, startV);
        _ghostMaterial.SetColor("_Color", new Color(newColor.r, newColor.g, newColor.b, 0));
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

    public async UniTask StartAvatarRevealAnimation(bool withTransition, CancellationToken cancellationToken)
    {
        try
        {
            if (!withTransition)
            {
                SetFullRendered();
                return;
            }

            isRevealing = true;
            animation.Play();
            await UniTask.WaitUntil(() => !isRevealing, cancellationToken: cancellationToken).AttachExternalCancellation(cancellationToken);
        }
        catch(OperationCanceledException)
        {
            SetFullRendered();
        }
    }

    public void OnRevealAnimationEnd()
    {
        isRevealing = false;
        meshRenderer.enabled = false;
    }

    private void SetFullRendered()
    {
        meshRenderer.enabled = false;
        animation.Stop();
        const float REVEALED_POSITION = -10;
        foreach (Material m in _materials)
        {
            m.SetVector("_RevealPosition", new Vector3(0, REVEALED_POSITION, 0));
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
