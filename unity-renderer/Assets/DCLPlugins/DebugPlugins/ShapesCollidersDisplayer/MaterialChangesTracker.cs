using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

internal class MaterialChangesTracker : IDisposable
{
    private readonly Dictionary<Renderer, Material> originalMaterials;
    private readonly Dictionary<Renderer, Material> entityWithColliderMaterials;
    private readonly MeshesInfo meshesInfo;

    private readonly Coroutine checkRoutine;

    public event Action<Renderer> OnRendererMaterialChanged;

    public MaterialChangesTracker(in MeshesInfo meshesInfo, in Dictionary<Renderer, Material> originalMaterials)
    {
        this.meshesInfo = meshesInfo;
        this.originalMaterials = originalMaterials;

        checkRoutine = CoroutineStarter.Start(UpdateRoutine());
    }

    public void Dispose()
    {
        CoroutineStarter.Stop(checkRoutine);
    }

    IEnumerator UpdateRoutine()
    {
        while (true)
        {
            DoMaterialChangeCheck();
            yield return null;
        }
    }

    private void DoMaterialChangeCheck()
    {
        for (int i = 0; i < meshesInfo.renderers.Length; i++)
        {
            Renderer renderer = meshesInfo.renderers[i];
            if (renderer == null)
                continue;

            Material rendererMaterial = renderer.sharedMaterial;
            if (rendererMaterial == null)
                continue;

            // check if current material is the material used for debugging
            if (rendererMaterial.name == EntityStyle.ENTITY_MATERIAL_NAME)
                continue;

            // check if current material is the same stored as the original material for this renderer
            if (originalMaterials.TryGetValue(renderer, out Material storedMaterial))
            {
                if (rendererMaterial == storedMaterial)
                    continue;
            }

            OnRendererMaterialChanged?.Invoke(renderer);
        }
    }

}