using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

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

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel) =>
                default;
        }

        protected BaseShape()
        {
            model = new Model();
        }

        public new Model GetModel() =>
            (Model) model;

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
                return;

            //NOTE(Brian): This assignation must be before the base.AttachTo call, or the OnAttach events will receive entity.currentShape == null
            //             and fail to generate colliders randomly.
            entity.meshesInfo.currentShape = this;

            base.AttachTo(entity, typeof(BaseShape));
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity))
                return;

            // We do this instead of OnDetach += because it is required to run after every OnDetach listener
            entity.meshesInfo.currentShape = null;

            base.DetachFrom(entity, typeof(BaseShape));

            entity.OnShapeUpdated?.Invoke(entity);
        }

        public virtual bool IsVisible() { return false; }

        public virtual bool HasCollisions() { return false; }

        public static void ConfigureVisibility(GameObject meshGameObject, bool shouldBeVisible, Renderer[] meshRenderers = null)
        {
            if (meshGameObject == null)
                return;

            if (meshRenderers == null)
                meshRenderers = meshGameObject.GetComponentsInChildren<Renderer>(true);

            for (var i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] == null)
                    continue;

                meshRenderers[i].enabled = shouldBeVisible;
                Transform meshRendererT = meshRenderers[i].transform;

                for (int j = 0; j < meshRendererT.transform.childCount; j++)
                {
                    Collider onPointerEventCollider = meshRendererT.GetChild(j).GetComponent<Collider>();

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
