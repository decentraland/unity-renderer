using Cinemachine;
using DCL.Camera;
using UnityEngine;

namespace DCL
{
    public class PlayerReferences : MonoBehaviour
    {
        public InputController inputController;
        public GameObject cursorCanvas;
        public PlayerAvatarController avatarController;
        public CameraController cameraController;
        public UnityEngine.Camera mainCamera;
        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
    }
}