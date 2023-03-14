using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalCollidersExtensions
    {
        public static void AddCollider(this IInternalECSComponent<InternalColliders> colliderInternalComponent,
            IParcelScene scene, IDCLEntity entity, Collider collider)
        {
            if (collider is null)
                return;

            if (colliderInternalComponent.HasCollider(scene, entity, collider))
                return;

            var model = colliderInternalComponent.GetFor(scene, entity)?.model ?? new InternalColliders();
            model.colliders.Add(collider);
            colliderInternalComponent.PutFor(scene, entity, model);
        }

        public static void AddColliders(this IInternalECSComponent<InternalColliders> colliderInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Collider> colliders)
        {
            var model = colliderInternalComponent.GetFor(scene, entity)?.model ?? new InternalColliders();

            for (int i = 0; i < colliders.Count; i++)
            {
                Collider collider = colliders[i];

                if (!collider)
                    continue;

                if (model.colliders.Contains(collider))
                    continue;

                model.colliders.Add(collider);
            }

            if (model.colliders.Count > 0)
            {
                colliderInternalComponent.PutFor(scene, entity, model);
            }
        }

        public static bool HasCollider(this IInternalECSComponent<InternalColliders> colliderInternalComponent,
            IParcelScene scene, IDCLEntity entity, Collider collider)
        {
            if (collider is null)
                return false;

            var compData = colliderInternalComponent.GetFor(scene, entity);

            if (compData != null)
            {
                return compData.model.colliders.Contains(collider);
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
