using DCLServices.WorldsAPIService;
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

    public static List<PlaceCardComponentModel> CreateTestWorlds(Sprite sprite, int amount = 2)
    {
        List<PlaceCardComponentModel> testWorlds = new List<PlaceCardComponentModel>();

        for (int j = 0; j < amount; j++)
            testWorlds.Add(CreateTestWorld($"Test world {j + 1}", sprite));

        return testWorlds;
    }

    public static PlaceCardComponentModel CreateTestPlace(string name, Sprite sprite)
    {
        return new PlaceCardComponentModel
        {
            isWorld = false,
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

    public static PlaceCardComponentModel CreateTestWorld(string name, Sprite sprite)
    {
        return new PlaceCardComponentModel
        {
            isWorld = true,
            coords = new Vector2Int(0, 0),
            placeInfo = new PlaceInfo()
            {
                base_position = "0,0",
                title = name,
                owner = "Test world Author",
                description = "Test world Description"
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

    public static List<WorldsResponse.WorldInfo> CreateTestWorldsFromApi(int numberOfWorlds)
    {
        List<WorldsResponse.WorldInfo> testWorlds = new List<WorldsResponse.WorldInfo>();

        for (int i = 0; i < numberOfWorlds; i++)
        {
            testWorlds.Add(CreateTestWorldSceneInfo((i + 1).ToString()));
        }

        return testWorlds;
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

    public static WorldsResponse.WorldInfo CreateTestWorldSceneInfo(string id)
    {
        return new WorldsResponse.WorldInfo()
        {
            id = id,
            base_position = "0,0",
            owner = "Test Creator",
            description = "Test Description",
            title = "Test world",
            Positions = new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1) },
            realms_detail = new WorldsResponse.WorldInfo.Realm[]
            {
                new WorldsResponse.WorldInfo.Realm
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
