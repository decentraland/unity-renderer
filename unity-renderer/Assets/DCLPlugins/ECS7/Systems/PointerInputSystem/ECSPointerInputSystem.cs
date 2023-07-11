using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;
using RaycastHit = DCL.ECSComponents.RaycastHit;

namespace ECSSystems.PointerInputSystem
{
    public class ECSPointerInputSystem
    {
        private static readonly InputAction[] INPUT_ACTION_ENUM = (InputAction[])Enum.GetValues(typeof(WebInterface.ACTION_BUTTON));

        private readonly IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private readonly IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private readonly IInternalECSComponent<InternalPointerEvents> pointerEvents;
        private readonly DataStore_ECS7 dataStoreEcs7;
        private readonly EntityInput lastHoverFeedback;
        private readonly IWorldState worldState;
        private readonly IECSInteractionHoverCanvas interactionHoverCanvas;
        private readonly bool[] inputActionState;
        private readonly RestrictedActionsContext restrictedActionsRpcContext;

        private class EntityInput
        {
            public long entityId;
            public IParcelScene scene;
            public int sceneNumber;
            public bool hasValue;
            public IReadOnlyList<InternalPointerEvents.Entry> pointerEvents;
        }

        public ECSPointerInputSystem(
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalPointerEvents> pointerEvents,
            IECSInteractionHoverCanvas interactionHoverCanvas,
            IWorldState worldState,
            DataStore_ECS7 dataStoreEcs,
            RestrictedActionsContext restrictedActionsRpcContext)
        {
            this.pointerColliderComponent = pointerColliderComponent;
            this.inputResultComponent = inputResultComponent;
            this.pointerEvents = pointerEvents;
            this.worldState = worldState;
            this.interactionHoverCanvas = interactionHoverCanvas;
            this.dataStoreEcs7 = dataStoreEcs;
            this.lastHoverFeedback = new EntityInput() { hasValue = false };
            this.inputActionState = new bool[INPUT_ACTION_ENUM.Length];
            this.restrictedActionsRpcContext = restrictedActionsRpcContext;
        }

        public void Update()
        {
            // Retrieve the last raycast hit
            bool doesRaycastHit = dataStoreEcs7.lastPointerRayHit.hasValue && dataStoreEcs7.lastPointerRayHit.didHit;
            DataStore_ECS7.RaycastEvent raycastEvent = dataStoreEcs7.lastPointerRayHit;
            DataStore_ECS7.RaycastEvent.Hit raycastHit = raycastEvent.hit;
            Ray raycastRay = raycastEvent.ray;
            IReadOnlyList<IParcelScene> loadedScenes = dataStoreEcs7.scenes;

            // Get the collider that the raycast hit
            ECSComponentData<InternalColliders>? colliderData = doesRaycastHit
                ? GetEntityWithCollider(pointerColliderComponent, raycastHit.collider)
                : null;

            IParcelScene colliderScene = colliderData?.scene;

            IReadOnlyList<InternalPointerEvents.Entry> entityPointerEvents = colliderData != null
                ? pointerEvents.GetFor(colliderData.Value.scene, colliderData.Value.entity)?.model.PointerEvents
                : null;

            bool isAnyButtonDown = false;

            // Emit command for button states
            bool[] curState = dataStoreEcs7.inputActionState;
            bool[] prevState = inputActionState;

            for (int i = 0; i < curState.Length; i++)
            {
                isAnyButtonDown |= curState[i];

                if (curState[i] != prevState[i])
                {
                    PointerEventType pointerEventType = curState[i] ? PointerEventType.PetDown : PointerEventType.PetUp;
                    InputAction inputAction = INPUT_ACTION_ENUM[i];

                    if (colliderData != null)
                    {
                        AddInputResultEvent(
                            inputResultComponent,
                            inputAction,
                            colliderData.Value.scene,
                            colliderData.Value.entity.entityId,
                            raycastRay,
                            raycastHit,
                            pointerEventType,
                            entityPointerEvents
                        );
                    }

                    BroadcastInputResultEvent(
                        inputResultComponent,
                        loadedScenes,
                        inputAction,
                        raycastRay,
                        raycastHit,
                        pointerEventType,
                        colliderScene
                    );

                    // update
                    prevState[i] = curState[i];

                    // set current frame count since input is required to prompt modals
                    // for externalUrl and Nft
                    if (curState[i] && IsValidInputForUnlockingUiPrompts(inputAction))
                    {
                        restrictedActionsRpcContext.LastFrameWithInput = Time.frameCount;
                    }
                }
            }

            // Check if the hovered entity has changed with three options:
            // 1) We were hitting a collider A and now we're hitting a collider B
            if (IsColliderDifferent(lastHoverFeedback, colliderData))
            {
                HandleColliderChanged(colliderData.Value, lastHoverFeedback, raycastEvent, interactionHoverCanvas, inputResultComponent, worldState, entityPointerEvents);
            }

            // 2) We were hitting a collider A and now we're not hitting anything
            else if (IsColliderMissing(lastHoverFeedback, colliderData))
            {
                HandleMissingCollider(lastHoverFeedback, raycastEvent, interactionHoverCanvas, inputResultComponent, worldState);
            }

            // 3) We were not hitting anything and now we're hitting collider A
            else if (IsColliderAvailable(lastHoverFeedback, colliderData))
            {
                HandleAvailableCollider(colliderData.Value, lastHoverFeedback, raycastEvent, inputResultComponent, entityPointerEvents);
            }

            if (entityPointerEvents != null)
            {
                ShowHoverTooltips(interactionHoverCanvas, entityPointerEvents, curState, raycastHit.distance, isAnyButtonDown);
            }
        }

