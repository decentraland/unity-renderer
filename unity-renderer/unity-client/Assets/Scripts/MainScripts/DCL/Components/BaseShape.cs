using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseShape : BaseDisposable, IShape
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public bool withCollisions = true;
            public bool isPointerBlocker = true;
            public bool visible = true;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        public BaseShape()
        {
            model = new Model();
        }

        new public Model GetModel()
        {
            return (Model) model;
        }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity)) return;

            //NOTE(Brian): This assignation must be before the base.AttachTo call, or the OnAttach events will receive entity.currentShape == null
            //             and fail to generate colliders randomly.
            entity.meshesInfo.currentShape = this;

            base.AttachTo(entity, typeof(BaseShape));
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity)) return;

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

            for (var i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = shouldBeVisible;

                if (meshRenderers[i].transform.childCount > 0)
                {
                    onPointerEventCollider = meshRenderers[i].transform.GetChild(0).GetComponent<Collider>();

                    if (onPointerEventCollider != null && onPointerEventCollider.gameObject.layer == PhysicsLayers.onPointerEventLayer)
                        onPointerEventCollider.enabled = shouldBeVisible;
                }
            }
        }

        protected int CalculateCollidersLayer(Model model)
        {
            // We can't enable this layer changer logic until we redeploy all the builder and street scenes with the corrected 'withCollision' default in true...
            /* if (!model.withCollisions && model.isPointerBlocker)
                return PhysicsLayers.onPointerEventLayer;
            else */
            if (model.withCollisions && !model.isPointerBlocker)
                return PhysicsLayers.characterOnlyLayer;

            return PhysicsLayers.defaultLayer;
        }
    }
}