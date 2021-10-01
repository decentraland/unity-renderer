using DCL;
using UnityEngine;

public class FeatureFlagController : IFeatureFlagController
{
    private FeatureFlagBridge featureFlagBridgeComponent;

    public FeatureFlagController()
    {
        if (InitialSceneReferences.i.bridgeGameObject != null)
            featureFlagBridgeComponent = InitialSceneReferences.i.bridgeGameObject.AddComponent<FeatureFlagBridge>();
    }

    public void Dispose() { GameObject.Destroy(featureFlagBridgeComponent); }
}