using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.PointerInputSystem
{
    public static class ECSPointerInputSystem
    {
        private static readonly InputAction[] INPUT_ACTION_ENUM = (InputAction[])Enum.GetValues(typeof(WebInterface.ACTION_BUTTON));

        private class State
        {
            public IInternalECSComponent<InternalColliders> pointerColliderComponent;
            public IInternalECSComponent<InternalInputEventResults> inputResultComponent;
            public IInternalECSComponent<InternalPointerEvents> pointerEvents;
            public DataStore_ECS7 dataStoreEcs7;
            public EntityInput lastHoverFeedback;
            public IWorldState worldState;
            public IECSInteractionHoverCanvas interactionHoverCanvas;
            public bool[] inputActionState;
        }

        private class EntityInput
        {
            public long entityId;

            public IParcelScene scene;
            public int sceneNumber;

            public bool hasValue;
        }

        public static Action CreateSystem(
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalPointerEvents> pointerEvents,
            IECSInteractionHoverCanvas interactionHoverCanvas,
            IWorldState worldState,
            DataStore_ECS7 dataStoreEcs)
        {
            var state = new State()
            {
                pointerColliderComponent = pointerColliderComponent,
                inputResultComponent = inputResultComponent,
                pointerEvents = pointerEvents,
                worldState = worldState,
                interactionHoverCanvas = interactionHoverCanvas,
                dataStoreEcs7 = dataStoreEcs,
                lastHoverFeedback = new EntityInput() { hasValue = false },
                inputActionState = new bool[INPUT_ACTION_ENUM.Length],
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            // Retrieve the last raycast hit
            bool doesRaycastHit = state.dataStoreEcs7.lastPointerRayHit.hasValue && state.dataStoreEcs7.lastPointerRayHit.didHit;
            DataStore_ECS7.RaycastEvent.Hit raycastHit = state.dataStoreEcs7.lastPointerRayHit.hit;
            Ray raycastRay = state.dataStoreEcs7.lastPointerRayHit.ray;

            // Get the collider that the raycast hit
            IECSReadOnlyComponentData<InternalColliders> colliderData = doesRaycastHit
                ? state.pointerColliderComponent
                       .GetEntityWithCollider(raycastHit.collider)
                : null;
            IParcelScene colliderScene = colliderData != null ? colliderData.scene : null;

            bool isAnyButtonDown = false;
            bool hasAnyButtonChangedItsState = false;
            bool hasHoverEventEmitted = false;

            // Emit command for button states
            var curState = state.dataStoreEcs7.inputActionState;
            var prevState = state.inputActionState;
            for (int i = 0; i < state.dataStoreEcs7.inputActionState.Length; i++)
            {
                isAnyButtonDown |= curState[i];

                if (curState[i] != prevState[i])
                {
                    PointerEventType pointerEventType = curState[i] ? PointerEventType.PetDown : PointerEventType.PetUp;
                    InputAction inputAction = INPUT_ACTION_ENUM[i];

                    if (colliderData != null)
                    {
                        AddInputResultEvent(
                            state,
                            inputAction,
                            colliderData.scene,
                            colliderData.entity.entityId,
                            raycastRay,
                            raycastHit,
                            pointerEventType
                        );
                    }

                    BroadcastInputResultEvent(
                        state,
                        inputAction,
                        -1,
                        raycastRay,
                        raycastHit,
                        pointerEventType,
                        colliderScene
                    );

                    // update
                    prevState[i] = curState[i];
                }
            }

            // Check if the hovered entity has changed with three options:
            // 1) We were hitting a collider A and now we're hitting a collider B
            if (IsColliderDifferent(state, colliderData)) {
                HandleColliderChanged(state, colliderData);

            // 2) We were hitting a collider A and now we're not hitting anything
            } else if (IsColliderMissing(state, colliderData))
            {
                HandleMissingCollider(state);

            // 3) We were not hitting anything and now we're hitting collider A
            } else if (IsColliderAvailable(state, colliderData)) {
                HandleAvailableCollider(state, colliderData);
            }

            if (colliderData != null)
            {
                var hoverEvents = state.pointerEvents.GetFor(colliderData.scene, colliderData.entity)?.model.PointerEvents;
                state.interactionHoverCanvas.ShowHoverTooltips(hoverEvents, curState, raycastHit.distance, isAnyButtonDown);
            }
        }
        private static bool IsColliderDifferent(State state, IECSReadOnlyComponentData<InternalColliders> colliderData)
        {
            return colliderData != null && // current collider
                   state.lastHoverFeedback.hasValue && // previous collider
                   (state.lastHoverFeedback.entityId != colliderData.entity.entityId ||
                    state.lastHoverFeedback.sceneNumber != colliderData.scene.sceneData.sceneNumber);
        }

        private static bool IsColliderMissing(State state, IECSReadOnlyComponentData<InternalColliders> colliderData)
        {
            return colliderData == null && state.lastHoverFeedback.hasValue;
        }

        private static bool IsColliderAvailable(State state, IECSReadOnlyComponentData<InternalColliders> colliderData)
        {
            return colliderData != null && !state.lastHoverFeedback.hasValue;
        }

        private static void HandleColliderChanged(State state, IECSReadOnlyComponentData<InternalColliders> colliderData) {
            DataStore_ECS7.RaycastEvent.Hit raycastHit = state.dataStoreEcs7.lastPointerRayHit.hit;
            Ray raycastRay = state.dataStoreEcs7.lastPointerRayHit.ray;

            if (state.worldState.ContainsScene(state.lastHoverFeedback.sceneNumber))
            {
                AddInputResultEvent(
                    state,
                    InputAction.IaPointer,
                    state.lastHoverFeedback.scene,
                    state.lastHoverFeedback.entityId,
                    raycastRay,
                    raycastHit,
                    PointerEventType.PetHoverLeave
                );
            }

            AddInputResultEvent(
                state,
                InputAction.IaPointer,
                colliderData.scene,
                colliderData.entity.entityId,
                raycastRay,
                raycastHit,
                PointerEventType.PetHoverEnter
            );

            state.interactionHoverCanvas.Hide();

            state.lastHoverFeedback.hasValue = true;
            state.lastHoverFeedback.sceneNumber = colliderData.scene.sceneData.sceneNumber;
            state.lastHoverFeedback.scene = colliderData.scene;
            state.lastHoverFeedback.entityId = colliderData.entity.entityId;
        }

        private static void HandleMissingCollider(State state) {
            DataStore_ECS7.RaycastEvent.Hit raycastHit = state.dataStoreEcs7.lastPointerRayHit.hit;
            Ray raycastRay = state.dataStoreEcs7.lastPointerRayHit.ray;

            if (state.worldState.ContainsScene(state.lastHoverFeedback.sceneNumber))
            {
                AddInputResultEvent(
                    state,
                    InputAction.IaPointer,
                    state.lastHoverFeedback.scene,
                    state.lastHoverFeedback.entityId,
                    raycastRay,
                    raycastHit,
                    PointerEventType.PetHoverLeave
                );
            }

            state.interactionHoverCanvas.Hide();
            state.lastHoverFeedback.hasValue = false;
        }

        private static void  HandleAvailableCollider(State state, IECSReadOnlyComponentData<InternalColliders> colliderData) {
            DataStore_ECS7.RaycastEvent.Hit raycastHit = state.dataStoreEcs7.lastPointerRayHit.hit;
            Ray raycastRay = state.dataStoreEcs7.lastPointerRayHit.ray;

            AddInputResultEvent(
                state,
                InputAction.IaPointer,
                colliderData.scene,
                colliderData.entity.entityId,
                raycastRay,
                raycastHit,
                PointerEventType.PetHoverEnter
            );

            state.lastHoverFeedback.hasValue = true;
            state.lastHoverFeedback.sceneNumber = colliderData.scene.sceneData.sceneNumber;
            state.lastHoverFeedback.scene = colliderData.scene;
            state.lastHoverFeedback.entityId = colliderData.entity.entityId;
        }

        private static void BroadcastInputResultEvent(State state, InputAction buttonId,
            long entityId, Ray ray, DataStore_ECS7.RaycastEvent.Hit raycastHit, PointerEventType pointerEventType, IParcelScene skipScene = null)
        {
            IReadOnlyList<IParcelScene> loadedScenes = state.dataStoreEcs7.scenes;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (loadedScenes[i] != skipScene)
                {
                    AddInputResultEvent(state, buttonId, loadedScenes[i], entityId, ray, raycastHit, pointerEventType);
                }
            }
        }

        private static void AddInputResultEvent(State state, InputAction buttonId, IParcelScene scene,
            long entityId, Ray ray, DataStore_ECS7.RaycastEvent.Hit raycastHit, PointerEventType pointerEventType)
        {
            raycastHit.point = WorldStateUtils.ConvertUnityToScenePosition(raycastHit.point, scene);
            ray.origin = WorldStateUtils.ConvertUnityToScenePosition(ray.origin, scene);

            state.inputResultComponent.AddEvent(scene, new InternalInputEventResults.EventData()
            {
                button = buttonId,
                hit = ProtoConvertUtils.ToPBRaycasHit(entityId, null,
                    ray, raycastHit.distance, raycastHit.point, raycastHit.normal, entityId != -1),
                type = pointerEventType
            });
        }

        private static IECSReadOnlyComponentData<InternalColliders> GetEntityWithCollider(
            this IInternalECSComponent<InternalColliders> pointerColliderComponent,
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

        private static void ShowHoverTooltips(this IECSInteractionHoverCanvas canvas,
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
    }
}
