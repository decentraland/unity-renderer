using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class PBRMaterial : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            [Range(0f, 1f)]
            public float alphaTest = 0.5f;

            public Color albedoColor = Color.white;
            public string albedoTexture;
            public Color ambientColor = Color.white;
            public float metallic = 0.5f;
            public float roughness = 0.5f;
            public float microSurface = 1f; // Glossiness
            public float specularIntensity = 1f;

            public string alphaTexture;
            public string emissiveTexture;
            public Color emissiveColor = Color.black;
            public float emissiveIntensity = 2f;
            public Color reflectionColor = Color.white; // Specular color
            public Color reflectivityColor = Color.white;
            public float directIntensity = 1f;
            public float environmentIntensity = 1f;
            public string bumpTexture;
            public string refractionTexture;
            public bool disableLighting = false;

            [Range(0, 4)]
            public int transparencyMode = 4; // 0: OPAQUE; 1: ALPHATEST; 2: ALPHBLEND; 3: ALPHATESTANDBLEND; 4: AUTO (Engine decide)
        }

        enum TransparencyMode
        {
            OPAQUE,
            ALPHA_TEST,
            ALPHA_BLEND,
            ALPHA_TEST_AND_BLEND,
            AUTO
        }

        public Model model = new Model();
        public Material material { get; set; }

        const string MATERIAL_RESOURCES_PATH = "Materials/";
        const string BASIC_MATERIAL_NAME = "BasicShapeMaterial";
        const string PBR_MATERIAL_NAME = "ShapeMaterial";

        DCLTexture albedoDCLTexture = null;
        DCLTexture emissiveDCLTexture = null;
        DCLTexture bumpDCLTexture = null;

        public PBRMaterial(ParcelScene scene) : base(scene)
        {
            model = new Model();

            LoadMaterial("ShapeMaterial");

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            entity.RemoveSharedComponent(typeof(BasicMaterial));

            base.AttachTo(entity);
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (model.disableLighting)
            {
                LoadMaterial(BASIC_MATERIAL_NAME);
            }
            else
            {
                LoadMaterial(PBR_MATERIAL_NAME);

                material.SetColor("_BaseColor", model.albedoColor);

                if (model.emissiveColor != Color.clear && model.emissiveColor != Color.black)
                {
                    material.EnableKeyword("_EMISSION");
                }

                // METALLIC/SPECULAR CONFIGURATIONS
                material.SetColor("_EmissionColor", model.emissiveColor * model.emissiveIntensity);
                material.SetColor("_SpecColor", model.reflectivityColor);

                material.SetFloat("_Metallic", model.metallic);
                material.SetFloat("_Smoothness", 1 - model.roughness);
                material.SetFloat("_EnvironmentReflections", model.microSurface);
                material.SetFloat("_SpecularHighlights", model.specularIntensity * model.directIntensity);

                // FETCH AND LOAD EMISSIVE TEXTURE
                SetMaterialTexture("_EmissionMap", model.emissiveTexture, emissiveDCLTexture);
            }

            SetupTransparencyMode();

            // FETCH AND LOAD TEXTURES
            SetMaterialTexture("_BaseMap", model.albedoTexture, albedoDCLTexture);
            SetMaterialTexture("_BumpMap", model.bumpTexture, bumpDCLTexture);

            return null;
        }

        private void SetupTransparencyMode()
        {

            // Reset shader keywords
            material.DisableKeyword("_ALPHATEST_ON"); // Cut Out Transparency
            material.DisableKeyword("_ALPHABLEND_ON"); // Fade Transparency
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Transparent

            TransparencyMode transparencyMode = (TransparencyMode)model.transparencyMode;

            if (transparencyMode == TransparencyMode.AUTO)
            {
                if (!string.IsNullOrEmpty(model.alphaTexture) || model.albedoColor.a < 1f) //AlphaBlend
                {
                    transparencyMode = TransparencyMode.ALPHA_BLEND;
                }
                else // Opaque
                {
                    transparencyMode = TransparencyMode.OPAQUE;
                }
            }

            switch (transparencyMode)
            {
                case TransparencyMode.OPAQUE:
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    material.SetFloat("_AlphaClip", 0);
                    break;
                case TransparencyMode.ALPHA_TEST: // ALPHATEST
                    material.EnableKeyword("_ALPHATEST_ON");

                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.SetFloat("_AlphaClip", 1);
                    material.SetFloat("_Cutoff", model.alphaTest);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case TransparencyMode.ALPHA_BLEND: // ALPHABLEND
                    material.EnableKeyword("_ALPHABLEND_ON");

                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.SetFloat("_AlphaClip", 0);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case TransparencyMode.ALPHA_TEST_AND_BLEND:
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.SetFloat("_AlphaClip", 1);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        private void LoadMaterial(string name)
        {
            if (material == null || material.name != name)
            {
                if (material != null)
                {
                    UnityEngine.Object.Destroy(material);
                    Resources.UnloadUnusedAssets();
                }

                material = new Material(Utils.EnsureResourcesMaterial(MATERIAL_RESOURCES_PATH + name));
                material.name = name;
            }
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
                    matTransition.PopulateTargetRendererWithMaterial(matTransition.finalMaterials);
                }

                meshRenderer.sharedMaterial = material;
                SRPBatchingHelper.OptimizeMaterial(meshRenderer, material);

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

        void SetMaterialTexture(string materialPropertyName, string textureComponentId, DCLTexture cachedDCLTexture)
        {
            if (!string.IsNullOrEmpty(textureComponentId))
            {
                if (!AreSameTextureComponent(cachedDCLTexture, textureComponentId))
                {
                    scene.StartCoroutine(DCLTexture.FetchTextureComponent(scene, textureComponentId,
                        (fetchedDCLTexture) =>
                        {
                            material.SetTexture(materialPropertyName, fetchedDCLTexture.texture);
                            SwitchTextureComponent(cachedDCLTexture, fetchedDCLTexture);
                        }));
                }
            }
            else
            {
                material.SetTexture(materialPropertyName, null);
                cachedDCLTexture?.DetachFrom(this);
                cachedDCLTexture = null;
            }
        }

        bool AreSameTextureComponent(DCLTexture dclTexture, string textureId)
        {
            if (dclTexture == null) return false;
            return dclTexture.id == textureId;
        }

        void SwitchTextureComponent(DCLTexture cachedTexture, DCLTexture newTexture)
        {
            cachedTexture?.DetachFrom(this);
            cachedTexture = newTexture;
            cachedTexture.AttachTo(this);
        }

        public override void Dispose()
        {
            albedoDCLTexture?.DetachFrom(this);
            emissiveDCLTexture?.DetachFrom(this);
            bumpDCLTexture?.DetachFrom(this);

            albedoDCLTexture = null;
            emissiveDCLTexture = null;
            bumpDCLTexture = null;

            if (material != null)
            {
                GameObject.Destroy(material);
            }

            base.Dispose();
        }
    }
}
