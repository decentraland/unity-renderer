using MainScripts.DCL.Controllers.HotScenes;
using System.Collections.Generic;
using UnityEngine;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;

public static class ExplorePlacesTestHelpers
{
    public static List<PlaceCardComponentModel> CreateTestPlaces(Sprite sprite, int amount = 2)
    {
        List<PlaceCardComponentModel> testPlaces = new List<PlaceCardComponentModel>();

        for (int j = 0; j < amount; j++)
            testPlaces.Add(CreateTestPlace($"Test Place {j + 1}", sprite));

        return testPlaces;
    }

    public static PlaceCardComponentModel CreateTestPlace(string name, Sprite sprite)
    {
        return new PlaceCardComponentModel
        {
            coords = new Vector2Int(10, 10),
            hotSceneInfo = new HotSceneInfo(),
            numberOfUsers = 10,
            parcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            placeAuthor = "Test Author",
            placeDescription = "Test Description",
            placeName = name,
            placePictureSprite = sprite
        };
    }

    public static List<HotSceneInfo> CreateTestPlacesFromApi(int numberOfPlaces)
    {
        List<HotSceneInfo> testPlaces = new List<HotSceneInfo>();

        for (int i = 0; i < numberOfPlaces; i++)
        {
            testPlaces.Add(CreateTestHotSceneInfo((i + 1).ToString()));
        }

        return testPlaces;
    }

    public static HotSceneInfo CreateTestHotSceneInfo(string id)
    {
        return new HotSceneInfo
        {
            id = id,
            baseCoords = new Vector2Int(10, 10),
            creator = "Test Creator",
            description = "Test Description",
            name = "Test Name",
            parcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            realms = new HotSceneInfo.Realm[]
            {
                new HotSceneInfo.Realm
                {
                    layer = "Test Layer",
                    maxUsers = 500,
                    serverName = "Test Server",
                    userParcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
                    usersCount = 50
                }
            },
            thumbnail = "Test Thumbnail",
            usersTotalCount = 50
        };
    }
}
