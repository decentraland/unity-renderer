using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseShape : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public bool withCollisions = true;
            public bool visible = true;
        }

        public BaseShape(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity)) return;

            //NOTE(Brian): This assignation must be before the base.AttachTo call, or the OnAttach events will receive entity.currentShape == null
            //             and fail to generate colliders randomly.
            entity.meshesInfo.currentShape = this;

            base.AttachTo(entity, typeof(BaseShape));
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity)) return;

            if (DCLCharacterController.i != null)
            {
                // In case the character controller has been parented to this entity's mesh
                if (entity.meshRootGameObject != null && DCLCharacterController.i.transform.parent == entity.meshRootGameObject.transform)
                {
                    DCLCharacterController.i.ResetGround();
                }
            }

            // We do this instead of OnDetach += because it is required to run after every OnDetach listener
            entity.meshesInfo.currentShape = null;

            base.DetachFrom(entity, typeof(BaseShape));
        }

        public virtual bool IsVisible()
        {
            return false;
        }

        public virtual bool HasCollisions()
        {
            return false;
        }

        public static void ConfigureVisibility(GameObject meshGameObject, bool shouldBeVisible, Renderer[] meshRenderers = null)
        {
            if (meshGameObject == null) return;

            if (!shouldBeVisible)
            {
                MaterialTransitionController[] materialTransitionControllers = meshGameObject.GetComponentsInChildren<MaterialTransitionController>();

                for (var i = 0; i < materialTransitionControllers.Length; i++)
                {
                    GameObject.Destroy(materialTransitionControllers[i]);
                }
            }

            if (meshRenderers == null)
                meshRenderers = meshGameObject.GetComponentsInChildren<Renderer>(true);

            Collider onPointerEventCollider;
            int onClickLayer = LayerMask.NameToLayer(OnPointerEventColliders.COLLIDER_LAYER);

            for (var i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = shouldBeVisible;

                if (meshRenderers[i].transform.childCount > 0)
                {
                    onPointerEventCollider = meshRenderers[i].transform.GetChild(0).GetComponent<Collider>();

                    if (onPointerEventCollider != null && onPointerEventCollider.gameObject.layer == onClickLayer)
                        onPointerEventCollider.enabled = shouldBeVisible;
                }
            }
        }
    }
}
