using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using Google.Protobuf;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public static class UIPointerEventsUtils
    {
        internal abstract class UIEventSubscriptionFactory
        {
            internal abstract IDisposable CreateSubscription(VisualElement uiElement,
                IParcelScene parcelScene,
                IDCLEntity dclEntity,
                PointerEventType pointerEventType,
                PBPointerEvents.Types.Info requestInfo,
                IInternalECSComponent<InternalInputEventResults> results);
        }

        internal class UIEventSubscriptionFactory<TEvent> : UIEventSubscriptionFactory
            where TEvent: PointerEventBase<TEvent>, new()
        {
            internal override IDisposable CreateSubscription(
                VisualElement uiElement,
                IParcelScene parcelScene,
                IDCLEntity dclEntity,
                PointerEventType pointerEventType,
                PBPointerEvents.Types.Info requestInfo,
                IInternalECSComponent<InternalInputEventResults> results) =>
                new UIEventSubscription<TEvent>(uiElement, parcelScene, dclEntity, pointerEventType, requestInfo, results);
        }

        private static readonly Dictionary<PointerEventType, UIEventSubscriptionFactory> FACTORIES = new ()
        {
            { PointerEventType.PetDown, new UIEventSubscriptionFactory<PointerDownEvent>() },
            { PointerEventType.PetUp, new UIEventSubscriptionFactory<PointerUpEvent>() },
            { PointerEventType.PetHoverEnter, new UIEventSubscriptionFactory<PointerEnterEvent>() },
            { PointerEventType.PetHoverLeave, new UIEventSubscriptionFactory<PointerLeaveEvent>() }
        };

        [CanBeNull]
        public static UIEventsSubscriptions AddCommonInteractivity(
            VisualElement uiElement,
            IParcelScene parcelScene,
            IDCLEntity dclEntity,
            IReadOnlyList<PBPointerEvents.Types.Entry> requestedInteraction,
            IInternalECSComponent<InternalInputEventResults> results)
        {
            if (requestedInteraction is null or { Count: 0 })
                return null;

            var subscriptions = new UIEventsSubscriptions();

            foreach (var entry in requestedInteraction)
            {
                if (!FACTORIES.TryGetValue(entry.EventType, out var factory))
                    continue;

                subscriptions.list.Add(factory.CreateSubscription(uiElement, parcelScene, dclEntity,
                    entry.EventType, entry.EventInfo, results));
            }

            return subscriptions;
        }

        public static EventCallback<TEvent> RegisterFeedback<TEvent, TResultMessage>(
            IInternalECSComponent<InternalUIInputResults> inputResults,
            Func<TEvent, TResultMessage> createResult,
            IParcelScene scene,
            IDCLEntity entity,
            VisualElement uiElement,
            int resultComponentId)
            where TResultMessage : class, IMessage
            where TEvent : EventBase<TEvent>, new()
        {
            EventCallback<TEvent> callback = evt =>
            {
                var model = inputResults.GetFor(scene, entity)?.model ?? new InternalUIInputResults();
                model.Results.Enqueue(new InternalUIInputResults.Result(createResult(evt), resultComponentId));
                inputResults.PutFor(scene, entity, model);
            };

            uiElement.RegisterCallback(callback);
            return callback;
        }

        public static void UnregisterFeedback<TEvent>(
            this VisualElement uiElement,
            EventCallback<TEvent> callback
            ) where TEvent : EventBase<TEvent>, new()
        {
            uiElement.UnregisterCallback(callback);
        }
    }
}
