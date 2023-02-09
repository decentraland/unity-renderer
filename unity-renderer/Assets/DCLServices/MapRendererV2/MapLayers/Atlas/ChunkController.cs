using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.MapLayers.Atlas
{
    public class ChunkController : IChunkController
    {
        private const int PIXELS_PER_UNIT = 50;
        private const string CHUNKS_API = "https://api.decentraland.org/v1/map.png";

        private readonly SpriteRenderer spriteRenderer;

        private Service<IWebRequestController> webRequestController;

        /// <summary>
        ///
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="coordsCenter">0,0 will be considered at bottom left</param>
        /// <param name="parent"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async UniTask<IChunkController> CreateChunk(
            Vector3 chunkLocalPosition,
            int chunkSize,
            int parcelSize,
            int drawOrder,
            Vector2Int coordsCenter,
            Transform parent,
            SpriteRenderer prefab,
            CancellationToken ct)
        {
            var chunk = new ChunkController(prefab, chunkLocalPosition, chunkSize, coordsCenter, parent);
            chunk.SetDrawOrder(drawOrder);
            await chunk.LoadImage(chunkSize, parcelSize, coordsCenter, ct);
            return chunk;
        }

        private ChunkController(SpriteRenderer prefab, Vector3 chunkLocalPosition, int chunkSize, Vector2Int coordsCenter, Transform parent)
        {
            spriteRenderer = Object.Instantiate(prefab, parent);
#if UNITY_EDITOR
            spriteRenderer.gameObject.name = $"Chunk {coordsCenter.x},{coordsCenter.y}";
#endif
            var transform = spriteRenderer.transform;

            transform.localScale = Vector3.one * PIXELS_PER_UNIT;
            transform.localPosition = chunkLocalPosition;
        }

        private async UniTask LoadImage(int chunkSize, int parcelSize, Vector2Int mapPosition, CancellationToken ct)
        {
            string url = $"{CHUNKS_API}?center={mapPosition.x},{mapPosition.y}&width={chunkSize}&height={chunkSize}&size={parcelSize}";

            var webRequest = await webRequestController.Ref.GetTextureAsync(url, cancellationToken: ct);
            var texture = DownloadHandlerTexture.GetContent(webRequest);
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);

            spriteRenderer.sprite = newSprite;
        }

        public void Dispose()
        {
            Utils.SafeDestroy(spriteRenderer.gameObject);
        }

        public void SetDrawOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }
    }
}
