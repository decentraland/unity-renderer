using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using Google.Protobuf;
using System;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public static class UIPointerEventsUtils
    {
        public static EventCallback<TEvent> RegisterFeedback<TEvent, TResultMessage>(
            IInternalECSComponent<InternalUIInputResults> inputResults,
            Func<TEvent, TResultMessage> createResult,
            IParcelScene scene,
            IDCLEntity entity,
            VisualElement uiElement,
            int resultComponentId)
            where TResultMessage: class, IMessage
            where TEvent: EventBase<TEvent>, new()
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
        ) where TEvent: EventBase<TEvent>, new()
        {
            uiElement.UnregisterCallback(callback);
        }
    }
}
