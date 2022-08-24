using DCL.Helpers;
using UnityEngine;
using System;

public class MinimapMetadataController : MonoBehaviour
{
    private MinimapMetadata minimapMetadata => MinimapMetadata.GetMetadata();
    public static MinimapMetadataController i { get; private set; }
    public Vector2Int homeCoords;

    public void Awake()
    {
        i = this;
        minimapMetadata.Clear();
    }

    public void UpdateHomeScene(string sceneCoordinates)
    {
        Debug.Log($"Update home scene {sceneCoordinates}");
        if (sceneCoordinates == null)
            return;

        homeCoords = new Vector2Int(Int32.Parse(sceneCoordinates.Split(',')[0]), Int32.Parse(sceneCoordinates.Split(',')[1]));
    }

    public void UpdateMinimapSceneInformation(string scenesInfoJson)
    {
        var scenesInfo = Utils.ParseJsonArray<MinimapMetadata.MinimapSceneInfo[]>(scenesInfoJson);

        foreach (var sceneInfo in scenesInfo)
        {
            minimapMetadata.AddSceneInfo(sceneInfo);
        }
    }
}