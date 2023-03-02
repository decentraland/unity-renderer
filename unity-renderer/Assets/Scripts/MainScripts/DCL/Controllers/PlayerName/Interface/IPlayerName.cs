using UnityEngine;

public interface IPlayerName
{
    /// <summary>
    /// If you add a constraint to the visibility it won't renderer until you remove the constraint
    /// </summary>
    /// <param name="constraint"></param>
    void AddVisibilityConstaint(string constraint);

    /// <summary>
    /// Remove an added constraint to the visibility, if no constraint are shown it will take the last know value
    /// </summary>
    /// <param name="constraint"></param>
    void RemoveVisibilityConstaint(string constraint);

    void SetName(string name, bool isClaimed, bool isGuest);
    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetForceShow(bool forceShow);
    void SetIsTalking(bool talking);
    void SetYOffset(float yOffset);
    Rect ScreenSpaceRect(Camera mainCamera);
    Vector3 ScreenSpacePos(Camera mainCamera);
}
