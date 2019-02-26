using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class PBRMaterial : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            [Range(0f, 1f)]
            public float alpha = 1f;

            public string albedoColor = "#fff";
            public string albedoTexture;
            public string ambientColor = "#fff";
            public float metallic = 0.5f;
            public float roughness = 0.5f;
            public float microSurface = 1f; // Glossiness
            public float specularIntensity = 1f;
            public bool hasAlpha = false;
            public string alphaTexture;
            public string emissiveTexture;
            public string emissiveColor = "#000";
            public float emissiveIntensity = 1f;
            public string reflectionColor = "#fff"; // Specular color
            public string reflectivityColor = "#fff";
            public float directIntensity = 1f;
            public float environmentIntensity = 1f;
            public string bumpTexture;
            public string refractionTexture;
            public bool disableLighting = false;

            [Range(0, 3)]
            public int transparencyMode = 0; // 0: OPAQUE; 1: ALPHATEST; 2: ALPHBLEND; 3: ALPHATESTANDBLEND
        }

        public override string componentName => "material";
        public Model model = new Model();
        public Material material { get; set; }

        const string MATERIAL_RESOURCES_PATH = "Materials/";
        const string BASIC_MATERIAL_NAME = "BasicShapeMaterial";
        const string PBR_MATERIAL_NAME = "ShapeMaterial";

        bool loadingAlbedoTexture = false;
        bool loadingBumpTexture = false;
        bool loadingEmissiveTexture = false;

        public PBRMaterial(ParcelScene scene) : base(scene)
        {
            model = new Model();
            LoadMaterial("ShapeMaterial");

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;

            loadingAlbedoTexture = false;
            loadingBumpTexture = false;
            loadingEmissiveTexture = false;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = JsonUtility.FromJson<Model>(newJson);
            Color auxColor = new Color();

            if (model.disableLighting)
            {
                LoadMaterial(BASIC_MATERIAL_NAME);
            }
            else
            {
                LoadMaterial(PBR_MATERIAL_NAME);
                // FETCH AND LOAD EMISSIVE TEXTURE
                if (!string.IsNullOrEmpty(model.emissiveTexture) && !loadingEmissiveTexture)
                {
                    string texUrl;

                    if (scene.sceneData.TryGetContentsUrl(model.emissiveTexture, out texUrl))
                    {
                        loadingEmissiveTexture = true;
                        yield return Utils.FetchTexture(texUrl, (fetchedEmissiveTexture) =>
                        {
                            material.SetTexture("_EmissionMap", fetchedEmissiveTexture);
                            loadingEmissiveTexture = false;
                        });
                    }
                }

                // METALLIC/SPECULAR CONFIGURATIONS
                ColorUtility.TryParseHtmlString(model.emissiveColor, out auxColor);
                material.SetColor("_EmissionColor", auxColor * model.emissiveIntensity);

                ColorUtility.TryParseHtmlString(model.reflectivityColor, out auxColor);
                material.SetColor("_SpecColor", auxColor);

                material.SetFloat("_Metallic", model.metallic);
                material.SetFloat("_Glossiness", 1 - model.roughness);
                material.SetFloat("_GlossyReflections", model.microSurface);
                material.SetFloat("_SpecularHighlights", model.specularIntensity * model.directIntensity);
            }

            ColorUtility.TryParseHtmlString(model.albedoColor, out auxColor);
            material.SetColor("_Color", auxColor);

            // FETCH AND LOAD TEXTURES
            if (!string.IsNullOrEmpty(model.albedoTexture) && !loadingAlbedoTexture)
            {
                string albedoTextureUrl;

                if (scene.sceneData.TryGetContentsUrl(model.albedoTexture, out albedoTextureUrl))
                {
                    loadingAlbedoTexture = true;
                    yield return Utils.FetchTexture(albedoTextureUrl, (fetchedAlbedoTexture) =>
                    {
                        loadingAlbedoTexture = false;
                        material.SetTexture("_MainTex", fetchedAlbedoTexture);
                    });
                }
            }

            if (!string.IsNullOrEmpty(model.bumpTexture) && !loadingBumpTexture)
            {
                string bumpTextureUrl;

                if (scene.sceneData.TryGetContentsUrl(model.bumpTexture, out bumpTextureUrl))
                {
                    loadingBumpTexture = true;
                    yield return Utils.FetchTexture(bumpTextureUrl, (fetchedBumpTexture) =>
                    {
                        loadingBumpTexture = false;
                        material.SetTexture("_BumpMap", fetchedBumpTexture);
                    });
                }
            }

            // ALPHA CONFIGURATION
            material.SetFloat("_AlphaClip", model.alpha);

            if (model.hasAlpha || !string.IsNullOrEmpty(model.alphaTexture))
            {
                // Reset shader keywords
                material.DisableKeyword("_ALPHATEST_ON"); // Cut Out Transparency
                material.DisableKeyword("_ALPHABLEND_ON"); // Fade Transparency
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Transparent

                switch (model.transparencyMode)
                {
                    case 2: // ALPHABLEND
                        material.EnableKeyword("_ALPHABLEND_ON");

                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.renderQueue = 3000;
                        break;
                    case 3: // ALPHATESTANDBLEND
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.renderQueue = 3000;
                        break;
                    default: // ALPHATEST
                        material.EnableKeyword("_ALPHATEST_ON");

                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.renderQueue = 2450;
                        break;
                }
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

                material.enableInstancing = true;
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
