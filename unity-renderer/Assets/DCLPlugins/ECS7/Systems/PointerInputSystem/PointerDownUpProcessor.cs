using System.Collections.Generic;
using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using UnityEngine;

namespace ECSSystems.PointerInputSystem
{
    internal readonly struct PointerInputResult
    {
        public readonly long entityId;
        public readonly string sceneId;
        public readonly bool hasValue;

        public PointerInputResult(string sceneId, long entityId, bool hasValue = true)
        {
            this.entityId = entityId;
            this.sceneId = sceneId;
            this.hasValue = hasValue;
        }

        public static PointerInputResult empty => new PointerInputResult(null, -1, false);
    }

    internal static class PointerDownUpProcessor
    {
        public static PointerInputResult ProcessPointerDownUp(DataStore_ECS7.PointerEvent pointerEvent,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            PointerInputResult lastPointerDownEntity)
        {
            if (pointerEvent.isButtonDown)
            {
                return GetPointerDownEntity(pointerDownGroup, pointerEvent);
            }

            PointerInputResult pointerUp = GetPointerUpEntity(pointerUpGroup, pointerEvent);

            if (lastPointerDownEntity.hasValue && lastPointerDownEntity.Equals(pointerUp))
            {
                return pointerUp;
            }

            return PointerInputResult.empty;
        }

        private static PointerInputResult GetPointerDownEntity(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            DataStore_ECS7.PointerEvent pointerEvent)
        {
            var componentGroup = pointerDownGroup.group;
            for (int i = 0; i < componentGroup.Count; i++)
            {
                PBOnPointerDown pointerDown = componentGroup[i].componentData2.model;
                IList<Collider> entityColliders = componentGroup[i].componentData1.model.colliders;

                if (!PointerInputHelper.IsInputForEntity(pointerEvent.rayResult.hitInfo.hit.collider, entityColliders))
                    continue;

                if (!PointerInputHelper.IsValidInputForEntity(pointerEvent.buttonId,
                    pointerEvent.rayResult.hitInfo.hit.distance, pointerDown.GetMaxDistance(), pointerDown.GetButton()))
                    return PointerInputResult.empty;

                return new PointerInputResult(componentGroup[i].scene.sceneData.id, componentGroup[i].entity.entityId);
            }
            return PointerInputResult.empty;
        }

        private static PointerInputResult GetPointerUpEntity(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            DataStore_ECS7.PointerEvent pointerEvent)
        {
            var componentGroup = pointerUpGroup.group;
            for (int i = 0; i < componentGroup.Count; i++)
            {
                PBOnPointerUp pointerUp = componentGroup[i].componentData2.model;
                IList<Collider> entityColliders = componentGroup[i].componentData1.model.colliders;

                if (!PointerInputHelper.IsInputForEntity(pointerEvent.rayResult.hitInfo.hit.collider, entityColliders))
                    continue;

                if (!PointerInputHelper.IsValidInputForEntity(pointerEvent.buttonId,
                    pointerEvent.rayResult.hitInfo.hit.distance, pointerUp.GetMaxDistance(), pointerUp.GetButton()))
                    return PointerInputResult.empty;

                return new PointerInputResult(componentGroup[i].scene.sceneData.id, componentGroup[i].entity.entityId);
            }
            return PointerInputResult.empty;
        }
    }
}