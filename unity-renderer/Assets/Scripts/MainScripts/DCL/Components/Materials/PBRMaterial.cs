using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using DCL.Shaders;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

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
            public float metallic = 0.5f;
            public float roughness = 0.5f;
            public float microSurface = 1f; // Glossiness
            public float specularIntensity = 1f;

            public string alphaTexture;
            public string emissiveTexture;
            public Color emissiveColor = Color.black;
            public float emissiveIntensity = 2f;
            public Color reflectivityColor = Color.white;
            public float directIntensity = 1f;
            public string bumpTexture;
            public bool castShadows = true;

            [Range(0, 4)]
            public int transparencyMode = 4; // 0: OPAQUE; 1: ALPHATEST; 2: ALPHBLEND; 3: ALPHATESTANDBLEND; 4: AUTO (Engine decide)

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.Material)
                    return Utils.SafeUnimplemented<PBRMaterial, Model>(expected: ComponentBodyPayload.PayloadOneofCase.Material, actual: pbModel.PayloadCase);
                
                var pb = new Model();
                if (pbModel.Material.HasMetallic) pb.metallic = pbModel.Material.Metallic;
                if (pbModel.Material.HasRoughness) pb.roughness = pbModel.Material.Roughness;
                if (pbModel.Material.HasAlphaTest) pb.alphaTest = pbModel.Material.AlphaTest;
                if (pbModel.Material.HasDirectIntensity) pb.directIntensity = pbModel.Material.DirectIntensity;
                if (pbModel.Material.HasMicroSurface) pb.microSurface = pbModel.Material.MicroSurface;
                if (pbModel.Material.HasSpecularIntensity) pb.specularIntensity = pbModel.Material.SpecularIntensity;
                if (pbModel.Material.HasAlbedoTexture) pb.albedoTexture = pbModel.Material.AlbedoTexture;
                if (pbModel.Material.HasAlphaTexture) pb.alphaTexture = pbModel.Material.AlphaTexture;
                if (pbModel.Material.HasBumpTexture) pb.bumpTexture = pbModel.Material.BumpTexture;
                if (pbModel.Material.HasCastShadows) pb.castShadows = pbModel.Material.CastShadows;
                if (pbModel.Material.HasEmissiveIntensity) pb.emissiveIntensity = pbModel.Material.EmissiveIntensity;
                if (pbModel.Material.HasEmissiveTexture) pb.emissiveTexture = pbModel.Material.EmissiveTexture;
                if (pbModel.Material.HasTransparencyMode) pb.transparencyMode = (int)pbModel.Material.TransparencyMode;
                if (pbModel.Material.AlbedoColor != null) pb.albedoColor = pbModel.Material.AlbedoColor.AsUnityColor();
                if (pbModel.Material.EmissiveColor != null) pb.emissiveColor = pbModel.Material.EmissiveColor.AsUnityColor();
                if (pbModel.Material.ReflectivityColor != null) pb.reflectivityColor = pbModel.Material.ReflectivityColor.AsUnityColor();
                
                return pb;
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

        private enum TextureType
        {
            Albedo,
            Alpha,
            Emissive,
            Bump
        }

        public Material material { get; private set; }
        private string currentMaterialResourcesFilename;

        const string MATERIAL_RESOURCES_PATH = "Materials/";
        const string PBR_MATERIAL_NAME = "ShapeMaterial";

        private readonly DCLTexture[] textures = new DCLTexture[4];

        private readonly DCLTexture.Fetcher[] dclTextureFetcher = new DCLTexture.Fetcher[]
        {
            new DCLTexture.Fetcher(),
            new DCLTexture.Fetcher(),
            new DCLTexture.Fetcher(),
            new DCLTexture.Fetcher()
        };

        public PBRMaterial()
        {
            model = new Model();

            LoadMaterial(PBR_MATERIAL_NAME);

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;
        }

        new public Model GetModel() { return (Model) model; }

        public override int GetClassId() { return (int) CLASS_ID.PBR_MATERIAL; }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            scene.componentsManagerLegacy.RemoveSharedComponent(entity, (typeof(BasicMaterial)));
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
            var fetchEmission = FetchTexture(ShaderUtils.EmissionMap, model.emissiveTexture, (int)TextureType.Emissive);

            SetupTransparencyMode();

            // FETCH AND LOAD TEXTURES
            var fetchBaseMap = FetchTexture(ShaderUtils.BaseMap, model.albedoTexture, (int)TextureType.Albedo);
            var fetchAlpha = FetchTexture(ShaderUtils.AlphaTexture, model.alphaTexture, (int)TextureType.Alpha);
            var fetchBump = FetchTexture(ShaderUtils.BumpMap, model.bumpTexture, (int)TextureType.Bump);

            yield return UniTask.WhenAll(fetchEmission, fetchBaseMap, fetchAlpha, fetchBump).ToCoroutine();

            foreach (IDCLEntity entity in attachedEntities)
                InitMaterial(entity);
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
                    material.SetInt("_Surface", 0);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case TransparencyMode.ALPHA_BLEND: // ALPHABLEND
                    material.EnableKeyword("_ALPHABLEND_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt(ShaderUtils.DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderUtils.ZWrite, 0);
                    material.SetFloat(ShaderUtils.AlphaClip, 0);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                    material.SetInt("_Surface", 1);
                    break;
                case TransparencyMode.ALPHA_TEST_AND_BLEND:
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(ShaderUtils.DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderUtils.ZWrite, 0);
                    material.SetFloat(ShaderUtils.AlphaClip, 1);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                    material.SetInt("_Surface", 1);
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
                    InitMaterial(entity);
            }
        }

        void InitMaterial(IDCLEntity entity)
        {
            var meshGameObject = entity.meshRootGameObject;

            if (meshGameObject == null)
                return;

            var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
                return;

            Model model = (Model) this.model;

            meshRenderer.shadowCastingMode = model.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if (meshRenderer.sharedMaterial == material)
                return;

            Material oldMaterial = meshRenderer.sharedMaterial;
            meshRenderer.sharedMaterial = material;
            SRPBatchingHelper.OptimizeMaterial(material);

            DataStore.i.sceneWorldObjects.RemoveMaterial(scene.sceneData.sceneNumber, entity.entityId, oldMaterial);
            DataStore.i.sceneWorldObjects.AddMaterial(scene.sceneData.sceneNumber, entity.entityId, material);
        }

        private void OnShapeUpdated(IDCLEntity entity)
        {
            if (entity != null)
                InitMaterial(entity);
        }

        private void OnMaterialDetached(IDCLEntity entity)
        {
            if (entity.meshRootGameObject != null)
            {
                entity.OnShapeUpdated -= OnShapeUpdated;

                var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

                if (meshRenderer && meshRenderer.sharedMaterial == material)
                    meshRenderer.sharedMaterial = null;
            }
            DataStore.i.sceneWorldObjects.RemoveMaterial(scene.sceneData.sceneNumber, entity.entityId, material);
        }

        private UniTask FetchTexture(int materialPropertyId, string textureComponentId, int textureType)
        {
            if (!string.IsNullOrEmpty(textureComponentId))
            {
                if (!AreSameTextureComponent(textureType, textureComponentId))
                {
                    return dclTextureFetcher[textureType]
                       .Fetch(scene, textureComponentId,
                            fetchedDCLTexture =>
                            {
                                if (material == null)
                                    return false;

                                material.SetTexture(materialPropertyId, fetchedDCLTexture.texture);
                                SwitchTextureComponent(textureType, fetchedDCLTexture);
                                return true;
                            });
                }
            }
            else
            {
                material.SetTexture(materialPropertyId, null);
                textures[textureType]?.DetachFrom(this);
                textures[textureType] = null;
            }

            return new UniTask();
        }

        bool AreSameTextureComponent(int textureType, string textureId)
        {
            DCLTexture dclTexture = textures[textureType];
            if (dclTexture == null)
                return false;
            return dclTexture.id == textureId;
        }

        void SwitchTextureComponent(int textureType, DCLTexture newTexture)
        {
            DCLTexture dclTexture = textures[textureType];
            dclTexture?.DetachFrom(this);
            textures[textureType] = newTexture;
            newTexture.AttachTo(this);
        }

        public override void Dispose()
        {
            for (int i = 0; i < dclTextureFetcher.Length; i++)
            {
                dclTextureFetcher[i].Dispose();
            }

            for (int i = 0; i < textures.Length; i++)
            {
                textures[i]?.DetachFrom(this);
                textures[i] = null;
            }

            if (material != null)
            {
                // we make sure we detach this material from every entity
                // before disposing it
                while ( attachedEntities.Count > 0 )
                {
                    DetachFrom(attachedEntities.First());
                }
                Utils.SafeDestroy(material);
            }

            base.Dispose();
        }
    }
}
