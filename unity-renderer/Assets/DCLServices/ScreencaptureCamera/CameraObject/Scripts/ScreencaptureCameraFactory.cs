using Cinemachine;
using DCL;
using DCLFeatures.ScreencaptureCamera.UI;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraFactory
    {
        public virtual ScreenRecorder CreateScreenRecorder(RectTransform canvasRectTransform) =>
            new ScreenRecorder(canvasRectTransform);

        public virtual PlayerName CreatePlayerNameUI(PlayerName playerNamePrefab, float minPlayerNameHeight, DataStore_Player player, PlayerAvatarController playerAvatar)
        {
            PlayerName playerName = Object.Instantiate(playerNamePrefab, playerAvatar.transform);
            UserProfile userProfile = UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id);
            playerName.SetName(userProfile.userName, userProfile.hasClaimedName, userProfile.isGuest);

            float height = playerAvatar.Avatar.extents.y - 0.85f;
            playerName.SetYOffset(Mathf.Max(minPlayerNameHeight, height));

            return playerName;
        }

        public virtual (ScreencaptureCameraHUDController, ScreencaptureCameraHUDView) CreateHUD(ScreencaptureCameraBehaviour mainBehaviour, ScreencaptureCameraHUDView viewPrefab)
        {
            ScreencaptureCameraHUDView screencaptureCameraHUDView = Object.Instantiate(viewPrefab);

            var screencaptureCameraHUDController = new ScreencaptureCameraHUDController(screencaptureCameraHUDView, mainBehaviour, DataStore.i, HUDController.i);
            screencaptureCameraHUDController.Initialize();

            return (screencaptureCameraHUDController, screencaptureCameraHUDView);
        }

        public virtual Camera CreateScreencaptureCamera(Camera cameraPrefab, Transform characterCameraTransform, Transform parent, int layer, CharacterController cameraTarget,
            CinemachineVirtualCamera virtualCamera)
        {
            Camera screenshotCamera = Object.Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation, parent);
            screenshotCamera.gameObject.layer = layer;

            ScreencaptureCameraMovement cameraMovement = screenshotCamera.GetComponent<ScreencaptureCameraMovement>();
            cameraMovement.Initialize(cameraTarget, virtualCamera, characterCameraTransform);
            cameraMovement.enabled = true;

            return screenshotCamera;
        }
    }
}
