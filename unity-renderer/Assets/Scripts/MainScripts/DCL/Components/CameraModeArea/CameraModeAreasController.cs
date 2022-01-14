using System.Collections.Generic;

namespace DCL.Components
{
    internal class CameraModeAreasController
    {
        private CameraMode.ModeId initialCameraMode;
        private readonly List<ICameraModeArea> insideAreasList = new List<ICameraModeArea>();

        public void AddInsideArea(in ICameraModeArea area)
        {
            if (insideAreasList.Count == 0)
            {
                initialCameraMode = CommonScriptableObjects.cameraMode.Get();
                CommonScriptableObjects.cameraModeInputLocked.Set(true);
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
    }
}