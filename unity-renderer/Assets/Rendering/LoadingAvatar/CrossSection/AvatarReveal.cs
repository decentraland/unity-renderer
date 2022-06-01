using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarReveal : MonoBehaviour
{
    public SkinnedMeshRenderer meshRenderer;
    public bool avatarLoaded;
    public GameObject revealer;
    public Transform finalPosition;
    public ParticleSystem particles;
    public ParticleSystem revealParticles;

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

        if(avatarLoaded)
        {
            RevealAvatar();
        }
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

    void RevealAvatar()
    {
        if (revealer.transform.position.y < finalPosition.position.y)
        {
            revealer.transform.position += revealer.transform.up * revealSpeed * Time.deltaTime;

            if (!particles.isPlaying)
            {
                particles.Play();
                revealParticles.Play();
            }
        }
        else 
        {
            if (particles.isPlaying)
            {
                particles.Stop();
                revealParticles.Stop();
            }
            meshRenderer.enabled = false;
        }
    }
}
