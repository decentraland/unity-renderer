using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.MapRendererV2.MapLayers.SatelliteAtlas
{
    public class SatelliteChunkController: IChunkController
    {
        private const int PIXELS_PER_UNIT = 50;
        private const string CHUNKS_API = "https://api.decentraland.org/v1/map.png";

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
            transform.localScale = Vector3.one * 78;
            transform.localPosition = chunkLocalPosition;
        }

        public async UniTask LoadImage(int chunkSize, int parcelSize, Vector2Int mapPosition, CancellationToken ct)
        {
            var url = $"https://media.githubusercontent.com/media/genesis-city/genesis.city/master/map/latest/3/{mapPosition.x}%2C{mapPosition.y}.jpg";
            var webRequest = await webRequestController.Ref.GetTextureAsync(url, cancellationToken: ct);

            var texture = CreateTexture(webRequest.downloadHandler.data);
            texture.wrapMode = TextureWrapMode.Clamp;


            spriteRenderer.sprite =
                Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2_OneHalf, PIXELS_PER_UNIT, 0, SpriteMeshType.FullRect, Vector4.one, false);

            Texture2D CreateTexture(byte[] data)
            {
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(data);
                return texture2D;
            }
        }

        public void Dispose()
        {
            if (spriteRenderer)
                Utils.SafeDestroy(spriteRenderer.gameObject);
        }

        public void SetDrawOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }
    }
}
