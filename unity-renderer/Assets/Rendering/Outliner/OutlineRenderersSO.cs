using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create OutlineRenderers", fileName = "OutlineRenderers", order = 0)]
public class OutlineRenderersSO : ScriptableObject
{
    [NonSerialized] public readonly List<(Renderer avatar, int meshCount)> avatars = new List<(Renderer avatar, int meshCount)>();
    [NonSerialized] public List<(Renderer renderer, int meshCount)> renderers;
}
