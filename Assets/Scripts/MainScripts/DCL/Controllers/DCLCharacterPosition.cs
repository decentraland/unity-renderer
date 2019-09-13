using UnityEngine;
using System;

public class DCLCharacterPosition
{
    public const float LIMIT = 100.0f;

    public Action<DCLCharacterPosition> OnPrecisionAdjust;

    private Vector3 worldPositionValue;
    private Vector3 unityPositionValue;
    private Vector3 offset;

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
    }

    private void CheckAndTeleport()
    {
        bool dirty = false;

        if (Mathf.Abs(unityPositionValue.x) > LIMIT)
        {
            float dist = (int)(unityPositionValue.x / LIMIT) * LIMIT;
            unityPositionValue.x -= dist;
            offset.x += dist;
            dirty = true;
        }

        if (Mathf.Abs(unityPositionValue.z) > LIMIT)
        {
            float dist = (int)(unityPositionValue.z / LIMIT) * LIMIT;
            unityPositionValue.z -= dist;
            offset.z += dist;
            dirty = true;
        }

        if (dirty)
        {
            worldPositionValue = unityPositionValue + offset;

            OnPrecisionAdjust?.Invoke(this);

            CommonScriptableObjects.playerUnityToWorldOffset.Set(offset);
        }
    }

    public override string ToString()
    {
        return $"worldPos: {worldPositionValue} - unityPos: {unityPositionValue} - offset: {offset}";
    }
}
