using System;
using UnityEngine;

/// <summary>
/// Determine the player's camera mode
/// </summary>
[CreateAssetMenu(fileName = "CameraMode", menuName = "CameraMode")]
public class CameraMode : BaseVariableAsset<CameraMode.ModeId>
{
    [Serializable]
    public enum ModeId
    {
        FirstPerson,
        ThirdPerson,
        BuildingToolGodMode
    }

}
