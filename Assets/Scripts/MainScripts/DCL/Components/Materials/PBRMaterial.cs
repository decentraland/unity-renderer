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
            public float alpha = 1f;

            public Color albedoColor = Color.white;
            public string albedoTexture;
            public Color ambientColor = Color.white;
            public float metallic = 0.5f;
            public float roughness = 0.5f;
            public float microSurface = 1f; // Glossiness
            public float specularIntensity = 1f;
            public bool hasAlpha = false;
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

        public Model model = new Model();
        public Material material { get; set; }

        const string MATERIAL_RESOURCES_PATH = "Materials/";
        const string BASIC_MATERIAL_NAME = "BasicShapeMaterial";
        const string PBR_MATERIAL_NAME = "ShapeMaterial";

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
                // FETCH AND LOAD EMISSIVE TEXTURE
                if (!string.IsNullOrEmpty(model.emissiveTexture))
                {
                    yield return DCLTexture.FetchFromComponent(scene, model.emissiveTexture,
                        (fetchedEmissiveTexture) =>
                        {
                            material.SetTexture("_EmissionMap", fetchedEmissiveTexture);
                        });
                }
                else
                {
                    material.SetTexture("_EmissionMap", null);
                }

                // METALLIC/SPECULAR CONFIGURATIONS
                material.SetColor("_EmissionColor", model.emissiveColor * model.emissiveIntensity);

                if (model.emissiveColor != Color.clear && model.emissiveColor != Color.black)
                {
                    material.EnableKeyword("_EMISSION");
                }

                material.SetColor("_SpecColor", model.reflectivityColor);

                material.SetFloat("_Metallic", model.metallic);
                material.SetFloat("_Glossiness", 1 - model.roughness);
                material.SetFloat("_GlossyReflections", model.microSurface);
                material.SetFloat("_SpecularHighlights", model.specularIntensity * model.directIntensity);
            }

            material.SetColor("_Color", model.albedoColor);

            // FETCH AND LOAD TEXTURES
            if (!string.IsNullOrEmpty(model.albedoTexture))
            {
                yield return DCLTexture.FetchFromComponent(scene, model.albedoTexture,
                    (fetchedAlbedoTexture) =>
                    {
                        material.SetTexture("_MainTex", fetchedAlbedoTexture);
                    });
            }
            else
            {
                material.SetTexture("_MainTex", null);
            }

            if (!string.IsNullOrEmpty(model.bumpTexture))
            {
                yield return DCLTexture.FetchFromComponent(scene, model.bumpTexture,
                    (fetchedBumpTexture) =>
                    {
                        material.SetTexture("_BumpMap", fetchedBumpTexture);
                    });
            }
            else
            {
                material.SetTexture("_BumpMap", null);
            }

            // ALPHA CONFIGURATION
            material.SetFloat("_AlphaClip", model.alpha);

            // Reset shader keywords
            material.DisableKeyword("_ALPHATEST_ON"); // Cut Out Transparency
            material.DisableKeyword("_ALPHABLEND_ON"); // Fade Transparency
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Transparent

            switch (model.transparencyMode)
            {
                case 0:
                    material.renderQueue = 2000;
                    break;
                case 1: // ALPHATEST
                    material.EnableKeyword("_ALPHATEST_ON");

                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.renderQueue = 2450;
                    break;
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
                default:
                    if (model.hasAlpha || !string.IsNullOrEmpty(model.alphaTexture)) //AlphaBlend
                    {
                        material.EnableKeyword("_ALPHABLEND_ON");

                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.renderQueue = 3000;
                    }
                    else // Opaque
                    {
                        material.renderQueue = 2000;
                    }

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
            {
                GameObject.Destroy(material);
            }

            base.Dispose();
        }
    }
}