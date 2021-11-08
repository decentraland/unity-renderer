using System.Data.Common;
using DCL.Camera;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class InitialSceneReferences : MonoBehaviour, IInitialSceneReferences
    {
        public class Data
        {
            public MouseCatcher mouseCatcher;
            public GameObject groundVisual;
            public GameObject cameraParent;
            public InputController inputController;
            public GameObject cursorCanvas;
            public BuilderInWorldBridge builderInWorldBridge;
            public PlayerAvatarController playerAvatarController;
            public CameraController cameraController;
            public UnityEngine.Camera mainCamera;
            public GameObject bridgeGameObject;

            public Data () { }

            public Data(InitialSceneReferences component)
            {
                cameraController = component.cameraControllerReference;
                cameraParent = component.cameraParentReference;
                cursorCanvas = component.cursorCanvasReference;
                groundVisual = component.groundVisualReference;
                inputController = component.inputControllerReference;
                mainCamera = component.mainCameraReference;
                builderInWorldBridge = component.builderInWorldBridgeReference;
                playerAvatarController = component.playerAvatarControllerReference;
                mouseCatcher = component.mouseCatcherReference;
            }
        }

        public Data data;

        [SerializeField] internal MouseCatcher mouseCatcherReference;
        [SerializeField] internal GameObject groundVisualReference;
        [SerializeField] internal GameObject cameraParentReference;
        [SerializeField] internal InputController inputControllerReference;
        [SerializeField] internal GameObject cursorCanvasReference;
        [SerializeField] internal BuilderInWorldBridge builderInWorldBridgeReference;
        [SerializeField] internal PlayerAvatarController playerAvatarControllerReference;
        [SerializeField] internal CameraController cameraControllerReference;
        [SerializeField] internal UnityEngine.Camera mainCameraReference;
        [SerializeField] internal GameObject bridgeGameObjectReference;

        public GameObject groundVisual => data.groundVisual;
        public GameObject cameraParent => data.cameraParent;
        public GameObject cursorCanvas => data.cursorCanvas;
        public GameObject mouseCatcher => data.mouseCatcher.gameObject;
        public GameObject inputController => data.inputController.gameObject;
        public GameObject builderInWorldBridge => data.builderInWorldBridge.gameObject;
        public GameObject playerAvatarController => data.playerAvatarController.gameObject;
        public GameObject cameraController => data.cameraController.gameObject;
        public UnityEngine.Camera mainCamera { get { return mainCameraReference; } }
        public GameObject bridgeGameObject { get { return bridgeGameObjectReference; } }
        public void Dispose() { }

        public static InitialSceneReferences i { get; private set; }

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;
            data = new Data(this);
        }

        void OnDestroy() { i = null; }
    }
}