        private static bool IsColliderDifferent(EntityInput lastHoverFeedback, ECSComponentData<InternalColliders>? colliderData)
        {
            return colliderData != null && // current collider
                   lastHoverFeedback.hasValue && // previous collider
                   (lastHoverFeedback.entityId != colliderData.Value.entity.entityId ||
                    lastHoverFeedback.sceneNumber != colliderData.Value.scene.sceneData.sceneNumber);
        }

        private static bool IsColliderMissing(EntityInput lastHoverFeedback, ECSComponentData<InternalColliders>? colliderData)
        {
            return colliderData == null && lastHoverFeedback.hasValue;
        }

        private static bool IsColliderAvailable(EntityInput lastHoverFeedback, ECSComponentData<InternalColliders>? colliderData)
        {
            return colliderData != null && !lastHoverFeedback.hasValue;
        }

        private static void HandleColliderChanged(
            ECSComponentData<InternalColliders> colliderData,
            EntityInput lastHoverFeedback,
            DataStore_ECS7.RaycastEvent lastPointerRayHit,
            IECSInteractionHoverCanvas interactionHoverCanvas,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IWorldState worldState,
            IReadOnlyList<InternalPointerEvents.Entry> entityEvents)
        {
            DataStore_ECS7.RaycastEvent.Hit raycastHit = lastPointerRayHit.hit;
            Ray raycastRay = lastPointerRayHit.ray;

            if (worldState.ContainsScene(lastHoverFeedback.sceneNumber))
            {
                AddInputResultEvent(
                    inputResultComponent,
                    InputAction.IaPointer,
                    lastHoverFeedback.scene,
                    lastHoverFeedback.entityId,
                    raycastRay,
                    raycastHit,
                    PointerEventType.PetHoverLeave,
                    lastHoverFeedback.pointerEvents
                );
            }

            AddInputResultEvent(
                inputResultComponent,
                InputAction.IaPointer,
                colliderData.scene,
                colliderData.entity.entityId,
                raycastRay,
                raycastHit,
                PointerEventType.PetHoverEnter,
                entityEvents
            );

            interactionHoverCanvas.Hide();

            lastHoverFeedback.hasValue = true;
            lastHoverFeedback.sceneNumber = colliderData.scene.sceneData.sceneNumber;
            lastHoverFeedback.scene = colliderData.scene;
            lastHoverFeedback.entityId = colliderData.entity.entityId;
            lastHoverFeedback.pointerEvents = entityEvents;
        }

