using UnityEngine;

public interface IPlayerName
{
    void SetName(string name);
    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetForceShow(bool forceShow);
    void SetIsTalking(bool talking);
    Rect ScreenSpaceRect(Camera mainCamera);
    Vector3 ScreenSpacePos(Camera mainCamera);
}