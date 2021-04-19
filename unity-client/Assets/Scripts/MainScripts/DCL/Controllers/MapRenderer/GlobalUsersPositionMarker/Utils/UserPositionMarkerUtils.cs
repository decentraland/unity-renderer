using System.Collections.Generic;
using UnityEngine;

internal class ExclusionArea
{
    public Vector2Int position;
    public int area;

    public bool Contains(Vector2Int coords)
    {
        return (coords - position).sqrMagnitude <= area * area;
    }
}

internal class ParcelData
{
    public Vector2Int coords { private set; get; }
    public string realmServer { private set; get; }
    public string realmLayer { private set; get; }

    public ParcelData(Vector2Int coords, string realmServer, string realmLayer)
    {
        this.coords = coords;
        this.realmServer = realmServer;
        this.realmLayer = realmLayer;
    }
}

internal class ScenesFilter
{
    public List<ParcelData> Filter(List<HotScenesController.HotSceneInfo> hotScenesList, int maxMarkers)
    {
        List<ParcelData> result = new List<ParcelData>(maxMarkers);
        List<ParcelData> rawParcelCoords = GetRawParcelCoords(hotScenesList);
        float stepAmount = rawParcelCoords.Count / (float)maxMarkers;
        if (stepAmount < 1) stepAmount = 1;

        float lastIndex = -1;
        for (float step = 0; step < rawParcelCoords.Count; step += stepAmount)
        {
            if ((step - lastIndex) >= 1)
            {
                lastIndex = step;
                result.Add(rawParcelCoords[(int)lastIndex]);

                if (result.Count >= maxMarkers)
                    break;
            }
        }

        return result;
    }

    private List<ParcelData> GetRawParcelCoords(List<HotScenesController.HotSceneInfo> hotScenesList)
    {
        List<ParcelData> result = new List<ParcelData>();

        HotScenesController.HotSceneInfo sceneInfo;
        HotScenesController.HotSceneInfo.Realm realm;
        int scenesCount = hotScenesList.Count;

        for (int sceneIdx = 0; sceneIdx < scenesCount; sceneIdx++)
        {
            sceneInfo = hotScenesList[sceneIdx];
            if (sceneInfo.usersTotalCount <= 0) continue;

            for (int realmIdx = 0; realmIdx < sceneInfo.realms.Length; realmIdx++)
            {
                realm = sceneInfo.realms[realmIdx];
                for (int parcelIdx = 0; parcelIdx < realm.userParcels.Length; parcelIdx++)
                {
                    result.Add(new ParcelData(realm.userParcels[parcelIdx], realm.serverName, realm.layer));
                }
            }
        }
        return result;
    }
}