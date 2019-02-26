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

            [Range(1, 3)]
            public int samplingMode = 2;  // 1: NEAREST; 2: BILINEAR; 3: TRILINEAR

            [Range(1, 3)]
            public int wrap = 0;          // 1: CLAMP; 2: WRAP; 3: MIRROR

            [Range(0f, 1f)]
            public float alphaTest = 0.5f; // value that defines if a pixel is visible or invisible (no transparency gradients)
        }

        public Model model = new Model();
        public override string componentName => "material";
        public Material material;
        bool isLoadingTexture = false;

        public BasicMaterial(ParcelScene scene) : base(scene)
        {
            material = new Material(Utils.EnsureResourcesMaterial("Materials/BasicShapeMaterial"));

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;

            isLoadingTexture = false;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (material == null) yield break; // We escape ApplyChanges called in the parent's constructor

            model = JsonUtility.FromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.texture) && !isLoadingTexture)
            {
                if (scene.sceneData.HasContentsUrl(model.texture))
                {
                    isLoadingTexture = true;
                    yield return Utils.FetchTexture(scene.sceneData.GetContentsUrl(model.texture), InitTexture);
                }
            }

            material.SetFloat("_AlphaClip", model.alphaTest);
        }

        void InitTexture(Texture texture)
        {
            isLoadingTexture = false;
            material.mainTexture = texture;

            // WRAP MODE CONFIGURATION
            switch (model.wrap)
            {
                case 2:
                    material.mainTexture.wrapMode = TextureWrapMode.Repeat;
                    break;
                case 3:
                    material.mainTexture.wrapMode = TextureWrapMode.Mirror;
                    break;
                default:
                    material.mainTexture.wrapMode = TextureWrapMode.Clamp;
                    break;
            }

            // SAMPLING/FILTER MODE CONFIGURATION
            switch (model.samplingMode)
            {
                case 2:
                    material.mainTexture.filterMode = FilterMode.Bilinear;
                    break;
                case 3:
                    material.mainTexture.filterMode = FilterMode.Trilinear;
                    break;
                default:
                    material.mainTexture.filterMode = FilterMode.Point;
                    break;
            }
        }

        void OnMaterialAttached(DecentralandEntity entity)
        {
            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            if (entity.meshGameObject != null)
            {
                var meshRenderer = entity.meshGameObject.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                {
                    InitMaterial(entity.meshGameObject);
                }
            }
        }

        void InitMaterial(GameObject meshGameObject)
        {
            if (meshGameObject == null)
                return;

            var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null && meshRenderer.sharedMaterial != material)
            {
                MaterialTransitionController matTransition = meshGameObject.GetComponent<MaterialTransitionController>();

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
                InitMaterial(entity.meshGameObject);
            }
        }

        void OnMaterialDetached(DecentralandEntity entity)
        {
            if (entity.meshGameObject == null)
            {
                return;
            }

            entity.OnShapeUpdated -= OnShapeUpdated;

            var meshRenderer = entity.meshGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer && meshRenderer.sharedMaterial == material)
            {
                meshRenderer.sharedMaterial = null;
            }
        }

        public override void Dispose()
        {
            if (material != null)
                GameObject.Destroy(material);

            base.Dispose();
        }
    }
}
