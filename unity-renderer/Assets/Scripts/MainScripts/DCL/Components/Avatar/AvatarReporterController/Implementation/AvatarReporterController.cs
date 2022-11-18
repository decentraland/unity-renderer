using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

/// <summary>
/// This controller checks if an avatar entered or exited a scene by checking its position every frame
/// </summary>
public class AvatarReporterController : IAvatarReporterController
{
    private string avatarId;
    private int lastSceneNumber;
    private Vector2Int lastCoords;
    private Vector3 lastPositionChecked;
    private bool isInitialReport = true;

    private readonly IWorldState worldState;

    IReporter IAvatarReporterController.reporter { get; set; } = new Reporter();

    public AvatarReporterController(IWorldState worldState)
    {
        this.worldState = worldState;
    }

    void IAvatarReporterController.SetUp(int sceneNumber, string avatarId)
    {
        // NOTE: do not report avatars that doesn't belong to the global scene
        if (sceneNumber != EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER)
            return;
        
        this.avatarId = avatarId;
        isInitialReport = true;
        lastSceneNumber = -1;
    }
    
    int GetcurrentSceneNumberNonAlloc(Vector2Int coords)
    {
        return worldState.GetSceneNumberByCoords(coords);
    }

    void IAvatarReporterController.ReportAvatarPosition(Vector3 position)
    {
        if (!CanReport())
            return;

        bool wasInLoadedScene = WasInLoadedScene();

        if (wasInLoadedScene && !HasMoved(position))
            return;

        Vector2Int coords = Utils.WorldToGridPosition(CommonScriptableObjects.worldOffset + position);

        if (wasInLoadedScene && lastCoords == coords)
        {
            return;
        }
        
        int currentSceneNumber = GetcurrentSceneNumberNonAlloc(coords);

        if (currentSceneNumber == lastSceneNumber && !isInitialReport)
        {
            return;
        }

        ((IAvatarReporterController)this).reporter.ReportAvatarSceneChange(avatarId, currentSceneNumber);

        lastSceneNumber = currentSceneNumber;
        lastCoords = coords;
        lastPositionChecked = position;
        isInitialReport = false;
    }

    void IAvatarReporterController.ReportAvatarRemoved()
    {
        if (!CanReport())
            return;

        ((IAvatarReporterController)this).reporter.ReportAvatarRemoved(avatarId);
        
        avatarId = null;
        lastSceneNumber = -1;
        isInitialReport = true;
    }

    private bool CanReport()
    {
        return !string.IsNullOrEmpty(avatarId);
    }

    private bool HasMoved(Vector3 currentPosition)
    {
        return Vector3.SqrMagnitude(currentPosition - lastPositionChecked) > 0.0001f;
    }

    private bool WasInLoadedScene()
    {
        return lastSceneNumber > 0;
    }
}