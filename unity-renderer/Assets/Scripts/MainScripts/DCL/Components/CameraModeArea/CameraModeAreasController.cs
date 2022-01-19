using System.Collections.Generic;
using DCL.NotificationModel;

namespace DCL.Components
{
    internal class CameraModeAreasController
    {
        private const string NOTIFICATION_GROUP = "CameraModeLockedByScene";
        private const float NOTIFICATION_TIME = 3;

        private static readonly Model notificationModel = new Model()
        {
            type = Type.CAMERA_MODE_LOCKED_BY_SCENE,
            groupID = NOTIFICATION_GROUP,
            timer = NOTIFICATION_TIME
        };

        private CameraMode.ModeId initialCameraMode;
        private readonly List<ICameraModeArea> insideAreasList = new List<ICameraModeArea>();

        public void AddInsideArea(in ICameraModeArea area)
        {
            if (!IsPlayerInsideAnyArea())
            {
                initialCameraMode = CommonScriptableObjects.cameraMode.Get();
                CommonScriptableObjects.cameraModeInputLocked.Set(true);
                ShowCameraModeLockedNotification();
            }

            CommonScriptableObjects.cameraMode.Set(area.cameraMode);
            insideAreasList.Add(area);
        }

        public void RemoveInsideArea(in ICameraModeArea area)
        {
            int affectingAreasCount = insideAreasList.Count;

            if (affectingAreasCount == 0)
            {
                return;
            }

            if (affectingAreasCount == 1)
            {
                // reset to initial state of camera mode
                ResetCameraMode();

                //remove notification
                HideCameraModeLockedNotification();
            }
            else if (IsTheActivelyAffectingArea(area))
            {
                // set camera mode to the previous area the player is in
                CommonScriptableObjects.cameraMode.Set(insideAreasList[affectingAreasCount - 2].cameraMode);
            }

            insideAreasList.Remove(area);
        }

        public void ChangeAreaMode(in ICameraModeArea area, in CameraMode.ModeId mode)
        {
            if (IsTheActivelyAffectingArea(area))
            {
                CommonScriptableObjects.cameraMode.Set(mode);
            }
        }

        private bool IsPlayerInsideAnyArea()
        {
            return insideAreasList.Count > 0;
        }

        private void ResetCameraMode()
        {
            CommonScriptableObjects.cameraMode.Set(initialCameraMode);
            CommonScriptableObjects.cameraModeInputLocked.Set(false);
        }

        private bool IsTheActivelyAffectingArea(in ICameraModeArea area)
        {
            int affectingAreasCount = insideAreasList.Count;

            if (affectingAreasCount == 0)
            {
                return false;
            }

            return insideAreasList[affectingAreasCount - 1] == area;
        }

        internal virtual void ShowCameraModeLockedNotification()
        {
            NotificationsController.i?.ShowNotification(notificationModel);
        }

        internal virtual void HideCameraModeLockedNotification()
        {
            NotificationsController.i?.DismissAllNotifications(NOTIFICATION_GROUP);
        }
    }
}