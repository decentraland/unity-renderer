using DCL.Helpers;
using UnityEngine;

public class MapUserIcon : MonoBehaviour
{
    private PlayerStatus trackedPlayer;

    public void Populate(PlayerStatus status) { trackedPlayer = status; }

    private void LateUpdate()
    {
        if (trackedPlayer == null)
            return;

        var gridPosition = Utils.WorldToGridPositionUnclamped(trackedPlayer.worldPosition);
        transform.localPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
    }
}