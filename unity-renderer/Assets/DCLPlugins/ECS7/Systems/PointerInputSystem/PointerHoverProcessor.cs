using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using UnityEngine;

namespace ECSSystems.PointerInputSystem
{
    internal readonly struct PointerHoverResult
    {
        public readonly long entityId;
        public readonly string sceneId;
        public readonly bool hasValue;
        public readonly bool hasFeedback;
        public readonly ActionButton buttonId;
        public readonly string text;

        public PointerHoverResult(string sceneId, long entityId, bool hasFeedback, ActionButton buttonId, string text, bool hasValue = true)
        {
            this.entityId = entityId;
            this.sceneId = sceneId;
            this.hasValue = hasValue;
            this.hasFeedback = hasFeedback;
            this.buttonId = buttonId;
            this.text = text;
        }

        public static PointerHoverResult empty =>
            new PointerHoverResult(null, -1, false, ActionButton.Any, null, false);
    }

    internal static class PointerHoverProcessor
    {
        public static PointerHoverResult ProcessPointerHover(bool isPointerDown, Collider hitCollider, float hitDistance,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup)
        {
            if (isPointerDown)
            {
                return GetPointerUpEntity(pointerUpGroup, hitCollider, hitDistance);
            }
            return GetPointerDownEntity(pointerDownGroup, hitCollider, hitDistance);
        }

        private static PointerHoverResult GetPointerDownEntity(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            Collider hitCollider, float hitDistance)
        {
            var componentGroup = pointerDownGroup.group;
            for (int i = 0; i < componentGroup.Count; i++)
            {
                PBOnPointerDown pointerDown = componentGroup[i].componentData2.model;
                IList<Collider> entityColliders = componentGroup[i].componentData1.model.colliders;

                if (!PointerInputHelper.IsInputForEntity(hitCollider, entityColliders))
                    continue;

                if (!PointerInputHelper.IsValidDistanceForEntity(hitDistance, pointerDown.GetMaxDistance()))
                    return PointerHoverResult.empty;

                if (!pointerDown.GetShowFeedback())
                    return PointerHoverResult.empty;

                return new PointerHoverResult(componentGroup[i].scene.sceneData.id, componentGroup[i].entity.entityId,
                    pointerDown.GetShowFeedback(), pointerDown.GetButton(), pointerDown.GetHoverText());
            }
            return PointerHoverResult.empty;
        }

        private static PointerHoverResult GetPointerUpEntity(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            Collider hitCollider, float hitDistance)
        {
            var componentGroup = pointerUpGroup.group;
            for (int i = 0; i < componentGroup.Count; i++)
            {
                PBOnPointerUp pointerUp = componentGroup[i].componentData2.model;
                IList<Collider> entityColliders = componentGroup[i].componentData1.model.colliders;

                if (!PointerInputHelper.IsInputForEntity(hitCollider, entityColliders))
                    continue;

                if (!PointerInputHelper.IsValidDistanceForEntity(hitDistance, pointerUp.GetMaxDistance()))
                    return PointerHoverResult.empty;

                if (!pointerUp.GetShowFeedback())
                    return PointerHoverResult.empty;

                return new PointerHoverResult(componentGroup[i].scene.sceneData.id, componentGroup[i].entity.entityId,
                    pointerUp.GetShowFeedback(), pointerUp.GetButton(), pointerUp.GetHoverText());
            }
            return PointerHoverResult.empty;
        }
    }
}