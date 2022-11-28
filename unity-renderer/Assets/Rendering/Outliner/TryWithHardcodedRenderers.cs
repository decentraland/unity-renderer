// unset:none
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TryWithHardcodedRenderers : MonoBehaviour
{
    public List<Renderer> theRenderers;
    public OutlineRenderers outlineRenderers;

    private void Update()
    {
        if (outlineRenderers != null) outlineRenderers.renderers = theRenderers.Select(x => (x, x.GetComponent<MeshFilter>().sharedMesh.subMeshCount)).ToList();
    }

    private void OnDestroy()
    {
        if (outlineRenderers != null) outlineRenderers.renderers = null;
    }
}
