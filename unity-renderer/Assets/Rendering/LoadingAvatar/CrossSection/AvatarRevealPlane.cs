using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AvatarRevealPlane : MonoBehaviour
{
    public Material avatarMaterial, ghostMaterial;
    void Start()
    {
        
    }

    
    void Update()
    {
        avatarMaterial.SetVector("_RevealPosition", transform.position);
        avatarMaterial.SetVector("_RevealNormal", transform.forward);
        ghostMaterial.SetVector("_RevealPosition", transform.position);
        ghostMaterial.SetVector("_RevealNormal", transform.forward);
    }
}
