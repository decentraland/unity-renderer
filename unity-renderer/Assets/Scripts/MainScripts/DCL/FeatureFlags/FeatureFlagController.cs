using DCL;
using UnityEngine;

public class FeatureFlagController : IFeatureFlagController
{
    internal FeatureFlagBridge featureFlagBridgeComponent;
    internal GameObject bridgeGameObject;

    public FeatureFlagController(GameObject bridgeGameObject = null)
    {
        if ( bridgeGameObject == null )
        {
            if (SceneReferences.i != null)
                bridgeGameObject = SceneReferences.i.bridgeGameObject;
        }

        this.bridgeGameObject = bridgeGameObject;

        if (bridgeGameObject == null)
            return;

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
        Object.Destroy(bridgeGameObject);
        bridgeGameObject = null;
        featureFlagBridgeComponent = null;
    }

    public void Initialize()
    {
    }
}