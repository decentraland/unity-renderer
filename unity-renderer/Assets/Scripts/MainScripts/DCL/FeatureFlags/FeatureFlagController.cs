using DCL;
using UnityEngine;

public class FeatureFlagController : IFeatureFlagController
{
    internal FeatureFlagBridge featureFlagBridgeComponent;

    public FeatureFlagController()
    {
        if (InitialSceneReferences.i.bridgeGameObject != null)
            AddBridgeComponent(InitialSceneReferences.i.bridgeGameObject );
    }

    public void AddBridgeComponent(GameObject gameObjectToAddBridge)
    {
        if (featureFlagBridgeComponent != null)
            Dispose();
        featureFlagBridgeComponent = gameObjectToAddBridge.AddComponent<FeatureFlagBridge>();
    }

    public void Dispose() { GameObject.Destroy(featureFlagBridgeComponent); }
}