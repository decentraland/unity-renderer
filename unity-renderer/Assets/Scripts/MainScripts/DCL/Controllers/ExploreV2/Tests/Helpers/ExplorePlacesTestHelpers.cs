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
            placeInfo = new PlaceInfo()
            {
                base_position = "10,10",
                title = name,
                owner = "Test Author",
                description = "Test Description"
            },
            numberOfUsers = 10,
            parcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            placeAuthor = "Test Author",
            placeDescription = "Test Description",
            placeName = name,
            placePictureSprite = sprite
        };
    }

    public static List<PlaceInfo> CreateTestPlacesFromApi(int numberOfPlaces)
    {
        List<PlaceInfo> testPlaces = new List<PlaceInfo>();

        for (int i = 0; i < numberOfPlaces; i++)
        {
            testPlaces.Add(CreateTestHotSceneInfo((i + 1).ToString()));
        }

        return testPlaces;
    }

    public static PlaceInfo CreateTestHotSceneInfo(string id)
    {
        return new PlaceInfo
        {
            id = id,
            base_position = "10,10",
            owner = "Test Creator",
            description = "Test Description",
            title = "Test Name",
            Positions = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            realms_detail = new PlaceInfo.Realm[]
            {
                new PlaceInfo.Realm
                {
                    layer = "Test Layer",
                    maxUsers = 500,
                    serverName = "Test Server",
                    userParcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
                    usersCount = 50
                }
            },
            image = "Test Thumbnail",
            user_count = 50
        };
    }
}
