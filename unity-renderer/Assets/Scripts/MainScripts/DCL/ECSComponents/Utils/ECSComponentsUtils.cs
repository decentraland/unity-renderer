using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ECSComponentsUtils
{
    public static void UpdateRenderer(IDCLEntity entity, Model model = null)
    {
        ConfigureVisibility(entity.meshRootGameObject, model.visible, entity.meshesInfo.renderers);
        
        CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, model.withCollisions, false, entity, CalculateCollidersLayer(model));

        if (entity.meshesInfo.meshFilters.Length > 0 && entity.meshesInfo.meshFilters[0].sharedMesh != currentMesh)
        {
            entity.meshesInfo.UpdateExistingMeshAtIndex(currentMesh, 0);
        }
    }
    
    public static void ConfigureVisibility(GameObject meshGameObject, bool shouldBeVisible, Renderer[] meshRenderers = null)
    {
        if (meshGameObject == null)
            return;

        if (!shouldBeVisible)
        {
            MaterialTransitionController[] materialTransitionControllers = meshGameObject.GetComponentsInChildren<MaterialTransitionController>();

            for (var i = 0; i < materialTransitionControllers.Length; i++)
            {
                Object.Destroy(materialTransitionControllers[i]);
            }
        }

        if (meshRenderers == null)
            meshRenderers = meshGameObject.GetComponentsInChildren<Renderer>(true);

        Collider onPointerEventCollider;

        for (var i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = shouldBeVisible;

            if (meshRenderers[i].transform.childCount > 0)
            {
                onPointerEventCollider = meshRenderers[i].transform.GetChild(0).GetComponent<Collider>();

                if (onPointerEventCollider != null && onPointerEventCollider.gameObject.layer == PhysicsLayers.onPointerEventLayer)
                    onPointerEventCollider.enabled = shouldBeVisible;
            }
        }
    }
}
