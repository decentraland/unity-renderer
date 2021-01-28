using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

/// <summary>
/// This bridge is used for toggling the current RenderProfile from kernel
/// </summary>
public class RenderProfileBridge : MonoBehaviour
{
    public enum ID
    {
        DEFAULT,
        HALLOWEEN,
        XMAS,
        NIGHT
    }

    [System.Serializable]
    public class Model
    {
        public ID id;
    }

    public static RenderProfileBridge i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    /// <summary>
    /// Called from kernel. Toggles the current WorldRenderProfile used by explorer.
    /// </summary>
    /// <param name="json">Model in json format</param>
    public void SetRenderProfile(string json)
    {
        ID id = JsonUtility.FromJson<Model>(json).id;
        SetRenderProfile(id);
    }

    /// <summary>
    /// Toggles the current WorldRenderProfile used by explorer.
    /// </summary>
    /// <param name="id">Which profile by id</param>
    public void SetRenderProfile(ID id)
    {
        RenderProfileWorld newProfile;
        switch (id)
        {
            default:
                newProfile = RenderProfileManifest.i.defaultProfile;
                break;
            case ID.HALLOWEEN:
            case ID.XMAS:
            case ID.NIGHT:
                newProfile = RenderProfileManifest.i.nightProfile;
                break;
        }

        RenderProfileManifest.i.currentProfile = newProfile;
        RenderProfileManifest.i.currentProfile.Apply();
    }
}