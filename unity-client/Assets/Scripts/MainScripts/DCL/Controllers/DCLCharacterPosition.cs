using DCL.Configuration;
using System;
using UnityEngine;

public class DCLCharacterPosition
{
    public Action<DCLCharacterPosition> OnPrecisionAdjust;

    private Vector3 worldPositionValue;
    private Vector3 unityPositionValue;
    private Vector3 offset;

    private int lastRepositionFrame = 0;

    public Vector3 UnityToWorldPosition(Vector3 pos)
    {

        return pos + offset;

    }

    public Vector3 WorldToUnityPosition(Vector3 pos)
    {
        return pos - offset;
    }

    public Vector3 worldPosition
    {
        get
        {
            return worldPositionValue;
        }

        set
        {
            worldPositionValue = value;
            unityPositionValue = WorldToUnityPosition(worldPositionValue);
            CheckAndTeleport();
        }
    }

    public Vector3 unityPosition
    {
        get
        {
            return unityPositionValue;
        }

        set
        {
            unityPositionValue = value;
            worldPositionValue = UnityToWorldPosition(unityPositionValue);
            CheckAndTeleport();
        }
    }

    public DCLCharacterPosition()
    {
        CommonScriptableObjects.playerUnityToWorldOffset.Set(Vector3.zero);
        CommonScriptableObjects.playerWorldPosition.Set(Vector3.zero);
    }

    private void CheckAndTeleport()
    {
        bool dirty = false;
        float minDistanceForReposition = PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE;

        if (Mathf.Abs(unityPositionValue.x) > minDistanceForReposition)
        {
            float dist = (int)(unityPositionValue.x / minDistanceForReposition) * minDistanceForReposition;
            unityPositionValue.x -= dist;
            offset.x += dist;
            dirty = true;
        }

        if (Mathf.Abs(unityPositionValue.z) > minDistanceForReposition)
        {
            float dist = (int)(unityPositionValue.z / minDistanceForReposition) * minDistanceForReposition;
            unityPositionValue.z -= dist;
            offset.z += dist;
            dirty = true;
        }

        if (dirty)
        {
            worldPositionValue = unityPositionValue + offset;

            lastRepositionFrame = Time.frameCount;

            OnPrecisionAdjust?.Invoke(this);

            CommonScriptableObjects.playerWorldPosition.Set(worldPositionValue);
            CommonScriptableObjects.playerUnityToWorldOffset.Set(offset);
        }
    }

    public bool RepositionedWorldLastFrame()
    {
        return lastRepositionFrame == Time.frameCount - 1;
    }

    public override string ToString()
    {
        return $"worldPos: {worldPositionValue} - unityPos: {unityPositionValue} - offset: {offset}";
    }
}
