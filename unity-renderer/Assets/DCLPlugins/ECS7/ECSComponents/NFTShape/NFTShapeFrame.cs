using NFTShape_Internal;
using UnityEngine;

namespace DCL.ECSComponents
{
    public interface INFTShapeFrame
    {
        /// <summary>
        /// The gameObject that represent the frame
        /// </summary>
        GameObject gameObject { get; }
        Renderer frameRenderer { get; }

        /// <summary>
        /// This set the image and creates the HQ Texture handler
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="nftAsset"></param>
        void SetImage(string name, string url, INFTAsset nftAsset);

        /// <summary>
        /// Dispose the frame
        /// </summary>
        void Dispose();

        /// <summary>
        /// Show that it has been an error loading the frame
        /// </summary>
        void FailLoading();

        /// <summary>
        /// Update the background of the texture so you can set the same background color as the image 
        /// </summary>
        /// <param name="newColor"></param>
        void UpdateBackgroundColor(Color newColor);
    }

    public class NFTShapeFrame : MonoBehaviour, INFTShapeFrame
    {
        [SerializeField] internal BoxCollider boxCollider;
        [SerializeField] internal MeshRenderer meshRenderer;
        [SerializeField] private GameObject loadingSpinnerGameObject;
        [SerializeField] private NFTShapeMaterial[] materials;

        [Header("Noise Shader")]
        [SerializeField] NoiseType noiseType = NoiseType.Simplex;
        [SerializeField] bool noiseIs3D = false;
        [SerializeField] bool noiseIsFractal = false;

        private NFTShapeHQImageHandler hqTextureHandler;
        private Material frameMaterial;
        private Material imageMaterial;
        private Material backgroundMaterial;

        static readonly int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
        static readonly int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

        public enum NoiseType
        {
            ClassicPerlin,
            PeriodicPerlin,
            Simplex,
            SimplexNumericalGrad,
            SimplexAnalyticalGrad,
            None
        }

        public Renderer frameRenderer => meshRenderer;

        private void Awake()
        {
            // NOTE: we use half scale to keep backward compatibility cause we are using 512px to normalize the scale with a 256px value that comes from the images
            meshRenderer.transform.localScale = new UnityEngine.Vector3(0.5f, 0.5f, 1);
            InitializeMaterials();
        }

        private void Start()
        {
            loadingSpinnerGameObject.layer = LayerMask.NameToLayer("ViewportCullingIgnored");
        }

        public void SetImage(string name, string url, INFTAsset nftAsset)
        {
            if (nftAsset.previewAsset != null)
                SetFrameImage(nftAsset.previewAsset.texture, resizeFrameMesh: true);

            loadingSpinnerGameObject.SetActive(false);
            var hqImageHandlerConfig = new NFTShapeHQImageConfig()
            {
                transform = transform,
                collider = boxCollider,
                name = name,
                imageUrl = url,
                asset = nftAsset
            };

            hqTextureHandler = NFTShapeHQImageHandler.Create(hqImageHandlerConfig);
            nftAsset.OnTextureUpdate += UpdateTexture;
        }

        public void Update()
        {
            hqTextureHandler?.Update();
        }

        public void Dispose()
        {
            hqTextureHandler?.Dispose();
        }

        public void FailLoading()
        {
            loadingSpinnerGameObject.SetActive(false);
#if UNITY_EDITOR
            gameObject.name += " - Failed loading";
#endif
        }

        public void UpdateBackgroundColor(Color newColor)
        {
            if (backgroundMaterial == null)
                return;

            backgroundMaterial.SetColor(COLOR_SHADER_PROPERTY, newColor);
        }

        private void SetFrameImage(Texture2D texture, bool resizeFrameMesh = false)
        {
            if (texture == null)
                return;

            UpdateTexture(texture);

            if (resizeFrameMesh && meshRenderer != null)
            {
                float w, h;
                w = h = 0.5f;
                if (texture.width > texture.height)
                    h *= texture.height / (float)texture.width;
                else if (texture.width < texture.height)
                    w *= texture.width / (float)texture.height;
                UnityEngine.Vector3 newScale = new UnityEngine.Vector3(w, h, 1f);

                meshRenderer.transform.localScale = newScale;
            }
        }

        private void UpdateTexture(Texture2D texture)
        {
            if (imageMaterial == null)
                return;

            imageMaterial.SetTexture(BASEMAP_SHADER_PROPERTY, texture);
            imageMaterial.SetColor(COLOR_SHADER_PROPERTY, Color.white);
        }

        private void InitializeMaterials()
        {
            Material[] meshMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                switch (materials[i].type)
                {
                    case NFTShapeMaterial.MaterialType.BACKGROUND:
                        backgroundMaterial = new Material(materials[i].material);
                        meshMaterials[i] = backgroundMaterial;
                        break;
                    case NFTShapeMaterial.MaterialType.FRAME:
                        frameMaterial = materials[i].material;
                        meshMaterials[i] = frameMaterial;
                        break;
                    case NFTShapeMaterial.MaterialType.IMAGE:
                        imageMaterial = new Material(materials[i].material);
                        meshMaterials[i] = imageMaterial;
                        break;
                }
            }

            meshRenderer.materials = meshMaterials;

            if (frameMaterial == null)
                return;

            frameMaterial.shaderKeywords = null;

            if (noiseType == NoiseType.None)
                return;

            switch (noiseType)
            {
                case NoiseType.ClassicPerlin:
                    frameMaterial.EnableKeyword("CNOISE");
                    break;
                case NoiseType.PeriodicPerlin:
                    frameMaterial.EnableKeyword("PNOISE");
                    break;
                case NoiseType.Simplex:
                    frameMaterial.EnableKeyword("SNOISE");
                    break;
                case NoiseType.SimplexNumericalGrad:
                    frameMaterial.EnableKeyword("SNOISE_NGRAD");
                    break;
                default: // SimplexAnalyticalGrad
                    frameMaterial.EnableKeyword("SNOISE_AGRAD");
                    break;
            }

            if (noiseIs3D)
                frameMaterial.EnableKeyword("THREED");

            if (noiseIsFractal)
                frameMaterial.EnableKeyword("FRACTAL");
        }

    }
}