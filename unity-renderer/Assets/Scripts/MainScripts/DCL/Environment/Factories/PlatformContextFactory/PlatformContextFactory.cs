using DCL.Rendering;
using UnityEngine;

namespace DCL
{
    public static class PlatformContextFactory
    {
        public static PlatformContext CreateDefault()
        {
            if (SceneReferences.i != null)
                return CreateDefault(SceneReferences.i.bridgeGameObject);

            return CreateDefault(new GameObject("Bridges"));
        }

        public static PlatformContext CreateDefault(GameObject bridgesGameObject)
        {
            return new PlatformContext(
                memoryManager: new MemoryManager(),
                cullingController: CullingController.Create(),
                clipboard: Clipboard.Create(),
                physicsSyncController: new PhysicsSyncController(),
                parcelScenesCleaner: new ParcelScenesCleaner(),
                webRequest: WebRequestController.Create(),
                serviceProviders: new ServiceProviders(),
                idleChecker: new IdleChecker(),
                avatarsLODController: new AvatarsLODController(),
                featureFlagController: new FeatureFlagController(bridgesGameObject),
                updateEventHandler: new UpdateEventHandler());
        }
    }
}