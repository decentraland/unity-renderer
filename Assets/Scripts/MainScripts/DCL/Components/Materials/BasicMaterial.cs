using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class BasicMaterial : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string texture;

            [Range(0f, 1f)]
            public float
                alphaTest = 0.5f; // value that defines if a pixel is visible or invisible (no transparency gradients)
        }

        public Model model = new Model();
        public Material material;

        private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int _AlphaClip = Shader.PropertyToID("_AlphaClip");

        public BasicMaterial(ParcelScene scene) : base(scene)
        {
            material = new Material(Utils.EnsureResourcesMaterial("Materials/BasicShapeMaterial"));

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            entity.RemoveSharedComponent(typeof(PBRMaterial));

            base.AttachTo(entity);
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (material == null)
            {
                yield break; // We escape ApplyChanges called in the parent's constructor
            }

            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.texture))
            {
                yield return DCLTexture.FetchFromComponent(scene, model.texture, (downloadedTexture) =>
                {
                    material.SetTexture(_BaseMap, downloadedTexture);
                });
            }
            else
            {
                material.mainTexture = null;
            }

            material.SetInt("_ZWrite", 1);
            material.SetFloat(_AlphaClip, 1);
            material.SetFloat("_Cutoff", model.alphaTest);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

        }

        void OnMaterialAttached(DecentralandEntity entity)
        {
            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            if (entity.meshRootGameObject != null)
            {
                var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                {
                    InitMaterial(entity.meshRootGameObject);
                }
            }
        }

        void InitMaterial(GameObject meshGameObject)
        {
            if (meshGameObject == null)
            {
                return;
            }

            var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null && meshRenderer.sharedMaterial != material)
            {
                MaterialTransitionController
                    matTransition = meshGameObject.GetComponent<MaterialTransitionController>();

                if (matTransition != null && matTransition.canSwitchMaterial)
                {
                    matTransition.finalMaterials = new Material[] { material };
                    matTransition.PopulateLoadingMaterialWithFinalMaterial();
                }

                meshRenderer.sharedMaterial = material;
            }
        }

        private void OnShapeUpdated(DecentralandEntity entity)
        {
            if (entity != null)
            {
                InitMaterial(entity.meshRootGameObject);
            }
        }

        void OnMaterialDetached(DecentralandEntity entity)
        {
            if (entity.meshRootGameObject == null)
            {
                return;
            }

            entity.OnShapeUpdated -= OnShapeUpdated;

            var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer && meshRenderer.sharedMaterial == material)
            {
                meshRenderer.sharedMaterial = null;
            }
        }

        public override void Dispose()
        {
            if (material != null)
            {
                GameObject.Destroy(material);
            }

            base.Dispose();
        }
    }
}
