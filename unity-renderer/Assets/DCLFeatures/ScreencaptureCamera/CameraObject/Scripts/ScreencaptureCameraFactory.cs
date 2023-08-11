using Cinemachine;
using DCL;
using DCLFeatures.ScreencaptureCamera.UI;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraFactory
    {
        public PlayerName CreatePlayerNameUI(PlayerName playerNamePrefab, float minPlayerNameHeight, UserProfile userProfile, PlayerAvatarController playerAvatar)
        {
            var playerName = Object.Instantiate(playerNamePrefab, playerAvatar.transform);
            playerName.SetName(userProfile.userName, userProfile.hasClaimedName, userProfile.isGuest);

            float height = playerAvatar.Avatar.extents.y - 0.85f;
            playerName.SetYOffset(Mathf.Max(minPlayerNameHeight, height));

            return playerName;
        }

        public (ScreencaptureCameraHUDController, ScreencaptureCameraHUDView) CreateHUD(ScreencaptureCameraBehaviour mainBehaviour, ScreencaptureCameraHUDView viewPrefab, ScreencaptureCameraInputSchema inputActionsSchema)
        {
            var screencaptureCameraHUDView = Object.Instantiate(viewPrefab);

            var screencaptureCameraHUDController = new ScreencaptureCameraHUDController(screencaptureCameraHUDView,
                mainBehaviour, inputActionsSchema, DataStore.i);

             screencaptureCameraHUDController.Initialize();

             return (screencaptureCameraHUDController, screencaptureCameraHUDView);
        }

        public Camera CreateScreencaptureCamera(Camera cameraPrefab, Transform characterCameraTransform, Transform parent, int layer, CharacterController cameraTarget, CinemachineVirtualCamera virtualCamera)
        {
            var screenshotCamera =  Object.Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation, parent);
            screenshotCamera.gameObject.layer = layer;

            ScreencaptureCameraMovement cameraMovement = screenshotCamera.GetComponent<ScreencaptureCameraMovement>();
            cameraMovement.Initialize(cameraTarget, virtualCamera, characterCameraTransform);
            cameraMovement.enabled = true;

            return screenshotCamera;
        }
    }
}
