using System;
using UnityEngine;

/// <summary>
/// This bridge is used for toggling the current RenderProfile from kernel
/// </summary>
[Obsolete]
public class RenderProfileBridge : MonoBehaviour
{
    public enum ID
    {
        DEFAULT,
        HALLOWEEN,
        XMAS,
        NIGHT
    }

    /// <summary>
    /// Called from kernel. Toggles the current WorldRenderProfile used by explorer.
    /// </summary>
    /// <param name="json">Model in json format</param>
    [Obsolete]
    public void SetRenderProfile(string json)
    {
        Debug.LogWarning("Deprecated due to procedural skybox");
    }

    /// <summary>
    /// Toggles the current WorldRenderProfile used by explorer.
    /// </summary>
    /// <param name="id">Which profile by id</param>
    [Obsolete]
    public void SetRenderProfile(ID id)
    {
        Debug.LogWarning("Deprecated due to procedural skybox");
    }
}