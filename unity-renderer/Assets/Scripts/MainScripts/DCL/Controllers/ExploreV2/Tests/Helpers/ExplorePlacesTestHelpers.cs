using System.Collections.Generic;
using UnityEngine;
using static HotScenesController;

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
            hotSceneInfo = new HotScenesController.HotSceneInfo(),
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
            base_position = new Vector2Int(10, 10),
            owner = "Test Creator",
            description = "Test Description",
            title = "Test Name",
            positions = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            image = "Test Thumbnail",
            user_count = 50
        };
    }
}
