using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelPrefab : MonoBehaviour
{
    public Material editMaterial, errorMaterial;
    public Renderer meshRenderer;

    bool isAvailable = true;
    public void SetAvailability(bool isAvailable)
    {
        if (isAvailable)
        {
            if (meshRenderer.material != editMaterial)
                meshRenderer.material = editMaterial;
        }
        else
        {
            if (meshRenderer.material != errorMaterial)
                meshRenderer.material = errorMaterial;
        }
        this.isAvailable = isAvailable;
    }

    public bool IsAvailable()
    {
        return isAvailable;
    }
}
