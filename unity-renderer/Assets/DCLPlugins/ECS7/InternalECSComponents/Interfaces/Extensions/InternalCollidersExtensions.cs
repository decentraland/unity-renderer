using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalCollidersExtensions
    {
        public static void AddCollider(this IInternalECSComponent<InternalColliders> colliderInternalComponent,
            IParcelScene scene, IDCLEntity entity, Collider collider, uint colliderLayer)
        {
            if (collider is null)
                return;

            if (colliderInternalComponent.HasCollider(scene, entity, collider))
                return;

            var model = colliderInternalComponent.GetFor(scene, entity)?.model ?? new InternalColliders();
            model.colliders.Add(collider, colliderLayer);
            colliderInternalComponent.PutFor(scene, entity, model);
        }

        public static bool HasCollider(this IInternalECSComponent<InternalColliders> colliderInternalComponent,
            IParcelScene scene, IDCLEntity entity, Collider collider)
        {
            if (collider is null)
                return false;

            var compData = colliderInternalComponent.GetFor(scene, entity);
            if (compData != null)
            {
                return compData.model.colliders.ContainsKey(collider);
            }

            return false;
        }

        public static bool RemoveCollider(this IInternalECSComponent<InternalColliders> colliderInternalComponent,
            IParcelScene scene, IDCLEntity entity, Collider collider)
        {
            if (collider is null)
                return false;

            var compData = colliderInternalComponent.GetFor(scene, entity);

            if (compData == null)
                return false;

            bool ret = compData.model.colliders.Remove(collider);

            if (ret && compData.model.colliders.Count == 0)
            {
                colliderInternalComponent.RemoveFor(scene, entity, new InternalColliders());
            }

            return ret;
        }
    }
}
