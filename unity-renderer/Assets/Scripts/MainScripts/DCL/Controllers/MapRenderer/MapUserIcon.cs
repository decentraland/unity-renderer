using DCL.Helpers;
using UnityEngine;

public class MapUserIcon : MonoBehaviour
{
    private Player trackedPlayer;

    public void Populate(Player status) { trackedPlayer = status; }

    private void LateUpdate()
    {
        if (trackedPlayer == null)
            return;

        var gridPosition = Utils.WorldToGridPositionUnclamped(trackedPlayer.worldPosition + CommonScriptableObjects.worldOffset.Get());
        transform.localPosition = MapUtils.CoordsToPositionUnclamped(gridPosition);
    }
}