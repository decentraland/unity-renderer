using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogExcludeBehavior : MonoBehaviour
{
    private bool revertFogState = false;

    void OnPreRender()
    {
        revertFogState = RenderSettings.fog;
        RenderSettings.fog = false;
    }

    void OnPostRender() { RenderSettings.fog = revertFogState; }
}