        private static void HandleMissingCollider(
            EntityInput lastHoverFeedback,
            DataStore_ECS7.RaycastEvent lastPointerRayHit,
            IECSInteractionHoverCanvas interactionHoverCanvas,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IWorldState worldState)
        {
            DataStore_ECS7.RaycastEvent.Hit raycastHit = lastPointerRayHit.hit;
            Ray raycastRay = lastPointerRayHit.ray;

            if (worldState.ContainsScene(lastHoverFeedback.sceneNumber))
            {
                AddInputResultEvent(
                    inputResultComponent,
                    InputAction.IaPointer,
                    lastHoverFeedback.scene,
                    lastHoverFeedback.entityId,
                    raycastRay,
                    raycastHit,
                    PointerEventType.PetHoverLeave,
                    lastHoverFeedback.pointerEvents
                );
            }

            interactionHoverCanvas.Hide();
            lastHoverFeedback.hasValue = false;
        }

        private static void HandleAvailableCollider(
            ECSComponentData<InternalColliders> colliderData,
            EntityInput lastHoverFeedback,
            DataStore_ECS7.RaycastEvent lastPointerRayHit,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IReadOnlyList<InternalPointerEvents.Entry> entityEvents)
        {
            DataStore_ECS7.RaycastEvent.Hit raycastHit = lastPointerRayHit.hit;
            Ray raycastRay = lastPointerRayHit.ray;

            AddInputResultEvent(
                inputResultComponent,
                InputAction.IaPointer,
                colliderData.scene,
                colliderData.entity.entityId,
                raycastRay,
                raycastHit,
                PointerEventType.PetHoverEnter,
                entityEvents
            );

            lastHoverFeedback.hasValue = true;
            lastHoverFeedback.sceneNumber = colliderData.scene.sceneData.sceneNumber;
            lastHoverFeedback.scene = colliderData.scene;
            lastHoverFeedback.entityId = colliderData.entity.entityId;
            lastHoverFeedback.pointerEvents = entityEvents;
        }

