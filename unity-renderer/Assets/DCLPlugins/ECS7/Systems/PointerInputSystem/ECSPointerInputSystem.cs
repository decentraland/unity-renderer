using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using Ray = UnityEngine.Ray;

namespace ECSSystems.PointerInputSystem
{
    public static class ECSPointerInputSystem
    {
        private class State
        {
            public IInternalECSComponent<InternalColliders> pointerColliderComponent;
            public IInternalECSComponent<InternalInputEventResults> inputResultComponent;
            public ECSComponent<PBPointerEvents> pointerEvents;
            public DataStore_ECS7 dataStoreEcs7;
            public bool isLastInputPointerDown;
            public IWorldState worldState;
            public EntityInput lastInputDown;
            public EntityInput lastInputHover;
            public EntityInput lastHoverFeedback;
            public IECSInteractionHoverCanvas interactionHoverCanvas;
        }

        private class EntityInput
        {
            public IDCLEntity entity;
            public IParcelScene scene;
            public string sceneId;
            public bool hasValue;
        }

        private enum InputHitType
        {
            None,
            PointerDown,
            PointerUp,
            PointerHover
        }

        public static Action CreateSystem(
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            ECSComponent<PBPointerEvents> pointerEvents,
            IECSInteractionHoverCanvas interactionHoverCanvas,
            IWorldState worldState,
            DataStore_ECS7 dataStoreEcs)
        {
            var state = new State()
            {
                pointerColliderComponent = pointerColliderComponent,
                inputResultComponent = inputResultComponent,
                pointerEvents = pointerEvents,
                interactionHoverCanvas = interactionHoverCanvas,
                isLastInputPointerDown = false,
                dataStoreEcs7 = dataStoreEcs,
                worldState = worldState,
                lastInputDown = new EntityInput() { hasValue = false },
                lastInputHover = new EntityInput() { hasValue = false },
                lastHoverFeedback = new EntityInput() { hasValue = false },
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            DataStore_ECS7.PointerEvent currentPointerInput = state.dataStoreEcs7.lastPointerInputEvent;

            bool isHit = state.dataStoreEcs7.lastPointerRayHit.hasValue && state.dataStoreEcs7.lastPointerRayHit.didHit;
            bool isPointerDown = currentPointerInput.hasValue && currentPointerInput.isButtonDown;
            bool isPointerUp = state.isLastInputPointerDown && currentPointerInput.hasValue && !currentPointerInput.isButtonDown;

            DataStore_ECS7.RaycastEvent.Hit raycastHit = state.dataStoreEcs7.lastPointerRayHit.hit;
            Ray raycastRay = state.dataStoreEcs7.lastPointerRayHit.ray;

            IECSReadOnlyComponentData<InternalColliders> colliderData = isHit
                ? state.pointerColliderComponent
                       .GetEntityWithCollider(raycastHit.collider)
                : null;

            bool isRaycastHitValidEntity = colliderData != null;
            bool isHoveringInput = !isPointerDown && !isPointerUp && !state.isLastInputPointerDown;
            bool isHoveringExit = !isRaycastHitValidEntity && state.lastInputHover.hasValue;

            IList<PBPointerEvents.Types.Entry> hoverEvents = isRaycastHitValidEntity
                ? state.pointerEvents.GetPointerEventsForEntity(colliderData.scene, colliderData.entity)
                : null;

            // show hover tooltip for pointer down
            if (hoverEvents != null && isHoveringInput)
            {
                if (!state.lastHoverFeedback.hasValue || state.lastHoverFeedback.entity != colliderData.entity)
                {
                    state.interactionHoverCanvas.ShowPointerDownHover(hoverEvents, raycastHit.distance);
                    state.lastHoverFeedback.hasValue = true;
                    state.lastHoverFeedback.entity = colliderData.entity;
                    state.lastHoverFeedback.scene = colliderData.scene;
                    state.lastHoverFeedback.sceneId = colliderData.scene.sceneData.id;
                }
            }
            // show hover tooltip for pointer up
            else if (hoverEvents != null && state.isLastInputPointerDown && !isPointerUp)
            {
                if (!state.lastHoverFeedback.hasValue && state.lastInputDown.entity == colliderData.entity)
                {
                    state.interactionHoverCanvas.ShowPointerUpHover(hoverEvents, raycastHit.distance, (ActionButton)currentPointerInput.buttonId);
                    state.lastHoverFeedback.hasValue = true;
                    state.lastHoverFeedback.entity = colliderData.entity;
                    state.lastHoverFeedback.scene = colliderData.scene;
                    state.lastHoverFeedback.sceneId = colliderData.scene.sceneData.id;
                }
            }
            // hide hover tooltip
            else if (state.lastHoverFeedback.hasValue)
            {
                state.interactionHoverCanvas.Hide();
                state.lastHoverFeedback.hasValue = false;
            }

            InputHitType inputHitType = InputHitType.None;

            if (isRaycastHitValidEntity)
            {
                inputHitType = isPointerDown ? InputHitType.PointerDown
                    : isPointerUp ? InputHitType.PointerUp
                    : isHoveringInput ? InputHitType.PointerHover
                    : InputHitType.None;
            }

            // process entity hit with input
            switch (inputHitType)
            {
                case InputHitType.PointerDown:
                    state.lastInputDown.entity = colliderData.entity;
                    state.lastInputDown.scene = colliderData.scene;
                    state.lastInputDown.sceneId = colliderData.scene.sceneData.id;
                    state.lastInputDown.hasValue = true;

                    state.inputResultComponent.AddEvent(colliderData.scene, new InternalInputEventResults.EventData()
                    {
                        analog = 1,
                        button = (ActionButton)currentPointerInput.buttonId,
                        hit = ProtoConvertUtils.ToPBRaycasHit(colliderData.entity.entityId, null,
                            raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal),
                        type = PointerEventType.Down
                    });
                    break;

                case InputHitType.PointerUp:
                    bool validInputDownExist = state.lastInputDown.hasValue;
                    EntityInput lastInputDownData = state.lastInputDown;

                    // did it hit same entity as pointer down hit?
                    if (validInputDownExist && colliderData.entity.entityId == lastInputDownData.entity.entityId
                                            && colliderData.scene.sceneData.id == lastInputDownData.sceneId)
                    {
                        state.inputResultComponent.AddEvent(colliderData.scene, new InternalInputEventResults.EventData()
                        {
                            analog = 1,
                            button = (ActionButton)currentPointerInput.buttonId,
                            hit = ProtoConvertUtils.ToPBRaycasHit(colliderData.entity.entityId, null,
                                raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal),
                            type = PointerEventType.Up
                        });
                    }
                    // did it hit different entity as pointer down hit?
                    else if (validInputDownExist)
                    {
                        bool isEntityFromSameScene = colliderData.scene.sceneData.id == lastInputDownData.sceneId;
                        bool isValidScene = isEntityFromSameScene || state.worldState.ContainsScene(lastInputDownData.sceneId);

                        if (isValidScene)
                        {
                            state.inputResultComponent.AddEvent(lastInputDownData.scene, new InternalInputEventResults.EventData()
                            {
                                analog = 1,
                                button = (ActionButton)currentPointerInput.buttonId,
                                hit = ProtoConvertUtils.ToPBRaycasHit(-1, null,
                                    raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal, false),
                                type = PointerEventType.Up
                            });
                        }
                    }
                    state.lastInputDown.hasValue = false;
                    break;

                case InputHitType.PointerHover:
                    bool isPreviouslyHoveredEntity = state.lastInputHover.hasValue;
                    bool isHoveringNewEntity = !isPreviouslyHoveredEntity
                                               || state.lastInputHover.entity.entityId != colliderData.entity.entityId
                                               || state.lastInputHover.sceneId != colliderData.scene.sceneData.id;

                    // was other entity previously hovered?
                    if (isPreviouslyHoveredEntity && isHoveringNewEntity)
                    {
                        bool isValidScene = colliderData.scene.sceneData.id == state.lastInputHover.sceneId
                                            || state.worldState.ContainsScene(state.lastInputHover.sceneId);

                        if (isValidScene)
                        {
                            state.inputResultComponent.AddEvent(state.lastInputHover.scene, new InternalInputEventResults.EventData()
                            {
                                analog = 1,
                                button = (ActionButton)currentPointerInput.buttonId,
                                hit = ProtoConvertUtils.ToPBRaycasHit(state.lastInputHover.entity.entityId, null,
                                    raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal),
                                type = PointerEventType.HoverLeave
                            });
                        }
                    }

                    // hover enter
                    if (isHoveringNewEntity)
                    {
                        state.lastInputHover.entity = colliderData.entity;
                        state.lastInputHover.scene = colliderData.scene;
                        state.lastInputHover.sceneId = colliderData.scene.sceneData.id;
                        state.lastInputHover.hasValue = true;

                        state.inputResultComponent.AddEvent(colliderData.scene, new InternalInputEventResults.EventData()
                        {
                            analog = 1,
                            button = (ActionButton)currentPointerInput.buttonId,
                            hit = ProtoConvertUtils.ToPBRaycasHit(colliderData.entity.entityId, null,
                                raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal),
                            type = PointerEventType.HoverEnter
                        });
                    }
                    break;
            }

            // no entity hit
            if (!isRaycastHitValidEntity)
            {
                if (isPointerUp)
                {
                    if (state.lastInputDown.hasValue)
                    {
                        // input up without hit but with valid input down
                        bool isValidScene = state.worldState.ContainsScene(state.lastInputDown.sceneId);
                        if (isValidScene)
                        {
                            state.inputResultComponent.AddEvent(state.lastInputDown.scene, new InternalInputEventResults.EventData()
                            {
                                analog = 1,
                                button = (ActionButton)currentPointerInput.buttonId,
                                hit = ProtoConvertUtils.ToPBRaycasHit(-1, null,
                                    raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal, false),
                                type = PointerEventType.Up
                            });
                        }
                    }

                    state.lastInputDown.hasValue = false;
                }
                if (isHoveringExit)
                {
                    bool isValidScene = state.worldState.ContainsScene(state.lastInputHover.sceneId);

                    if (isValidScene)
                    {
                        state.inputResultComponent.AddEvent(state.lastInputHover.scene, new InternalInputEventResults.EventData()
                        {
                            analog = 1,
                            button = (ActionButton)currentPointerInput.buttonId,
                            hit = ProtoConvertUtils.ToPBRaycasHit(state.lastInputHover.entity.entityId, null,
                                raycastRay, raycastHit.distance, raycastHit.point, raycastHit.normal),
                            type = PointerEventType.HoverLeave
                        });
                    }
                    state.lastInputHover.hasValue = false;
                }
            }

            state.dataStoreEcs7.lastPointerInputEvent.hasValue = false;
            state.dataStoreEcs7.lastPointerRayHit.hasValue = false;
            state.isLastInputPointerDown = isPointerDown || (!isPointerUp && state.isLastInputPointerDown);
        }

        private static IECSReadOnlyComponentData<InternalColliders> GetEntityWithCollider(
            this IInternalECSComponent<InternalColliders> pointerColliderComponent,
            Collider collider)
        {
            var collidersData = pointerColliderComponent.GetForAll();
            for (int i = 0; i < collidersData.Count; i++)
            {
                var colliderData = collidersData[i].value;
                if (colliderData.model.colliders.Contains(collider))
                {
                    return colliderData;
                }
            }
            return null;
        }

        private static IList<PBPointerEvents.Types.Entry> GetPointerEventsForEntity(this ECSComponent<PBPointerEvents> component,
            IParcelScene scene, IDCLEntity entity)
        {
            var componentData = component.Get(scene, entity);
            return componentData?.model.PointerEvents;
        }

        private static void ShowPointerDownHover(this IECSInteractionHoverCanvas canvas,
            IList<PBPointerEvents.Types.Entry> entityEvents, float distance)
        {
            canvas.ShowHoverTooltips(entityEvents, (pointerEvent) =>
                pointerEvent.EventType == PointerEventType.Down
                && pointerEvent.EventInfo.GetShowFeedback()
                && distance <= pointerEvent.EventInfo.GetMaxDistance()
            );
        }

        private static void ShowPointerUpHover(this IECSInteractionHoverCanvas canvas,
            IList<PBPointerEvents.Types.Entry> entityEvents, float distance, ActionButton expectedButton)
        {
            canvas.ShowHoverTooltips(entityEvents, (pointerEvent) =>
                pointerEvent.EventType == PointerEventType.Up
                && pointerEvent.EventInfo.GetShowFeedback()
                && distance <= pointerEvent.EventInfo.GetMaxDistance()
                && (pointerEvent.EventInfo.GetButton() == expectedButton || pointerEvent.EventInfo.GetButton() == ActionButton.Any)
            );
        }

        private static void ShowHoverTooltips(this IECSInteractionHoverCanvas canvas,
            IList<PBPointerEvents.Types.Entry> entityEvents, Func<PBPointerEvents.Types.Entry, bool> filter)
        {
            if (entityEvents is null)
                return;

            bool anyTooltipAdded = false;
            int eventIndex = 0;
            for (int i = 0; i < canvas.tooltipsCount; i++)
            {
                PBPointerEvents.Types.Entry pointerEvent = null;
                for (; eventIndex < entityEvents.Count; eventIndex++)
                {
                    pointerEvent = entityEvents[eventIndex];
                    if (filter(pointerEvent))
                    {
                        eventIndex++;
                        break;
                    }
                    pointerEvent = null;
                }

                if (!(pointerEvent is null))
                {
                    anyTooltipAdded = true;
                    canvas.SetTooltipText(i, pointerEvent.EventInfo.GetHoverText());
                    canvas.SetTooltipInput(i, pointerEvent.EventInfo.GetButton());
                    canvas.SetTooltipActive(i, true);
                }
                else
                {
                    canvas.SetTooltipActive(i, false);
                }
            }

            if (anyTooltipAdded)
            {
                canvas.Show();
            }
        }
    }
}