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

        public static void ConfigureColliders(GameObject meshGameObject, bool hasCollision, bool filterByColliderName = false)
        {
            if (meshGameObject == null) return;

            Collider collider;
            int onClickLayer = LayerMask.NameToLayer(OnPointerEventColliders.COLLIDER_LAYER); // meshes can have a child collider for the OnClick that should be ignored
            MeshFilter[] meshFilters = meshGameObject.GetComponentsInChildren<MeshFilter>(true);

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject.layer == onClickLayer) continue;

                if (filterByColliderName)
                {
                    if (!meshFilters[i].transform.parent.name.ToLower().Contains("_collider")) continue;

                    // we remove the Renderer of the '_collider' object, as its true renderer is in another castle
                    GameObject.Destroy(meshFilters[i].GetComponent<Renderer>());
                }

                collider = meshFilters[i].GetComponent<Collider>();

                if (hasCollision)
                {
                    if (collider == null)
                        collider = meshFilters[i].gameObject.AddComponent<MeshCollider>();

                    if (collider is MeshCollider)
                        ((MeshCollider)collider).sharedMesh = meshFilters[i].sharedMesh;
                }

                if (collider != null)
                    collider.enabled = hasCollision || filterByColliderName;
            }
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
