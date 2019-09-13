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
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            //NOTE(Brian): This assignation must be before the base.AttachTo call, or the OnAttach events will receive entity.currentShape == null
            //             and fail to generate colliders randomly.
            entity.currentShape = this;
            base.AttachTo(entity, typeof(BaseShape));
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity))
            {
                return;
            }

            // In case the character controller has been parented to this entity's mesh
            if(entity.meshGameObject != null &&  DCLCharacterController.i.transform.parent == entity.meshGameObject.transform)
            {
                DCLCharacterController.i.transform.SetParent(null);
            }

            // We do this instead of OnDetach += because it is required to run after every OnDetach listener
            entity.currentShape = null;

            base.DetachFrom(entity, typeof(BaseShape));
        }

        public static void ConfigureVisibility(GameObject meshGameObject, bool isVisible)
        {
            if (meshGameObject == null)
            {
                return;
            }

            if (!isVisible)
            {
                MaterialTransitionController[] materialTransitionControllers = meshGameObject.GetComponentsInChildren<MaterialTransitionController>();

                for (var i = 0; i < materialTransitionControllers.Length; i++)
                {
                    GameObject.Destroy(materialTransitionControllers[i]);
                }
            }

            Renderer[] renderers = meshGameObject.GetComponentsInChildren<Renderer>(true);
            Collider onPointerEventCollider;
            int onClickLayer = LayerMask.NameToLayer(OnPointerEventColliders.COLLIDER_LAYER);

            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = isVisible;

                if (renderers[i].transform.childCount > 0)
                {
                    onPointerEventCollider = renderers[i].transform.GetChild(0).GetComponent<Collider>();

                    if (onPointerEventCollider != null && onPointerEventCollider.gameObject.layer == onClickLayer)
                        onPointerEventCollider.enabled = isVisible;
                }
            }
        }
    }
}
