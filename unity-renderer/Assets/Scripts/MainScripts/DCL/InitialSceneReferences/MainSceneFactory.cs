using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public static class MainSceneFactory
    {
        public static List<GameObject> CreatePlayerSystems()
        {
            List<GameObject> result = new List<GameObject>();
            GameObject playerGo = LoadAndInstantiate("Player");
            var playerReferences = playerGo.GetComponent<PlayerReferences>();
            SceneReferences.i.playerAvatarController = playerReferences.avatarController;
            SceneReferences.i.biwCameraParent = playerReferences.biwCameraRoot;
            SceneReferences.i.inputController = playerReferences.inputController;
            SceneReferences.i.cursorCanvas = playerReferences.cursorCanvas;
            SceneReferences.i.cameraController = playerReferences.cameraController;
            SceneReferences.i.mainCamera = playerReferences.mainCamera;
            SceneReferences.i.thirdPersonCamera = playerReferences.thirdPersonCamera;
            SceneReferences.i.firstPersonCamera = playerReferences.firstPersonCamera;

            // Why we don't just add the playerGo?
            // This happens because the characterController reparents itself.
            // (and any of our systems may do this as well).
            result.Add( playerReferences.cursorCanvas.gameObject );
            result.Add( playerReferences.cameraController.gameObject );
            result.Add( playerReferences.inputController.gameObject );
            result.Add( playerReferences.avatarController.gameObject );
            result.Add( playerGo );

            return result;
        }

        public static GameObject CreateMouseCatcher()
        {
            GameObject result = LoadAndInstantiate("MouseCatcher");
            MouseCatcher mouseCatcher = result.GetComponent<MouseCatcher>();
            SceneReferences.i.mouseCatcher = mouseCatcher;
            return result;
        }

        public static GameObject CreateHudController() => LoadAndInstantiate("HUDController");

        public static GameObject CreateAudioHandler() => LoadAndInstantiate("HUDAudioHandler");

        public static GameObject CreateNavMap() => LoadAndInstantiate("NavMap");

        public static GameObject CreateSettingsController() => LoadAndInstantiate("SettingsController");

        public static GameObject CreateEnvironment()
        {
            GameObject result = LoadAndInstantiate("Environment");
            var env = result.GetComponent<EnvironmentReferences>();
            SceneReferences.i.environmentLight = env.environmentLight;
            SceneReferences.i.postProcessVolume = env.postProcessVolume;
            SceneReferences.i.groundVisual = env.ground;
            return result;
        }

        public static GameObject CreateBridges()
        {
            var bridges = LoadAndInstantiate("Bridges");
            SceneReferences.i.bridgeGameObject = bridges;
            return bridges;
        }

        public static GameObject CreateEventSystem() => LoadAndInstantiate("EventSystem");

        public static GameObject CreateInteractionHoverCanvas() => LoadAndInstantiate("InteractionHoverCanvas");

        private static GameObject LoadAndInstantiate(string name)
        {
            GameObject instance = UnityEngine.Object.Instantiate(Resources.Load(name)) as GameObject;
            instance.name = name;
            return instance;
        }
    }
}