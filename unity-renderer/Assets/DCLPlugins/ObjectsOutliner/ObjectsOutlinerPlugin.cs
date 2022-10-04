using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectsOutlinerPlugin : IPlugin
{
    private readonly DataStore_ObjectsOutliner dataStore;
    private readonly OutlinerConfig outlinerConfig;
    private Dictionary<Renderer, Shader> originalShaders = new Dictionary<Renderer, Shader>();

    public ObjectsOutlinerPlugin()
    {
        outlinerConfig = Resources.Load<OutlinerConfig>("OutlinerConfig");
        dataStore = DataStore.i.objectsOutliner;
        dataStore.renderers.OnSet += OnSet;
        OnSet(dataStore.renderers.Get());
    }

    private void OnSet(IEnumerable<Renderer> renderers)
    {
        //TODO: Evaluate caching this to avoid allocation and swap the dictionary reference
        Dictionary<Renderer, Shader> newOriginalShaders = new Dictionary<Renderer, Shader>();
        foreach (Renderer renderer in renderers)
        {
            if (originalShaders.ContainsKey(renderer))
            {
                newOriginalShaders.Add(renderer, originalShaders[renderer]);
                originalShaders.Remove(renderer);
                continue;
            }

            Material material = renderer.material;
            newOriginalShaders.Add(renderer, material.shader);
            material.shader = outlinerConfig.shaderOutline;
            ObjectsOutlinerUtils.PrepareMaterial(material, outlinerConfig);

            //Remove from the originalMaterial to leave there the one no longer needed
            originalShaders.Remove(renderer);
        }

        // All renderers left in originalMaterials are not outlined anymore
        foreach ((Renderer renderer, Shader originalShader) in originalShaders)
        {
            renderer.material.shader = originalShader;
        }
        originalShaders = newOriginalShaders;
    }

    public void Dispose()
    {
        foreach ((Renderer renderer, Shader originalShader) in originalShaders)
        {
            renderer.material.shader = originalShader;
        }
        originalShaders.Clear();
        dataStore.renderers.OnSet -= OnSet;
    }
}