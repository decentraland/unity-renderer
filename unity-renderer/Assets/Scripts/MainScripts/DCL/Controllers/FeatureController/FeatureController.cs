using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class FeatureController : MonoBehaviour
{
    public InitialSceneReferences sceneReferences;

    public GameObject builderInWorldFeaturePrefab;

    private List<Feature> activeFeatures = new List<Feature>();

    private GameObject builderInWorld;

    void Start()
    {
        KernelConfig.i.EnsureConfigInitialized().Then(HandleFeatures);
        KernelConfig.i.OnChange += OnKernelConfigChanged;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Feature feature in activeFeatures)
        {
            feature.Update();
        }
    }

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { HandleFeatures(current); }

    private void HandleFeatures(KernelConfigModel config) { HandleBuilderInWorld(config.features.enableBuilderInWorld); }

    private void HandleBuilderInWorld(bool isActive)
    {
        if (isActive)
        {
            if (builderInWorld != null)
                return;
            builderInWorld = Instantiate(builderInWorldFeaturePrefab);
        }
        else
        {
            if (builderInWorld != null)
                Destroy(builderInWorld);
        }
    }

}