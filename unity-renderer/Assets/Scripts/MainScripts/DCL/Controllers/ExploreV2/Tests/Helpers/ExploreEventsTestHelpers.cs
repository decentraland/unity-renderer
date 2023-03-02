using System.Collections.Generic;
using UnityEngine;

public static class ExploreEventsTestHelpers
{
    public static List<EventCardComponentModel> CreateTestEvents(Sprite sprite, int amount = 2)
    {
        List<EventCardComponentModel> testEvents = new List<EventCardComponentModel>();
        
        for (int j = 0; j < amount; j++)
            testEvents.Add(CreateTestEvent($"Test Event {j + 1}", sprite));

        return testEvents;
    }

    public static EventCardComponentModel CreateTestEvent(string id, Sprite sprite)
    {
        return new EventCardComponentModel
        {
            eventId = id,
            coords = new Vector2Int(19, 10),
            eventDateText = "Test Date",
            eventDescription = "Test Description",
            eventName = "Test Name",
            eventOrganizer = "Test Organizer",
            eventPictureSprite = sprite,
            eventPlace = "Test Place",
            eventStartedIn = "Test Date",
            eventStartsInFromTo = "Test Start",
            isLive = true,
            isSubscribed = false,
            liveTagText = "Test Live Text",
            subscribedUsers = 100,
            eventFromAPIInfo = new EventFromAPIModel
            {
                id = id,
                attending = false,
                coordinates = new int[] { 10, 10 },
                description = "Test Description",
                finish_at = "Test Date",
                highlighted = false,
                image = "Test Uri",
                live = true,
                name = "Test Name",
                next_start_at = "Test Start",
                realm = "Test Realm",
                scene_name = "Test Scene Name",
                total_attendees = 100,
                trending = false,
                user_name = "Test User Name"
            }
        };
    }

    public static List<EventFromAPIModel> CreateTestEventsFromApi(int numberOfEvents)
    {
        List<EventFromAPIModel> testEvents = new List<EventFromAPIModel>();

        for (int i = 0; i < numberOfEvents; i++)
        {
            testEvents.Add(CreateTestEvent((i + 1).ToString()));
        }

        return testEvents;
    }

    public static EventFromAPIModel CreateTestEvent(string id)
    {
        return new EventFromAPIModel
        {
            id = id,
            attending = false,
            coordinates = new int[] { 10, 10 },
            description = "Test Description",
            finish_at = "2021-11-30T11:11:00.000Z",
            highlighted = false,
            image = "Test Uri",
            live = true,
            name = "Test Name",
            next_start_at = "2021-09-30T11:11:00.000Z",
            realm = null,
            scene_name = "Test Scene Name",
            total_attendees = 100,
            trending = false,
            user_name = "Test User Name"
        };
    }
}