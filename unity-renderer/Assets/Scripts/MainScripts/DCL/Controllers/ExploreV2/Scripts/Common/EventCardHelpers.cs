using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventCardHelpers
{
    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";

    public static EventCardComponentView ConfigureEventCardModal(EventCardComponentView eventCardModalPrefab)
    {
        EventCardComponentView eventModal = null;

        GameObject existingModal = GameObject.Find(EVENT_CARD_MODAL_ID);
        if (existingModal != null)
            eventModal = existingModal.GetComponent<EventCardComponentView>();
        else
        {
            eventModal = GameObject.Instantiate(eventCardModalPrefab);
            eventModal.name = EVENT_CARD_MODAL_ID;
        }

        eventModal.Hide(true);

        return eventModal;
    }

    public static void ConfigureEventCardsPool(out Pool pool, string poolName, EventCardComponentView eventCardPrefab, int maxPrewarmCount)
    {
        pool = PoolManager.i.GetPool(poolName);
        if (pool == null)
        {
            pool = PoolManager.i.AddPool(
                poolName,
                GameObject.Instantiate(eventCardPrefab).gameObject,
                maxPrewarmCount: maxPrewarmCount,
                isPersistent: true);
        }
    }

    public static List<BaseComponentView> InstantiateAndConfigureEventCards(
        List<EventCardComponentModel> events,
        Pool pool,
        Action<EventCardComponentModel> OnEventInfoClicked,
        Action<EventFromAPIModel> OnEventJumpInClicked,
        Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
        {
            EventCardComponentView eventGO = pool.Get().gameObject.GetComponent<EventCardComponentView>();
            ConfigureEventCard(eventGO, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);
            instantiatedPlaces.Add(eventGO);
        }

        return instantiatedPlaces;
    }

    public static void ConfigureEventCard(
        EventCardComponentView eventCard,
        EventCardComponentModel eventInfo,
        Action<EventCardComponentModel> OnEventInfoClicked,
        Action<EventFromAPIModel> OnEventJumpInClicked,
        Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked)
    {
        eventCard.Configure(eventInfo);
        eventCard.onInfoClick?.RemoveAllListeners();
        eventCard.onInfoClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
        eventCard.onJumpInClick?.RemoveAllListeners();
        eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onSubscribeClick?.RemoveAllListeners();
        eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
        eventCard.onUnsubscribeClick?.RemoveAllListeners();
        eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));
    }
}