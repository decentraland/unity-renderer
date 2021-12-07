using UnityEngine;

namespace DCL
{
    public static class MainSceneFactory
    {
        // public static void CreateAllDependencies()
        // {
        //     SceneReferences.i.Initialize(
        //         mouseCatcher,
        //         environment.ground,
        //         playerReferences.biwCameraRoot,
        //         playerReferences.inputController,
        //         playerReferences.cursorCanvas,
        //         gameObject,
        //         playerReferences.avatarController,
        //         playerReferences.cameraController,
        //         playerReferences.mainCamera,
        //         bridges,
        //         environment.environmentLight,
        //         environment.postProcessVolume,
        //         playerReferences.thirdPersonCamera,
        //         playerReferences.firstPersonCamera);
        // }

        public static GameObject CreatePlayer()
        {
            GameObject result = LoadAndInstantiate("Player");
            var playerReferences = LoadAndInstantiate("Player").GetComponent<PlayerReferences>();
            SceneReferences.i.playerAvatarController = playerReferences.avatarController;
            SceneReferences.i.biwCameraParent = playerReferences.biwCameraRoot;
            SceneReferences.i.inputController = playerReferences.inputController;
            SceneReferences.i.cursorCanvas = playerReferences.cursorCanvas;
            SceneReferences.i.cameraController = playerReferences.cameraController;
            SceneReferences.i.mainCamera = playerReferences.mainCamera;
            SceneReferences.i.thirdPersonCamera = playerReferences.thirdPersonCamera;
            SceneReferences.i.firstPersonCamera = playerReferences.firstPersonCamera;
            return result;
        }

        public static GameObject CreateMouseCatcher()
        {
            GameObject result = LoadAndInstantiate("MouseCatcher");
            MouseCatcher mouseCatcher = result.GetComponent<MouseCatcher>();
            SceneReferences.i.mouseCatcher = mouseCatcher;
            return result;
        }

        public static GameObject CreateHudController()
        {
            GameObject result = LoadAndInstantiate("HUDController");
            return result;
        }

        public static GameObject CreateAudioHandler()
        {
            GameObject result = LoadAndInstantiate("HUDAudioHandler");
            return result;
        }

        public static GameObject CreateNavMap()
        {
            GameObject result = LoadAndInstantiate("NavMap");
            return result;
        }

        public static GameObject CreateSettingsController()
        {
            GameObject result = LoadAndInstantiate("SettingsController");
            return result;
        }

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

        private static GameObject LoadAndInstantiate(string name)
        {
            GameObject instance = UnityEngine.Object.Instantiate(Resources.Load(name)) as GameObject;
            instance.name = name;
            return instance;
        }
    }
}