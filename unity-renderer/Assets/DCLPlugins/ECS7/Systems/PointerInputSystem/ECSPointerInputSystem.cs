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

namespace ECSSystems.PointerInputSystem
{
    public class ECSPointerInputSystem
    {
        private static readonly InputAction[] INPUT_ACTION_ENUM = (InputAction[])Enum.GetValues(typeof(WebInterface.ACTION_BUTTON));

        private IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private IInternalECSComponent<InternalPointerEvents> pointerEvents;
        private DataStore_ECS7 dataStoreEcs7;
        private EntityInput lastHoverFeedback;
        private IWorldState worldState;
        private IECSInteractionHoverCanvas interactionHoverCanvas;
        private bool[] inputActionState;
        private RestrictedActionsContext restrictedActionsRpcContext;

        private class EntityInput
        {
            public long entityId;
            public IParcelScene scene;
            public int sceneNumber;
            public bool hasValue;
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
            IECSReadOnlyComponentData<InternalColliders> colliderData = doesRaycastHit
                ? GetEntityWithCollider(pointerColliderComponent, raycastHit.collider)
                : null;

            IParcelScene colliderScene = colliderData != null ? colliderData.scene : null;

            bool isAnyButtonDown = false;
            bool hasAnyButtonChangedItsState = false;
            bool hasHoverEventEmitted = false;

            // Emit command for button states
            var curState = dataStoreEcs7.inputActionState;
            var prevState = inputActionState;

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
                            colliderData.scene,
                            colliderData.entity.entityId,
                            raycastRay,
                            raycastHit,
                            pointerEventType
                        );
                    }

                    BroadcastInputResultEvent(
                        inputResultComponent,
                        loadedScenes,
                        inputAction,
                        -1,
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
                HandleColliderChanged(colliderData, lastHoverFeedback, raycastEvent, interactionHoverCanvas, inputResultComponent, worldState);
            }

            // 2) We were hitting a collider A and now we're not hitting anything
            else if (IsColliderMissing(lastHoverFeedback, colliderData))
            {
                HandleMissingCollider(lastHoverFeedback, raycastEvent, interactionHoverCanvas, inputResultComponent, worldState);
            }

            // 3) We were not hitting anything and now we're hitting collider A
            else if (IsColliderAvailable(lastHoverFeedback, colliderData))
            {
                HandleAvailableCollider(colliderData, lastHoverFeedback, raycastEvent, inputResultComponent);
            }

            if (colliderData != null)
            {
                var hoverEvents = pointerEvents.GetFor(colliderData.scene, colliderData.entity)?.model.PointerEvents;
                ShowHoverTooltips(interactionHoverCanvas, hoverEvents, curState, raycastHit.distance, isAnyButtonDown);
            }
        }

        private static bool IsColliderDifferent(EntityInput lastHoverFeedback, IECSReadOnlyComponentData<InternalColliders> colliderData)
        {
            return colliderData != null && // current collider
                   lastHoverFeedback.hasValue && // previous collider
                   (lastHoverFeedback.entityId != colliderData.entity.entityId ||
                    lastHoverFeedback.sceneNumber != colliderData.scene.sceneData.sceneNumber);
        }

        private static bool IsColliderMissing(EntityInput lastHoverFeedback, IECSReadOnlyComponentData<InternalColliders> colliderData)
        {
            return colliderData == null && lastHoverFeedback.hasValue;
        }

        private static bool IsColliderAvailable(EntityInput lastHoverFeedback, IECSReadOnlyComponentData<InternalColliders> colliderData)
        {
            return colliderData != null && !lastHoverFeedback.hasValue;
        }

        private static void HandleColliderChanged(
            IECSReadOnlyComponentData<InternalColliders> colliderData,
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
                    PointerEventType.PetHoverLeave
                );
            }

            AddInputResultEvent(
                inputResultComponent,
                InputAction.IaPointer,
                colliderData.scene,
                colliderData.entity.entityId,
                raycastRay,
                raycastHit,
                PointerEventType.PetHoverEnter
            );

            interactionHoverCanvas.Hide();

            lastHoverFeedback.hasValue = true;
            lastHoverFeedback.sceneNumber = colliderData.scene.sceneData.sceneNumber;
            lastHoverFeedback.scene = colliderData.scene;
            lastHoverFeedback.entityId = colliderData.entity.entityId;
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
                    PointerEventType.PetHoverLeave
                );
            }

            interactionHoverCanvas.Hide();
            lastHoverFeedback.hasValue = false;
        }

        private static void HandleAvailableCollider(
            IECSReadOnlyComponentData<InternalColliders> colliderData,
            EntityInput lastHoverFeedback,
            DataStore_ECS7.RaycastEvent lastPointerRayHit,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent)
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
                PointerEventType.PetHoverEnter
            );

            lastHoverFeedback.hasValue = true;
            lastHoverFeedback.sceneNumber = colliderData.scene.sceneData.sceneNumber;
            lastHoverFeedback.scene = colliderData.scene;
            lastHoverFeedback.entityId = colliderData.entity.entityId;
        }

        private static void BroadcastInputResultEvent(IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IReadOnlyList<IParcelScene> scenes,
            InputAction buttonId,
            long entityId,
            Ray ray,
            DataStore_ECS7.RaycastEvent.Hit raycastHit,
            PointerEventType pointerEventType,
            IParcelScene skipScene = null)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i] != skipScene)
                {
                    AddInputResultEvent(inputResultComponent, buttonId, scenes[i], entityId, ray, raycastHit, pointerEventType);
                }
            }
        }

        private static void AddInputResultEvent(IInternalECSComponent<InternalInputEventResults> inputResultComponent, InputAction buttonId, IParcelScene scene,
            long entityId, Ray ray, DataStore_ECS7.RaycastEvent.Hit raycastHit, PointerEventType pointerEventType)
        {
            raycastHit.point = WorldStateUtils.ConvertUnityToScenePosition(raycastHit.point, scene);
            ray.origin = WorldStateUtils.ConvertUnityToScenePosition(ray.origin, scene);

            inputResultComponent.AddEvent(scene, new InternalInputEventResults.EventData()
            {
                button = buttonId,
                hit = ProtoConvertUtils.ToPBRaycasHit(entityId, null,
                    ray, raycastHit.distance, raycastHit.point, raycastHit.normal, entityId != -1),
                type = pointerEventType
            });
        }

        private static IECSReadOnlyComponentData<InternalColliders> GetEntityWithCollider(
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
    }
}
