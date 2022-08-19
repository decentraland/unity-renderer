
using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

public class MaterialTransitionControllerUtils {
    
    
    public static IEnumerator SetMaterialTransition(bool doTransition, HashSet<Renderer> renderers, Action OnSuccess = null, bool useHologram = true)
    {
        if (doTransition)
        {
            MaterialTransitionController[] materialTransitionControllers = new MaterialTransitionController[renderers.Count];
            int index = 0;
            foreach (Renderer assetRenderer in renderers)
            {
                MaterialTransitionController transition = assetRenderer.gameObject.GetOrCreateComponent<MaterialTransitionController>();
                materialTransitionControllers[index] = transition;
                transition.useHologram = useHologram;
                transition.OnDidFinishLoading(assetRenderer.sharedMaterial);
                index++;
            }
            // Wait until MaterialTransitionController finishes its effect
            yield return new WaitUntil(() => IsTransitionFinished(materialTransitionControllers));
        }
        OnSuccess?.Invoke();
    }
    
    public static bool IsTransitionFinished(MaterialTransitionController[] matTransitions)
    {
        bool finishedTransition = true;

        for (int i = 0; i < matTransitions.Length; i++)
        {
            if (matTransitions[i] != null)
            {
                finishedTransition = false;

                break;
            }
        }
        return finishedTransition;
    }
    
    public static void ApplyToLoadedObject(GameObject meshContainer, bool useHologram = true, float fadeThickness = 20,
        float delay = 0)
    {
        Renderer[] renderers = meshContainer.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];

            if (r.gameObject.GetComponent<MaterialTransitionController>() != null)
                continue;

            MaterialTransitionController transition = r.gameObject.AddComponent<MaterialTransitionController>();
            Material finalMaterial = r.sharedMaterial;
            transition.delay = delay;
            transition.useHologram = useHologram;
            transition.fadeThickness = fadeThickness;
            transition.OnDidFinishLoading(finalMaterial);
        }
    }

}
