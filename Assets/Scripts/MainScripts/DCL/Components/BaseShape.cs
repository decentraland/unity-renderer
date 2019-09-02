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

            base.AttachTo(entity, typeof(BaseShape));
            entity.currentShape = this;
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity))
            {
                return;
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
