using DCL.Components;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public static class DCLVideoTextureUtils
    {
        public static float GetClosestDistanceSqr(ISharedComponent disposableComponent, Vector3 fromPosition)
        {
            float dist = int.MaxValue;

            if (disposableComponent.GetAttachedEntities().Count <= 0)
                return dist;

            using (var iterator = disposableComponent.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    IDCLEntity entity = iterator.Current;

                    if (IsEntityVisible(iterator.Current))
                    {
                        var entityDist = (entity.meshRootGameObject.transform.position - fromPosition).sqrMagnitude;
                        if (entityDist < dist)
                            dist = entityDist;
                    }
                }
            }

            return dist;
        }

        public static bool IsComponentVisible(ISharedComponent disposableComponent)
        {
            if (disposableComponent.GetAttachedEntities().Count <= 0)
                return false;

            using (var iterator = disposableComponent.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    bool isEntityVisible = IsEntityVisible(iterator.Current);

                    if (isEntityVisible)
                        return true;
                }
            }

            return false;
        }

        public static bool IsEntityVisible(IDCLEntity entity)
        {
            if (entity.meshesInfo == null)
                return false;
            
            if (entity.meshesInfo.currentShape == null)
                return false;
            
            return entity.meshesInfo.currentShape.IsVisible();
        }

        public static void UnsubscribeToEntityShapeUpdate(ISharedComponent component,
            System.Action<IDCLEntity> OnUpdate)
        {
            if (component.GetAttachedEntities().Count <= 0)
                return;

            using (var iterator = component.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var entity = iterator.Current;
                    entity.OnShapeUpdated -= OnUpdate;
                }
            }
        }

        public static void SubscribeToEntityUpdates(ISharedComponent component, System.Action<IDCLEntity> OnUpdate)
        {
            if (component.GetAttachedEntities().Count <= 0)
                return;

            using (var iterator = component.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var entity = iterator.Current;
                    entity.OnShapeUpdated -= OnUpdate;
                    entity.OnShapeUpdated += OnUpdate;
                }
            }
        }
    }
}