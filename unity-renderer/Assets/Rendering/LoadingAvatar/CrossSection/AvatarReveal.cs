using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarReveal : MonoBehaviour
{
    [SerializeField] private Animation animation;
    public SkinnedMeshRenderer meshRenderer;
    public bool avatarLoaded;
    public GameObject revealer;

    public float revealSpeed;

    public float fadeInSpeed;
    Material _ghostMaterial;
    
    public List<Renderer> targets = new List<Renderer>();
    List<Material> _materials = new List<Material>();

    private void Start()
    {
        _ghostMaterial = meshRenderer.material;
        foreach (Renderer r in targets)
        {
            _materials.Add(r.material);
        }
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
        UpdateMaterials();
    }

    void UpdateMaterials()
    {
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

    public void StartAvatarRevealAnimation()
    {
        animation.Play();
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
}
