using MainScripts.DCL.Controllers.HotScenes;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SearchSubSectionComponentViewShould
{
    private SearchSubSectionComponentView view;

    [SetUp]
    public void SetUp()
    {
        view = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesSubSection/SearchSubSection")).GetComponent<SearchSubSectionComponentView>();
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
        Object.Destroy(view.gameObject);
    }

    [Test]
    public void ShowPlaces()
    {
        view.ShowPlaces(new List<PlaceCardComponentModel>()
        {
            new ()
            {
                placeName = "TestPlace1",
                isFavorite = false,
                numberOfUsers = 3,
                placeAuthor = "test auth",
                placeDescription = "test description",
                coords = new Vector2Int(10,3),
                parcels = new []{new Vector2Int(10,3)},
                placePictureUri = "www.test.com",
                placeInfo = new IHotScenesController.PlaceInfo()
                {
                    id = "id1"
                }
            },
            new ()
            {
                placeName = "TestPlace2",
                isFavorite = true,
                numberOfUsers = 0,
                placeAuthor = "test auth",
                placeDescription = "test description",
                coords = new Vector2Int(11,10),
                parcels = new []{new Vector2Int(11,10)},
                placePictureUri = "www.test.com",
                placeInfo = new IHotScenesController.PlaceInfo()
                {
                    id = "id2"
                }
            }
        }, "test");

        Assert.AreEqual(2, view.pooledPlaces.Count, "places count does not match");
    }

    [Test]
    public void ShowEmptyPlaces()
    {
        view.ShowPlaces(new List<PlaceCardComponentModel>(), "test");

        Assert.AreEqual(0, view.pooledPlaces.Count, "places count does not match");
        Assert.True(view.noPlaces.activeSelf, "no places gameobject should be active");
    }

    [Test]
    public void ShowEmptyEvents()
    {
        view.ShowEvents(new List<EventCardComponentModel>(), "test");

        Assert.AreEqual(0, view.pooledEvents.Count, "places count does not match");
        Assert.True(view.noEvents.activeSelf, "no places gameobject should be active");
    }
}
