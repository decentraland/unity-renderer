using System;
using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using UnityEngine;

namespace ECSSystems.PointerInputSystem
{
    public static class ECSPointerInputSystem
    {
        private class State
        {
            public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup;
            public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup;
            public DataStore_ECS7 dataStoreEcs7;
            public long? lastPointerDownEntityId;
        }

        public static Action CreateSystem(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            DataStore_ECS7 dataStoreEcs)
        {
            var state = new State()
            {
                pointerDownGroup = pointerDownGroup,
                pointerUpGroup = pointerUpGroup,
                dataStoreEcs7 = dataStoreEcs,
                lastPointerDownEntityId = null,
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            if (state.dataStoreEcs7.lastPointerInputEvent.HasValue)
            {
                var input = state.dataStoreEcs7.lastPointerInputEvent.Value;

                long entityId;

                if (input.isButtonDown)
                {
                    if (GetPointerDownEntity(state.pointerDownGroup, input, out entityId))
                    {
                        state.lastPointerDownEntityId = entityId;
                        // TODO: add pointer down result
                        Debug.Log($"down {entityId}");
                    }
                    else
                    {
                        state.lastPointerDownEntityId = null;
                    }
                }
                else
                {
                    if (GetPointerUpEntity(state.pointerUpGroup, input, out entityId))
                    {
                        if (state.lastPointerDownEntityId.HasValue && state.lastPointerDownEntityId.Value == entityId)
                        {
                            // TODO: add pointer up result
                            Debug.Log($"up {entityId}");
                        }
                    }
                    state.lastPointerDownEntityId = null;
                }

                state.dataStoreEcs7.lastPointerInputEvent = null;
            }
        }

        private static bool GetPointerDownEntity(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            DataStore_ECS7.PointerEvent pointerEvent, out long entityPressed)
        {
            var componentGroup = pointerDownGroup.group;
            for (int i = 0; i < componentGroup.Count; i++)
            {
                if (!componentGroup[i].componentData1.model.colliders.Contains(pointerEvent.rayResult.hitInfo.hit.collider))
                    continue;

                PBOnPointerDown pointerDown = componentGroup[i].componentData2.model;
                if (pointerEvent.rayResult.hitInfo.hit.distance > pointerDown.GetMaxDistance())
                    continue;

                var expectedButton = pointerDown.GetButton();
                if (expectedButton == ActionButton.Any || (int)expectedButton == pointerEvent.buttonId)
                {
                    entityPressed = componentGroup[i].entity.entityId;
                    return true;
                }
            }
            entityPressed = 0;
            return false;
        }

        private static bool GetPointerUpEntity(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            DataStore_ECS7.PointerEvent pointerEvent, out long entityPressed)
        {
            var componentGroup = pointerUpGroup.group;
            for (int i = 0; i < componentGroup.Count; i++)
            {
                if (!componentGroup[i].componentData1.model.colliders.Contains(pointerEvent.rayResult.hitInfo.hit.collider))
                    continue;

                PBOnPointerUp pointerUp = componentGroup[i].componentData2.model;
                if (pointerEvent.rayResult.hitInfo.hit.distance > pointerUp.GetMaxDistance())
                    continue;

                var expectedButton = pointerUp.GetButton();
                if (expectedButton == ActionButton.Any || (int)expectedButton == pointerEvent.buttonId)
                {
                    entityPressed = componentGroup[i].entity.entityId;
                    return true;
                }
            }
            entityPressed = 0;
            return false;
        }
    }
}