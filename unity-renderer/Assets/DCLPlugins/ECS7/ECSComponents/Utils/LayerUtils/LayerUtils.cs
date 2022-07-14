using UnityEngine;

namespace DCL.CuloDeMono.Utils
{
    public static class LayerUtils
    {
        // Layers related to the components that can have physics. All models that have a withCollisions or isPointerBlocker will use them 
        public static readonly int onPointerEventLayer = LayerMask.NameToLayer("OnPointerEvent");
        public static readonly int onPointerEventWithCollisionsLayer = LayerMask.NameToLayer("OnPointerEventWithCollisions");
        // Note: when the old ecs get removed from the project, we should remove this layer and start using "Default"
        public static readonly int collisionsLayer = LayerMask.NameToLayer("Collisions");
        
        
        /// <summary>
        /// WithCollisions are properties from the shape and OnPointer
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="withCollisions"></param>
        /// <param name="isPointerBlocker"></param>
        /// <returns></returns>
        public static int CalculateLayerMask(long entityId, bool withCollisions, bool isPointerBlocker)
        {
            int colliderLayer;
            if (DataStore.i.ecs7.onPointerEventEntities.Contains(entityId))
                colliderLayer = withCollisions ? onPointerEventWithCollisionsLayer : onPointerEventLayer;
            else
                colliderLayer = isPointerBlocker ? onPointerEventLayer : collisionsLayer;

            return colliderLayer;
        }
    }
}