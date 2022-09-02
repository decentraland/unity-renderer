using DCL.Helpers;
using DCL;
using UnityEngine;
using System;

public class MinimapMetadataController : MonoBehaviour
{
    private MinimapMetadata minimapMetadata => MinimapMetadata.GetMetadata();
    public static MinimapMetadataController i { get; private set; }
    public Action<Vector2Int> OnHomeChanged;
    private BaseVariable<Vector2Int> homePoint => DataStore.i.HUDs.homePoint;

    public void Awake()
    {
        i = this;
        minimapMetadata.Clear();
    }

    public void UpdateHomeScene(string sceneCoordinates)
    {
        if (sceneCoordinates == null)
            return;

        homePoint.Set(new Vector2Int(Int32.Parse(sceneCoordinates.Split(',')[0]), Int32.Parse(sceneCoordinates.Split(',')[1])));
        OnHomeChanged?.Invoke(homePoint.Get());
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