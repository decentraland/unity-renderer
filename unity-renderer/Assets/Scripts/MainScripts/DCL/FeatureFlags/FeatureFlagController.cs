using DCL;
using UnityEngine;

public class FeatureFlagController : IFeatureFlagController
{
    internal FeatureFlagBridge featureFlagBridgeComponent;

    public FeatureFlagController(GameObject bridgeGameObject)
    {
        AddBridgeComponent(bridgeGameObject);
    }

    public void AddBridgeComponent(GameObject gameObjectToAddBridge)
    {
        if (gameObjectToAddBridge == null)
            return;

        if (featureFlagBridgeComponent != null)
            Dispose();

        featureFlagBridgeComponent = gameObjectToAddBridge.AddComponent<FeatureFlagBridge>();
    }

    public void Dispose()
    {
        Object.Destroy(featureFlagBridgeComponent);
        featureFlagBridgeComponent = null;
    }
}