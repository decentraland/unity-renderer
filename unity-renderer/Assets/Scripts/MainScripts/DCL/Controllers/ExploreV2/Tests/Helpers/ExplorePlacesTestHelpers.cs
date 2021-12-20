using System.Collections.Generic;
using UnityEngine;
using static HotScenesController;

public static class ExplorePlacesTestHelpers
{
    public static List<PlaceCardComponentModel> CreateTestPlaces(Sprite sprite)
    {
        List<PlaceCardComponentModel> testPlaces = new List<PlaceCardComponentModel>();
        testPlaces.Add(CreateTestPlace("Test Place 1", sprite));
        testPlaces.Add(CreateTestPlace("Test Place 2", sprite));

        return testPlaces;
    }

    public static PlaceCardComponentModel CreateTestPlace(string name, Sprite sprite)
    {
        return new PlaceCardComponentModel
        {
            coords = new Vector2Int(10, 10),
            hotSceneInfo = new HotScenesController.HotSceneInfo(),
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