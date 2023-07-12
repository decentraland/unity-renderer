using DCL.Controllers;
using MainScripts.DCL.InWorldCamera.Scripts;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class ScreenshotTaker : MonoBehaviour
    {
        [Space(10)]
        public Camera screenshotCamera;
        public Image image;
        public Canvas canvas;

        [Space(10)]
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private RectTransform imageRectTransform;
        [SerializeField] private Sprite sprite;
        [SerializeField] private ScreenshotCameraMovement cameraMovement;
        private readonly string screenshotFileName = "screenshot4.jpg";
        private readonly int desiredWidth = 1920;
        private readonly int desiredHeight = 1080;

        private bool cameraEnabled;
        private int originalLayer;

        private bool captureScreenshot;

        private void Awake()
        {
            originalLayer = gameObject.layer;
        }

        private void Start()
        {
            screenshotCamera.enabled = false;
            cameraMovement.enabled = false;
            canvas.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (cameraEnabled)
                {
                    screenshotCamera.enabled = false;
                    cameraMovement.enabled = false;
                    canvas.enabled = false;

                    Environment.i.serviceLocator.Get<IAvatarsLODController>().SetCamera(Camera.main);
                }
                else
                {
                    CopyCamera();

                    screenshotCamera.enabled = true;
                    cameraMovement.enabled = true;
                    canvas.enabled = true;

                    Environment.i.serviceLocator.Get<IAvatarsLODController>().SetCamera(screenshotCamera);
                }

                cameraEnabled = !cameraEnabled;
            }

            if (cameraEnabled && Input.GetKeyDown(KeyCode.Space))
                captureScreenshot = true;
        }

        private void LateUpdate()
        {
            if (captureScreenshot)
            {
                captureScreenshot = false;
                CaptureScreenshot();
            }
        }

        [ContextMenu(nameof(CopyCamera))]
        private void CopyCamera()
        {
            screenshotCamera.CopyFrom(Camera.main);
            gameObject.layer = originalLayer;
        }

        [ContextMenu(nameof(CaptureScreenshot))]
        private void CaptureScreenshot()
        {
            sprite = image.sprite;
            imageRectTransform = image.rectTransform;

            (float spriteWidth, float spriteHeight) = CalculateSpriteWidthHeight();

            float scaleFactorW = desiredWidth / spriteWidth;
            float scaleFactorH = desiredHeight / spriteHeight;

            int renderTextureWidth = Mathf.RoundToInt(canvasRectTransform.rect.width * scaleFactorW);
            int renderTextureHeight = Mathf.RoundToInt(canvasRectTransform.rect.height * scaleFactorH);

            var renderTexture = RenderTexture.GetTemporary(renderTextureWidth, renderTextureHeight, 24);
            Debug.Log($"RenderTexture = {renderTextureWidth},{renderTextureHeight}");

            screenshotCamera.targetTexture = renderTexture;
            screenshotCamera.Render();
            RenderTexture.active = screenshotCamera.targetTexture;

            float rtCenterX = renderTextureWidth / 2f;
            float rtCenterY = renderTextureHeight / 2f;

            float cornerX = rtCenterX - (desiredWidth / 2f);
            float cornerY = rtCenterY - (desiredHeight / 2f);
            Debug.Log($"corner = {cornerX},{cornerY}");

            var texture = new Texture2D(Mathf.RoundToInt(desiredWidth), Mathf.RoundToInt(desiredHeight), TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(cornerX, cornerY, desiredWidth, desiredHeight), 0, 0);
            texture.Apply();

            // Gather screenshot metadata
            GetScreenshotMetadata();

            // Save
            SaveScreenshot(texture);

            // Clean up
            Destroy(texture);

            RenderTexture.active = null; // Added to avoid errors
            screenshotCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            Debug.Log("Screenshot taken");

            (float, float) CalculateSpriteWidthHeight()
            {
                Rect imageRect = imageRectTransform.rect;

                float imageWidth = imageRect.width;
                float imageHeight = imageRect.height;

                float imageAspect = imageWidth / imageHeight;
                float spriteAspect = sprite.bounds.size.x / sprite.bounds.size.y;

                float actualWidth, actualHeight;

                // Depending on which dimension is the limiting one (width or height),
                // calculate the actual size of the sprite on screen.
                if (imageAspect > spriteAspect)
                {
                    // The height of the RectTransform is the limiting dimension.
                    // Calculate the width using the sprite's aspect ratio.
                    actualHeight = imageHeight;
                    actualWidth = actualHeight * spriteAspect;
                }
                else
                {
                    // The width of the RectTransform is the limiting dimension.
                    // Calculate the height using the sprite's aspect ratio.
                    actualWidth = imageWidth;
                    actualHeight = actualWidth / spriteAspect;
                }

                return (actualWidth, actualHeight);
            }
        }

        private void GetScreenshotMetadata()
        {
            Debug.Log("---- Taking screenshot ----");
            Debug.Log($"DateTime: {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            Debug.Log($"Realm: {DataStore.i.realm.realmName.Get()}");

            Debug.Log($"Player name: {DataStore.i.player.ownPlayer.Get().name}");
            Debug.Log($"Player id: {DataStore.i.player.ownPlayer.Get().id}");
            Debug.Log($"Player Position: {DataStore.i.player.playerGridPosition.Get()}");

            var sceneInfo =   MinimapMetadata.GetMetadata().GetSceneInfo(
                DataStore.i.player.playerGridPosition.Get().x,
                DataStore.i.player.playerGridPosition.Get().y);

            Debug.Log($"Scene name: {sceneInfo.name}");

            var lodControllers = Environment.i.serviceLocator.Get<IAvatarsLODController>().LodControllers;

            foreach (var lodController in lodControllers.Values.Where(lodController => !lodController.IsInvisible))
            {
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(screenshotCamera);
                Collider playerCollider = lodController.player.collider;

                if (GeometryUtility.TestPlanesAABB(planes, playerCollider.bounds))
                {
                    Debug.Log($"Player {lodController.player.name}, {lodController.player.id}", lodController.player.collider.gameObject);

                    var bridge = new UserProfileWebInterfaceBridge();
                    var userProfile = bridge.Get(lodController.player.id);
                    foreach (string wearable in userProfile.avatar.wearables)
                        Debug.Log($"wearable - {wearable}");
                }
            }
        }

        private void SaveScreenshot(Texture2D texture)
        {
            byte[] fileBytes = texture.EncodeToJPG();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string base64String = Convert.ToBase64String(fileBytes);
                var dataUri = $"data:application/octet-stream;base64,{base64String}";
                Application.OpenURL(dataUri);
            }
            else
            {
                string filePath = Path.Combine(Application.temporaryCachePath, screenshotFileName); // Application.persistentDataPath
                File.WriteAllBytes(filePath, fileBytes);
                Application.OpenURL(filePath);

                Debug.Log(filePath);
            }
        }
    }
}
