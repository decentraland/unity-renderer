using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public class UIEventSubscription<TEvent> : IDisposable where TEvent : PointerEventBase<TEvent>, new()
    {
        private readonly VisualElement uiElement;
        private readonly IParcelScene parcelScene;
        private readonly IDCLEntity dclEntity;
        private readonly PointerEventType pointerEventType;
        private readonly InputAction inputAction;
        private readonly IInternalECSComponent<InternalInputEventResults> results;

        public UIEventSubscription(VisualElement uiElement,
            IParcelScene parcelScene,
            IDCLEntity dclEntity,
            PointerEventType pointerEventType,
            PBPointerEvents.Types.Info requestInfo,
            IInternalECSComponent<InternalInputEventResults> results
        )
        {
            this.uiElement = uiElement;
            this.parcelScene = parcelScene;
            this.dclEntity = dclEntity;
            this.pointerEventType = pointerEventType;
            this.results = results;

            // every other field is irrelevant for UI
            inputAction = requestInfo.Button;

            uiElement.RegisterCallback<TEvent>(OnEventRaised);
        }

        private void OnEventRaised(TEvent evt)
        {
            // filter by button
            if (!IsInputAccepted(evt))
                return;

            var eventData = new InternalInputEventResults.EventData
            {
                button = (InputAction) evt.button,
                type = pointerEventType,
                hit = new RaycastHit
                {
                    EntityId = dclEntity.entityId,
                    Length = 0,
                    Position = ProtoConvertUtils.UnityVectorToPBVector(evt.position)
                }
            };

            results.AddEvent(parcelScene, eventData);
        }

        private bool IsInputAccepted(TEvent evt) =>
            inputAction == InputAction.IaAny ||
            (UIButtonAcceptanceMap.ACCEPTED_INPUT_ACTIONS.Length > evt.button
             && UIButtonAcceptanceMap.ACCEPTED_INPUT_ACTIONS[evt.button] == inputAction);

        public void Dispose()
        {
            uiElement.UnregisterCallback<TEvent>(OnEventRaised);
        }
    }
}