        // Sent input result to other scenes
        private static void BroadcastInputResultEvent(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IReadOnlyList<IParcelScene> scenes,
            InputAction buttonId,
            Ray ray,
            DataStore_ECS7.RaycastEvent.Hit raycastHit,
            PointerEventType pointerEventType,
            IParcelScene skipScene = null)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i] != skipScene)
                {
                    AddInputResultEvent(inputResultComponent, buttonId, scenes[i], -1, ray, raycastHit, pointerEventType, null);
                }
            }
        }

        private static void AddInputResultEvent(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            InputAction buttonId,
            IParcelScene scene,
            long entityId,
            Ray ray,
            DataStore_ECS7.RaycastEvent.Hit raycastHit,
            PointerEventType pointerEventType,
            IReadOnlyList<InternalPointerEvents.Entry> entityEvents)
        {
            RaycastHit hitInfo = null;

            // If entity has pointer event component for this `pointerEventType` we setup the `hit` data
            // otherwise we leave it empty (global input)
            if (HasInputEvent(entityEvents, pointerEventType, raycastHit.distance))
            {
                ray.origin = WorldStateUtils.ConvertUnityToScenePosition(ray.origin, scene);

                hitInfo = ProtoConvertUtils.ToPBRaycasHit(
                    entityId,
                    null,
                    ray,
                    raycastHit.distance,
                    WorldStateUtils.ConvertUnityToScenePosition(raycastHit.point, scene),
                    raycastHit.normal);
            }

            // If entity does not have pointer event component for this `pointerEventType` we ignore the event
            // so it's not send to the scene
            else if (pointerEventType == PointerEventType.PetHoverEnter || pointerEventType == PointerEventType.PetHoverLeave)
            {
                return;
            }

            inputResultComponent.AddEvent(scene, new InternalInputEventResults.EventData()
            {
                button = buttonId,
                hit = hitInfo,
                type = pointerEventType
            });
        }

        private static ECSComponentData<InternalColliders>? GetEntityWithCollider(
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            Collider collider)
        {
            var collidersData = pointerColliderComponent.GetForAll();

            for (int i = 0; i < collidersData.Count; i++)
            {
                var colliderData = collidersData[i].value;

                if (colliderData.model.colliders.ContainsKey(collider))
                {
                    return colliderData;
                }
            }

            return null;
        }

        private static void ShowHoverTooltips(IECSInteractionHoverCanvas canvas,
            IReadOnlyList<InternalPointerEvents.Entry> entityEvents, bool[] buttonState, float distance, bool isAnyButtonDown)
        {
            if (entityEvents is null)
                return;

            int tooltipIndex = -1;
            bool shouldAdd = false;
            InternalPointerEvents.Entry pointerEvent;

            for (int eventIndex = 0; eventIndex < entityEvents.Count; eventIndex++)
            {
                shouldAdd = false;
                pointerEvent = entityEvents[eventIndex];

                if (!pointerEvent.EventInfo.ShowFeedback || distance > pointerEvent.EventInfo.MaxDistance)
                    continue;

                int buttonId = (int)pointerEvent.EventInfo.Button;

                if (buttonId == (int)InputAction.IaAny)
                {
                    if (
                        (isAnyButtonDown && pointerEvent.EventType == PointerEventType.PetUp) ||
                        (!isAnyButtonDown && pointerEvent.EventType == PointerEventType.PetDown))
                    {
                        shouldAdd = true;
                    }
                }
                else if (buttonId >= 0 && buttonId < buttonState.Length)
                {
                    bool buttonIsDown = buttonState[buttonId];

                    if ((pointerEvent.EventType == PointerEventType.PetDown && !buttonIsDown)
                        || (pointerEvent.EventType == PointerEventType.PetUp && buttonIsDown))
                    {
                        shouldAdd = true;
                    }
                }

                if (shouldAdd)
                {
                    tooltipIndex++;
                    canvas.SetTooltipText(tooltipIndex, pointerEvent.EventInfo.HoverText);
                    canvas.SetTooltipInput(tooltipIndex, pointerEvent.EventInfo.Button);
                    canvas.SetTooltipActive(tooltipIndex, true);
                }
            }

            // first tooltip free
            for (int i = tooltipIndex + 1; i < canvas.tooltipsCount; i++)
            {
                canvas.SetTooltipActive(i, false);
            }

            if (tooltipIndex != -1)
            {
                canvas.Show();
            }
            else
            {
                canvas.Hide();
            }
        }

        private static bool IsValidInputForUnlockingUiPrompts(InputAction inputAction)
        {
            return inputAction == InputAction.IaPointer
                   || inputAction == InputAction.IaPrimary
                   || inputAction == InputAction.IaSecondary
                   || inputAction == InputAction.IaAction3
                   || inputAction == InputAction.IaAction4
                   || inputAction == InputAction.IaAction5
                   || inputAction == InputAction.IaAction6;
        }

        private static bool HasInputEvent(
            IReadOnlyList<InternalPointerEvents.Entry> entityEvents,
            PointerEventType pointerEventType,
            float distance)
        {
            if (entityEvents == null)
                return false;

            for (int i = 0; i < entityEvents.Count; i++)
            {
                var inputEventEntry = entityEvents[i];

                if (inputEventEntry.EventType == pointerEventType)
                {
                    if (distance <= inputEventEntry.EventInfo.MaxDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
