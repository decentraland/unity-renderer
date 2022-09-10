using System;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using Ray = UnityEngine.Ray;
using RaycastHit = UnityEngine.RaycastHit;

namespace ECSSystems.PointerInputSystem
{
    public static class ECSPointerInputSystem
    {
        private class State
        {
            public IInternalECSComponent<InternalColliders> pointerColliderComponent;
            public IInternalECSComponent<InternalInputEventResults> inputResultComponent;
            public DataStore_ECS7 dataStoreEcs7;
            public bool isLastInputPointerDown;
            public IWorldState worldState;
            public EntityInput lastInputDown;
            public EntityInput lastInputHover;
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
            IWorldState worldState,
            DataStore_ECS7 dataStoreEcs)
        {
            var state = new State()
            {
                pointerColliderComponent = pointerColliderComponent,
                inputResultComponent = inputResultComponent,
                isLastInputPointerDown = false,
                dataStoreEcs7 = dataStoreEcs,
                worldState = worldState,
                lastInputDown = new EntityInput() { hasValue = false },
                lastInputHover = new EntityInput() { hasValue = false },
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            DataStore_ECS7.PointerEvent currentPointerInput = state.dataStoreEcs7.lastPointerInputEvent;

            bool isHit = state.dataStoreEcs7.lastPointerRayHit.hasValue && state.dataStoreEcs7.lastPointerRayHit.didHit;
            bool isPointerDown = currentPointerInput.hasValue && currentPointerInput.isButtonDown;
            bool isPointerUp = state.isLastInputPointerDown && currentPointerInput.hasValue && !currentPointerInput.isButtonDown;

            RaycastHit raycastHit = state.dataStoreEcs7.lastPointerRayHit.hit;
            Ray raycastRay = state.dataStoreEcs7.lastPointerRayHit.ray;

            IECSReadOnlyComponentData<InternalColliders> colliderData = isHit
                ? state.pointerColliderComponent
                       .GetEntityWithCollider(raycastHit.collider)
                : null;

            bool isRaycastHitValidEntity = colliderData != null;
            bool isHoveringInput = !isPointerDown && !isPointerUp && !state.isLastInputPointerDown;
            bool isHoveringExit = !isRaycastHitValidEntity && state.lastInputHover.hasValue;

            InputHitType inputHitType = InputHitType.None;

            if (isRaycastHitValidEntity)
            {
                inputHitType = isPointerDown ? InputHitType.PointerDown
                    : isPointerUp ? InputHitType.PointerUp
                    : isHoveringInput ? InputHitType.PointerHover
                    : InputHitType.None;
            }

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
                            raycastRay, raycastHit),
                        type = PointerEventType.Down
                    });
                    Debug.Log($"pointerdown {colliderData.entity.entityId}");
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
                                raycastRay, raycastHit),
                            type = PointerEventType.Up
                        });
                        Debug.Log($"pointerup {lastInputDownData.entity.entityId}");
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
                                    raycastRay, raycastHit, false),
                                type = PointerEventType.Up
                            });
                            Debug.Log($"pointerup {lastInputDownData.entity.entityId}");
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
                        bool isValidScene = state.worldState.ContainsScene(state.lastInputHover.sceneId);

                        if (isValidScene)
                        {
                            state.inputResultComponent.AddEvent(state.lastInputHover.scene, new InternalInputEventResults.EventData()
                            {
                                analog = 1,
                                button = (ActionButton)currentPointerInput.buttonId,
                                hit = ProtoConvertUtils.ToPBRaycasHit(state.lastInputHover.entity.entityId, null,
                                    raycastRay, raycastHit),
                                type = PointerEventType.HoverLeave
                            });
                            Debug.Log($"hoverexit {state.lastInputHover.entity.entityId}");
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
                                raycastRay, raycastHit),
                            type = PointerEventType.HoverEnter
                        });
                        Debug.Log($"hoverenter {colliderData.entity.entityId}");
                    }
                    break;
            }

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
                                    raycastRay, raycastHit, false),
                                type = PointerEventType.Up
                            });
                            Debug.Log($"pointerup {state.lastInputDown.entity.entityId}");
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
                                raycastRay, raycastHit),
                            type = PointerEventType.HoverLeave
                        });
                        Debug.Log($"hoverexit {state.lastInputHover.entity.entityId}");
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
    }
}