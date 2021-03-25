using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Components
{
    public class PBRMaterial : BaseDisposable
    {
        [System.Serializable]
        public class Model : BaseModel
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
            public bool castShadows = true;

            [Range(0, 4)]
            public int transparencyMode = 4; // 0: OPAQUE; 1: ALPHATEST; 2: ALPHBLEND; 3: ALPHATESTANDBLEND; 4: AUTO (Engine decide)

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        enum TransparencyMode
        {
            OPAQUE,
            ALPHA_TEST,
            ALPHA_BLEND,
            ALPHA_TEST_AND_BLEND,
            AUTO
        }

        public Material material { get; set; }
        private string currentMaterialResourcesFilename;

        const string MATERIAL_RESOURCES_PATH = "Materials/";
        const string PBR_MATERIAL_NAME = "ShapeMaterial";

        DCLTexture albedoDCLTexture = null;
        DCLTexture alphaDCLTexture = null;
        DCLTexture emissiveDCLTexture = null;
        DCLTexture bumpDCLTexture = null;

        public PBRMaterial()
        {
            model = new Model();

            LoadMaterial(PBR_MATERIAL_NAME);

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;
        }

        new public Model GetModel()
        {
            return (Model) model;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.PBR_MATERIAL;
        }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            entity.RemoveSharedComponent(typeof(BasicMaterial));
            base.AttachTo(entity);
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            Model model = (Model) newModel;

            LoadMaterial(PBR_MATERIAL_NAME);

            material.SetColor(ShaderUtils.BaseColor, model.albedoColor);

            if (model.emissiveColor != Color.clear && model.emissiveColor != Color.black)
            {
                material.EnableKeyword("_EMISSION");
            }

            // METALLIC/SPECULAR CONFIGURATIONS
            material.SetColor(ShaderUtils.EmissionColor, model.emissiveColor * model.emissiveIntensity);
            material.SetColor(ShaderUtils.SpecColor, model.reflectivityColor);

            material.SetFloat(ShaderUtils.Metallic, model.metallic);
            material.SetFloat(ShaderUtils.Smoothness, 1 - model.roughness);
            material.SetFloat(ShaderUtils.EnvironmentReflections, model.microSurface);
            material.SetFloat(ShaderUtils.SpecularHighlights, model.specularIntensity * model.directIntensity);

            // FETCH AND LOAD EMISSIVE TEXTURE
            SetMaterialTexture(ShaderUtils.EmissionMap, model.emissiveTexture, emissiveDCLTexture);

            SetupTransparencyMode();

            // FETCH AND LOAD TEXTURES
            SetMaterialTexture(ShaderUtils.BaseMap, model.albedoTexture, albedoDCLTexture);
            SetMaterialTexture(ShaderUtils.AlphaTexture, model.alphaTexture, alphaDCLTexture);
            SetMaterialTexture(ShaderUtils.BumpMap, model.bumpTexture, bumpDCLTexture);

            foreach (IDCLEntity decentralandEntity in attachedEntities)
            {
                InitMaterial(decentralandEntity.meshRootGameObject);
            }

            return null;
        }

        private void SetupTransparencyMode()
        {
            Model model = (Model) this.model;

            // Reset shader keywords
            material.DisableKeyword("_ALPHATEST_ON"); // Cut Out Transparency
            material.DisableKeyword("_ALPHABLEND_ON"); // Fade Transparency
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Transparent

            TransparencyMode transparencyMode = (TransparencyMode) model.transparencyMode;

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
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry;
                    material.SetFloat(ShaderUtils.AlphaClip, 0);
                    break;
                case TransparencyMode.ALPHA_TEST: // ALPHATEST
                    material.EnableKeyword("_ALPHATEST_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(ShaderUtils.DstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt(ShaderUtils.ZWrite, 1);
                    material.SetFloat(ShaderUtils.AlphaClip, 1);
                    material.SetFloat(ShaderUtils.Cutoff, model.alphaTest);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case TransparencyMode.ALPHA_BLEND: // ALPHABLEND
                    material.EnableKeyword("_ALPHABLEND_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt(ShaderUtils.DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderUtils.ZWrite, 0);
                    material.SetFloat(ShaderUtils.AlphaClip, 0);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case TransparencyMode.ALPHA_TEST_AND_BLEND:
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(ShaderUtils.DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderUtils.ZWrite, 0);
                    material.SetFloat(ShaderUtils.AlphaClip, 1);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }


        private void LoadMaterial(string resourcesFilename)
        {
            if (material == null || currentMaterialResourcesFilename != resourcesFilename)
            {
                if (material != null)
                    Object.Destroy(material);

                material = new Material(Utils.EnsureResourcesMaterial(MATERIAL_RESOURCES_PATH + resourcesFilename));
#if UNITY_EDITOR
                material.name = "PBRMaterial_" + id;
#endif
                currentMaterialResourcesFilename = resourcesFilename;
            }
        }

        void OnMaterialAttached(IDCLEntity entity)
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
            if (meshRenderer == null)
                return;
            Model model = (Model) this.model;

            meshRenderer.shadowCastingMode = model.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            if (meshRenderer.sharedMaterial != material)
            {
                MaterialTransitionController
                    matTransition = meshGameObject.GetComponent<MaterialTransitionController>();

                if (matTransition != null && matTransition.canSwitchMaterial)
                {
                    matTransition.finalMaterials = new Material[] {material};
                    matTransition.PopulateTargetRendererWithMaterial(matTransition.finalMaterials);
                }

                meshRenderer.sharedMaterial = material;
                SRPBatchingHelper.OptimizeMaterial(material);
            }
        }


        private void OnShapeUpdated(IDCLEntity entity)
        {
            if (entity != null)
            {
                InitMaterial(entity.meshRootGameObject);
            }
        }


        void OnMaterialDetached(IDCLEntity entity)
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

        void SetMaterialTexture(int materialPropertyId, string textureComponentId, DCLTexture cachedDCLTexture)
        {
            if (!string.IsNullOrEmpty(textureComponentId))
            {
                if (!AreSameTextureComponent(cachedDCLTexture, textureComponentId))
                {
                    CoroutineStarter.Start(DCLTexture.FetchTextureComponent(scene, textureComponentId,
                        (fetchedDCLTexture) =>
                        {
                            if (material != null)
                            {
                                material.SetTexture(materialPropertyId, fetchedDCLTexture.texture);
                                SwitchTextureComponent(cachedDCLTexture, fetchedDCLTexture);
                            }
                        }));
                }
            }
            else
            {
                material.SetTexture(materialPropertyId, null);
                cachedDCLTexture?.DetachFrom(this);
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
            alphaDCLTexture?.DetachFrom(this);
            emissiveDCLTexture?.DetachFrom(this);
            bumpDCLTexture?.DetachFrom(this);

            if (material != null)
            {
                Utils.SafeDestroy(material);
            }

            base.Dispose();
        }
    }
}