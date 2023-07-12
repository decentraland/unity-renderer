using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using UnityEngine.UIElements;
using RaycastHit = DCL.ECSComponents.RaycastHit;

namespace ECSSystems.ECSUiPointerEventsSystem
{
    public class ECSUiPointerEventsSystem
    {
        private readonly IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents> unregisteredUiPointerEventsGroup;
        private readonly IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents, InternalRegisteredUiPointerEvents> registeredUiPointerEventsGroup;
        private readonly IECSReadOnlyComponentsGroup<InternalRegisteredUiPointerEvents> registeredUiPointerEventsWithUiRemovedGroup;
        private readonly IECSReadOnlyComponentsGroup<InternalUiContainer, InternalRegisteredUiPointerEvents> registeredUiPointerEventsWithPointerEventsRemovedGroup;

        private readonly IInternalECSComponent<InternalRegisteredUiPointerEvents> registeredUiPointerEventsComponent;
        private readonly IInternalECSComponent<InternalInputEventResults> inputResultsComponent;

        public ECSUiPointerEventsSystem(
            IInternalECSComponent<InternalRegisteredUiPointerEvents> registeredUiPointerEventsComponent,
            IInternalECSComponent<InternalInputEventResults> inputResultsComponent,
            IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents> unregisteredUiPointerEventsGroup,
            IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents, InternalRegisteredUiPointerEvents> registeredUiPointerEventsGroup,
            IECSReadOnlyComponentsGroup<InternalRegisteredUiPointerEvents> registeredUiPointerEventsWithUiRemovedGroup,
            IECSReadOnlyComponentsGroup<InternalUiContainer, InternalRegisteredUiPointerEvents> registeredUiPointerEventsWithPointerEventsRemovedGroup)
        {
            this.registeredUiPointerEventsComponent = registeredUiPointerEventsComponent;
            this.unregisteredUiPointerEventsGroup = unregisteredUiPointerEventsGroup;
            this.registeredUiPointerEventsGroup = registeredUiPointerEventsGroup;
            this.registeredUiPointerEventsWithUiRemovedGroup = registeredUiPointerEventsWithUiRemovedGroup;
            this.registeredUiPointerEventsWithPointerEventsRemovedGroup = registeredUiPointerEventsWithPointerEventsRemovedGroup;
            this.inputResultsComponent = inputResultsComponent;
        }

        public void Update()
        {
            // adds `InternalRegisteredUiPointerEvents` component to entities that contains
            // `InternalUiContainer` and `InternalPointerEvents` components
            HandleUiWithoutRegisteredPointerEvents(
                unregisteredUiPointerEventsGroup,
                registeredUiPointerEventsComponent,
                inputResultsComponent);

            // updates `InternalRegisteredUiPointerEvents` component when
            // `InternalPointerEvents` is updated
            HandlePointerEventComponentUpdate(
                registeredUiPointerEventsGroup,
                registeredUiPointerEventsComponent,
                inputResultsComponent);

            // removes `InternalRegisteredUiPointerEvents` component from entities from which
            // `InternalUiContainer` component was removed
            HandleUiContainerRemoval(
                registeredUiPointerEventsWithUiRemovedGroup,
                registeredUiPointerEventsComponent);

            // removes `InternalRegisteredUiPointerEvents` component from entities from which
            // `InternalPointerEvents` component was removed
            HandlePointerEventsRemoval(
                registeredUiPointerEventsWithPointerEventsRemovedGroup,
                registeredUiPointerEventsComponent);
        }

        internal static void HandleUiWithoutRegisteredPointerEvents(
            IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents> unregisteredUiPointerEventsGroup,
            IInternalECSComponent<InternalRegisteredUiPointerEvents> registeredUiPointerEventsComponent,
            IInternalECSComponent<InternalInputEventResults> inputResultsComponent)
        {
            var unregisteredUiPointerEvents = unregisteredUiPointerEventsGroup.group;

            for (int i = unregisteredUiPointerEvents.Count - 1; i >= 0; i--)
            {
                var unregisteredGroupElement = unregisteredUiPointerEvents[i];
                IParcelScene scene = unregisteredGroupElement.scene;
                IDCLEntity entity = unregisteredGroupElement.entity;

                InternalRegisteredUiPointerEvents uiPointerEvents;

                // if `InternalPointerEvents` is dirty we delegate the callback setup to "InternalPointerEvents dirty" handler
                if (!unregisteredGroupElement.componentData2.model.dirty)
                {
                    VisualElement visualElement = unregisteredGroupElement.componentData1.model.rootElement;
                    InternalPointerEvents events = unregisteredGroupElement.componentData2.model;
                    uiPointerEvents = CreateUiPointerEvents(scene, entity, inputResultsComponent, events);
                    RegisterUiPointerEvents(visualElement, uiPointerEvents);
                }
                else
                {
                    uiPointerEvents = new InternalRegisteredUiPointerEvents();
                }

                registeredUiPointerEventsComponent.PutFor(scene, entity, uiPointerEvents);
            }
        }

        internal static void HandlePointerEventComponentUpdate(
            IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents, InternalRegisteredUiPointerEvents> registeredUiPointerEventsGroup,
            IInternalECSComponent<InternalRegisteredUiPointerEvents> registeredUiPointerEventsComponent,
            IInternalECSComponent<InternalInputEventResults> inputResultsComponent)
        {
            var group = registeredUiPointerEventsGroup.group;

            for (int i = 0; i < group.Count; i++)
            {
                var groupElement = group[i];

                if (!groupElement.componentData2.model.dirty)
                    continue;

                IParcelScene scene = groupElement.scene;
                IDCLEntity entity = groupElement.entity;
                VisualElement visualElement = groupElement.componentData1.model.rootElement;

                var registeredEventsData = registeredUiPointerEventsComponent.GetFor(scene, entity);

                if (registeredEventsData != null)
                {
                    UnregisterUiPointerEvents(visualElement, registeredEventsData.Value.model);
                }

                InternalPointerEvents events = groupElement.componentData2.model;
                InternalRegisteredUiPointerEvents uiPointerEvents = CreateUiPointerEvents(scene, entity, inputResultsComponent, events);
                RegisterUiPointerEvents(visualElement, uiPointerEvents);
                registeredUiPointerEventsComponent.PutFor(scene, entity, uiPointerEvents);
            }
        }

