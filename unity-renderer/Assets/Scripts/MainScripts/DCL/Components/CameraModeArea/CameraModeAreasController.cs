using System.Collections.Generic;

namespace DCL.Components
{
    internal class CameraModeAreasController
    {
        private CameraMode.ModeId initialCameraMode;
        private readonly List<CameraModeArea> insideAreasList = new List<CameraModeArea>();

        public void AddInsideArea(CameraModeArea area)
        {
            if (insideAreasList.Count == 0)
            {
                initialCameraMode = CommonScriptableObjects.cameraMode.Get();
                CommonScriptableObjects.cameraModeInputLocked.Set(true);
            }

            CommonScriptableObjects.cameraMode.Set(area.areaModel.cameraMode);
            insideAreasList.Add(area);
        }

        public void RemoveInsideArea(CameraModeArea area)
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
            }
            else if (IsTheActivelyAffectingArea(area))
            {
                // set camera mode to the previous area the player is in
                CommonScriptableObjects.cameraMode.Set(insideAreasList[affectingAreasCount - 2].areaModel.cameraMode);
            }

            insideAreasList.Remove(area);
        }

        public void ChangeAreaMode(CameraModeArea area, CameraMode.ModeId mode)
        {
            if (IsTheActivelyAffectingArea(area))
            {
                CommonScriptableObjects.cameraMode.Set(mode);
            }
        }

        private void ResetCameraMode()
        {
            CommonScriptableObjects.cameraMode.Set(initialCameraMode);
            CommonScriptableObjects.cameraModeInputLocked.Set(false);
        }

        private bool IsTheActivelyAffectingArea(CameraModeArea area)
        {
            int affectingAreasCount = insideAreasList.Count;

            if (affectingAreasCount == 0)
            {
                return false;
            }

            return insideAreasList[affectingAreasCount - 1] == area;
        }
    }
}