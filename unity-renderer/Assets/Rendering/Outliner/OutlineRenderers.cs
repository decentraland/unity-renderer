using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create OutlineRenderers", fileName = "OutlineRenderers", order = 0)]
public class OutlineRenderers : ScriptableObject
{
    [NonSerialized] public List<(Renderer avatar, int meshCount)> avatars;
    [NonSerialized] public List<(Renderer renderer, int meshCount)> renderers;
}
