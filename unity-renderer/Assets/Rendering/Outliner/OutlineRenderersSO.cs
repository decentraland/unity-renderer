using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create OutlineRenderers", fileName = "OutlineRenderers", order = 0)]
public class OutlineRenderersSO : ScriptableObject
{
    [NonSerialized] public (Renderer renderer, int meshCount, float avatarHeight) avatar = (null, -1, -1);
    [NonSerialized] public List<(Renderer renderer, int meshCount)> renderers;
}
