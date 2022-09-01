using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace ECSSystems.PointerInputSystem
{
    internal readonly struct PointerInputResult
    {
        public readonly long entityId;
        public readonly string sceneId;
        public readonly bool hasValue;
        public readonly bool hasEventComponent;

        public PointerInputResult(string sceneId, long entityId, bool hasEventComponent, bool hasValue = true)
        {
            this.entityId = entityId;
            this.sceneId = sceneId;
            this.hasValue = hasValue;
            this.hasEventComponent = hasEventComponent;
        }

        public static PointerInputResult empty => new PointerInputResult(null, -1, false, false);
    }

    internal static class PointerDownUpProcessor
    {
        public static PointerInputResult ProcessPointerDownUp(
            DataStore_ECS7.PointerEvent pointerEvent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            ECSComponent<PBOnPointerDown> pointerDownComponent,
            ECSComponent<PBOnPointerUp> pointerUpComponent,
            PointerInputResult lastPointerDownResult)
        {
            if (pointerEvent.isButtonDown)
            {
                return GetPointerDownEntity(pointerDownComponent, pointerColliderComponent, pointerEvent);
            }

            PointerInputResult pointerUp = GetPointerUpEntity(pointerUpComponent, pointerColliderComponent, pointerEvent);

            if (lastPointerDownResult.IsSameEntity(pointerUp))
            {
                return pointerUp;
            }

            return PointerInputResult.empty;
        }

        private static PointerInputResult GetPointerDownEntity(
            ECSComponent<PBOnPointerDown> pointerDownComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            DataStore_ECS7.PointerEvent pointerEvent)
        {
            var colliders = pointerColliderComponent.GetForAll();
            for (int i = 0; i < colliders.Count; i++)
            {
                ECSComponentData<InternalColliders> collider = colliders[i].value;
                if (!collider.model.colliders.Contains(pointerEvent.rayResult.hitInfo.hit.collider))
                    continue;

                var pointerDownData = pointerDownComponent.Get(collider.scene, collider.entity);
                if (pointerDownData == null)
                    return new PointerInputResult(collider.scene.sceneData.id, collider.entity.entityId, false);

                if (PointerInputHelper.IsValidInputForEntity(pointerEvent.buttonId,
                    pointerEvent.rayResult.hitInfo.hit.distance,
                    pointerDownData.model.GetMaxDistance(),
                    pointerDownData.model.GetButton()))
                {
                    return new PointerInputResult(collider.scene.sceneData.id, collider.entity.entityId, true);
                }
                return new PointerInputResult(collider.scene.sceneData.id, collider.entity.entityId, false);
            }
            return PointerInputResult.empty;
        }

        private static PointerInputResult GetPointerUpEntity(
            ECSComponent<PBOnPointerUp> pointerUpComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            DataStore_ECS7.PointerEvent pointerEvent)
        {
            var colliders = pointerColliderComponent.GetForAll();
            for (int i = 0; i < colliders.Count; i++)
            {
                ECSComponentData<InternalColliders> collider = colliders[i].value;
                if (!collider.model.colliders.Contains(pointerEvent.rayResult.hitInfo.hit.collider))
                    continue;

                var pointerUpData = pointerUpComponent.Get(collider.scene, collider.entity);
                if (pointerUpData == null)
                    return new PointerInputResult(collider.scene.sceneData.id, collider.entity.entityId, false);

                if (PointerInputHelper.IsValidInputForEntity(pointerEvent.buttonId,
                    pointerEvent.rayResult.hitInfo.hit.distance,
                    pointerUpData.model.GetMaxDistance(),
                    pointerUpData.model.GetButton()))
                {
                    return new PointerInputResult(collider.scene.sceneData.id, collider.entity.entityId, true);
                }
                return new PointerInputResult(collider.scene.sceneData.id, collider.entity.entityId, false);
            }
            return PointerInputResult.empty;
        }

        private static bool IsSameEntity(this PointerInputResult self, PointerInputResult other)
        {
            if (!self.hasValue || !other.hasValue)
                return false;
            return self.entityId == other.entityId && self.sceneId == other.sceneId;
        }
    }
}