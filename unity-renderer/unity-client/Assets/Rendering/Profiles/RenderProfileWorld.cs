using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// RenderProfileWorld allows us to toggle between several global rendering configuration presets.
/// This is useful for events, setting day/night cycles, lerping between any of those, etc.
///
/// All the presets are stored in the RenderProfileManifest object.
/// </summary>
[CreateAssetMenu(menuName = "DCL/Rendering/Create World Profile", fileName = "RenderProfileWorld", order = 0)]
public class RenderProfileWorld : ScriptableObject
{
    [Header("Loading Blocker")] public GameObject loadingBlockerPrefab;

    [Header("Ambient And Reflection")] [SerializeField]
    private Material skyboxMaterial;

    [SerializeField] private Cubemap reflectionCubemap;
    [SerializeField] private Color skyColor;
    [SerializeField] private Color equatorColor;
    [SerializeField] private Color groundColor;
    [SerializeField] private Color fogColor;

    [Header("Directional Light")] [SerializeField]
    private Color directionalColorLight;

    [SerializeField] private Vector3 directionalColorAngle;

    [Header("Misc")] public RenderProfileAvatar avatarProfile;

#if UNITY_EDITOR
    public bool fillWithRenderSettings;
    public bool copyToRenderSettings;

    //NOTE(Brian): Workaround to make an editor-time action.
    //             ContextMenu doesn't seem to work with ScriptableObjects.
    private void OnValidate()
    {
        if (fillWithRenderSettings)
        {
            FillWithRenderSettings();
        }

        if (copyToRenderSettings)
        {
            Apply();
        }

        fillWithRenderSettings = false;
        copyToRenderSettings = false;

        if (RenderProfileManifest.i.currentProfile == this)
        {
            Apply();
        }
    }

    public void FillWithRenderSettings()
    {
        skyboxMaterial = RenderSettings.skybox;
        equatorColor = RenderSettings.ambientEquatorColor;
        skyColor = RenderSettings.ambientSkyColor;
        groundColor = RenderSettings.ambientGroundColor;
        fogColor = RenderSettings.fogColor;

        if (RenderSettings.sun != null)
        {
            directionalColorLight = RenderSettings.sun.color;
            directionalColorAngle = RenderSettings.sun.transform.rotation.eulerAngles;
        }

        reflectionCubemap = RenderSettings.customReflection;
    }
#endif

    public void Apply(bool verbose = false)
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;

        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientGroundColor = groundColor;

        RenderSettings.fogColor = fogColor;

        if (RenderSettings.sun != null)
        {
            RenderSettings.sun.color = directionalColorLight;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(directionalColorAngle);
        }

        RenderSettings.customReflection = reflectionCubemap;

        avatarProfile.Apply();

        if (verbose)
            Debug.Log("Applying profile... " + name);
    }
}