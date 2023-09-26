using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.MapLayers.SatelliteAtlas
{
    public class SatelliteChunkController: IChunkController
    {
        private const int PIXELS_PER_UNIT = 50;
        private const string CHUNKS_API = "https://media.githubusercontent.com/media/genesis-city/genesis.city/master/map/latest/3/";

        private readonly Vector2 Vector2_OneHalf = new (0.5f, 0.5f);

        private readonly SpriteRenderer spriteRenderer;

        private Service<IWebRequestController> webRequestController;

        public SatelliteChunkController(SpriteRenderer prefab, Vector3 chunkLocalPosition, Vector2Int coordsCenter, Transform parent)
        {
            spriteRenderer = Object.Instantiate(prefab, parent);
#if UNITY_EDITOR
            spriteRenderer.gameObject.name = $"Chunk {coordsCenter.x},{coordsCenter.y}";
#endif

            var transform = spriteRenderer.transform;
            transform.localScale = Vector3.one * 78.125f;
            transform.localPosition = chunkLocalPosition;

            internalCts = new CancellationTokenSource();
        }

        private CancellationTokenSource internalCts;
        private CancellationTokenSource linkedCts;
        private int webRequestAttempts;
        private const int MAX_ATTEMPTS = 3;

        public async UniTask LoadImage(Vector2Int chunkId, CancellationToken ct)
        {
            webRequestAttempts = 0;
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, ct);

            var url = $"{CHUNKS_API}{chunkId.x}%2C{chunkId.y}.jpg";
            UnityWebRequest webRequest = await GetTextureFromWebAsync(url, linkedCts.Token);

            var texture = CreateTexture(webRequest.downloadHandler.data);
            texture.wrapMode = TextureWrapMode.Clamp;

            Debug.Log(texture.width + " " + texture.height);

            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2_OneHalf, pixelsPerUnit: PIXELS_PER_UNIT,
                0, SpriteMeshType.FullRect, Vector4.one, false);

            Debug.Log(GetScaledSpriteSize(spriteRenderer));
            return;

            Texture2D CreateTexture(byte[] data)
            {
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(data);
                return texture2D;
            }
        }

        private async UniTask<UnityWebRequest> GetTextureFromWebAsync(string url, CancellationToken ct)
        {
            UnityWebRequest webRequest;

            try
            {
                webRequest = await webRequestController.Ref.GetTextureAsync(url, cancellationToken:  linkedCts.Token);
            }
            catch (Exception e)
            {
                if (webRequestAttempts < MAX_ATTEMPTS)
                {
                    webRequestAttempts++;
                    webRequest = await GetTextureFromWebAsync(url, linkedCts.Token);
                }
                else
                    throw;
            }

            return webRequest;
        }

        private static Vector2 GetScaledSpriteSize(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return Vector2.zero;

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            Vector2 scaledSize = new Vector2(spriteSize.x * spriteRenderer.transform.localScale.x, spriteSize.y * spriteRenderer.transform.localScale.y);

            return scaledSize;
        }

        public void Dispose()
        {
            internalCts?.Cancel();
            linkedCts?.Dispose();
            linkedCts = null;

            internalCts?.Dispose();
            internalCts = null;

            if (spriteRenderer)
                Utils.SafeDestroy(spriteRenderer.gameObject);
        }

        public void SetDrawOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }
    }
}
