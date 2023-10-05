using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.MapLayers.Atlas.SatelliteAtlas
{
    public class SatelliteChunkController : IChunkController
    {
        private const string CHUNKS_API = "https://media.githubusercontent.com/media/genesis-city/genesis.city/master/map/latest/3/";

        private readonly SpriteRenderer spriteRenderer;

        private Service<IWebRequestController> webRequestController;

        private CancellationTokenSource internalCts;
        private CancellationTokenSource linkedCts;
        private int webRequestAttempts;

        public SatelliteChunkController(SpriteRenderer prefab, Vector3 chunkLocalPosition, Vector2Int coordsCenter, Transform parent, int drawOrder)
        {
            internalCts = new CancellationTokenSource();

            spriteRenderer = Object.Instantiate(prefab, parent);
            spriteRenderer.transform.localPosition = chunkLocalPosition;
            spriteRenderer.sortingOrder = drawOrder;

#if UNITY_EDITOR
            spriteRenderer.gameObject.name = $"Chunk {coordsCenter.x},{coordsCenter.y}";
#endif
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

        public async UniTask LoadImage(Vector2Int chunkId, float chunkWorldSize, CancellationToken ct)
        {
            webRequestAttempts = 0;
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, ct);

            var url = $"{CHUNKS_API}{chunkId.x}%2C{chunkId.y}.jpg";

            UnityWebRequest webRequest = await GetTextureFromWebAsync(url, ct: linkedCts.Token);

            Texture2D texture = CreateTexture(webRequest.downloadHandler.data);
            texture.wrapMode = TextureWrapMode.Clamp;

            float pixelsPerUnit = texture.width / chunkWorldSize;

            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2Ext.OneHalf, pixelsPerUnit,
                0, SpriteMeshType.FullRect, Vector4.one, false);

            return;

            Texture2D CreateTexture(byte[] data)
            {
                var texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(data);
                return texture2D;
            }
        }

        private async UniTask<UnityWebRequest> GetTextureFromWebAsync(string url, int maxAttempts = 3, CancellationToken ct = default)
        {
            UnityWebRequest webRequest;
#if UNITY_EDITOR
            // fixes editor exception for downloading pictures from github: "UnityWebRequestException: Unable to complete SSL connection"
            try { webRequest = await webRequestController.Ref.GetTextureAsync(url, cancellationToken: linkedCts.Token); }
            catch (UnityWebRequestException e) when (e.Message.Contains("Unable to complete SSL connection"))
            {
                if (webRequestAttempts < maxAttempts)
                {
                    webRequestAttempts++;
                    webRequest = await GetTextureFromWebAsync(url, ct: linkedCts.Token);
                }
                else
                    throw;
            }
#else
            webRequest = await webRequestController.Ref.GetTextureAsync(url, cancellationToken: linkedCts.Token);
#endif

            return webRequest;
        }
    }
}