        internal static void HandleUiContainerRemoval(
            IECSReadOnlyComponentsGroup<InternalRegisteredUiPointerEvents> registeredUiPointerEventsWithUiRemovedGroup,
            IInternalECSComponent<InternalRegisteredUiPointerEvents> registeredUiPointerEventsComponent)
        {
            var removedUiContainers = registeredUiPointerEventsWithUiRemovedGroup.group;

            for (int i = removedUiContainers.Count - 1; i >= 0; i--)
            {
                var removedElement = removedUiContainers[i];
                registeredUiPointerEventsComponent.RemoveFor(removedElement.scene, removedElement.entity);
            }
        }

        internal static void HandlePointerEventsRemoval(
            IECSReadOnlyComponentsGroup<InternalUiContainer, InternalRegisteredUiPointerEvents> registeredUiPointerEventsWithPointerEventsRemovedGroup,
            IInternalECSComponent<InternalRegisteredUiPointerEvents> registeredUiPointerEventsComponent)
        {
            var removedPointerEvents = registeredUiPointerEventsWithPointerEventsRemovedGroup.group;

            for (int i = removedPointerEvents.Count - 1; i >= 0; i--)
            {
                var removedElement = removedPointerEvents[i];
                registeredUiPointerEventsComponent.RemoveFor(removedElement.scene, removedElement.entity);
            }
        }

        internal static InternalRegisteredUiPointerEvents CreateUiPointerEvents(
            IParcelScene scene,
            IDCLEntity entity,
            IInternalECSComponent<InternalInputEventResults> inputResultsComponent,
            InternalPointerEvents events)
        {
            InternalRegisteredUiPointerEvents model = new InternalRegisteredUiPointerEvents();

            for (int i = 0; i < events.PointerEvents.Count; i++)
            {
                var pointerEvent = events.PointerEvents[i];

                if (pointerEvent.EventInfo.Button != InputAction.IaPointer)
                    continue;

                switch (pointerEvent.EventType)
                {
                    case PointerEventType.PetUp:
                        model.OnPointerUpCallback += evt => SetInputResult(
                            scene,
                            entity,
                            inputResultsComponent,
                            InputAction.IaPointer,
                            pointerEvent.EventType,
                            evt.position);

                        break;
                    case PointerEventType.PetDown:
                        model.OnPointerDownCallback += evt => SetInputResult(
                            scene,
                            entity,
                            inputResultsComponent,
                            InputAction.IaPointer,
                            pointerEvent.EventType,
                            evt.position);

                        break;
                    case PointerEventType.PetHoverEnter:
                        model.OnPointerEnterCallback += evt => SetInputResult(
                            scene,
                            entity,
                            inputResultsComponent,
                            InputAction.IaPointer,
                            pointerEvent.EventType,
                            evt.position);

                        break;
                    case PointerEventType.PetHoverLeave:
                        model.OnPointerLeaveCallback += evt => SetInputResult(
                            scene,
                            entity,
                            inputResultsComponent,
                            InputAction.IaPointer,
                            pointerEvent.EventType,
                            evt.position);

                        break;
                }
            }

            return model;
        }

        internal static void RegisterUiPointerEvents(VisualElement visualElement, InternalRegisteredUiPointerEvents uiEvents)
        {
            if (uiEvents.OnPointerDownCallback != null)
                visualElement.RegisterCallback(uiEvents.OnPointerDownCallback);

            if (uiEvents.OnPointerUpCallback != null)
                visualElement.RegisterCallback(uiEvents.OnPointerUpCallback);

            if (uiEvents.OnPointerEnterCallback != null)
                visualElement.RegisterCallback(uiEvents.OnPointerEnterCallback);

            if (uiEvents.OnPointerLeaveCallback != null)
                visualElement.RegisterCallback(uiEvents.OnPointerLeaveCallback);
        }

        internal static void UnregisterUiPointerEvents(VisualElement visualElement, InternalRegisteredUiPointerEvents uiEvents)
        {
            if (uiEvents.OnPointerDownCallback != null)
                visualElement.UnregisterCallback(uiEvents.OnPointerDownCallback);

            if (uiEvents.OnPointerUpCallback != null)
                visualElement.UnregisterCallback(uiEvents.OnPointerUpCallback);

            if (uiEvents.OnPointerEnterCallback != null)
                visualElement.UnregisterCallback(uiEvents.OnPointerEnterCallback);

            if (uiEvents.OnPointerLeaveCallback != null)
                visualElement.UnregisterCallback(uiEvents.OnPointerLeaveCallback);
        }

        private static void SetInputResult(IParcelScene scene, IDCLEntity entity,
            IInternalECSComponent<InternalInputEventResults> inputResultsComponent,
            InputAction button, PointerEventType pointerEventType, Vector3 position)
        {
            var eventData = new InternalInputEventResults.EventData
            {
                button = button,
                type = pointerEventType,
                hit = new RaycastHit
                {
                    EntityId = (uint)entity.entityId,
                    Length = 0,
                    Position = ProtoConvertUtils.UnityVectorToPBVector(position)
                }
            };

            inputResultsComponent.AddEvent(scene, eventData);
        }
    }
}
