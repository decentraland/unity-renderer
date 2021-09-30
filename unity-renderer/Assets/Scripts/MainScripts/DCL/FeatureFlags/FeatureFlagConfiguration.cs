using DCL;
using UnityEngine;

public class FeatureFlagController : IFeatureFlagController
{
    private FeatureFlagBridge featureFlagBridgeComponent;

    public FeatureFlagController() { featureFlagBridgeComponent = InitialSceneReferences.i.bridgeGameObject.AddComponent<FeatureFlagBridge>(); }

    public void Dispose() { GameObject.Destroy(featureFlagBridgeComponent); }
}