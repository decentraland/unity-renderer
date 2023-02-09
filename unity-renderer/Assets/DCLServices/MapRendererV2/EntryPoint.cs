using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using UnityEngine;

namespace DCLServices.MapRendererV2
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer prefab;

        public int chunkSize = 250;
        public int parcelSize = 5;
        public Transform selector;
        private ChunkAtlasController atlasController;
        private ICoordsUtils coordsUtils;

        void Awake()
        {
            coordsUtils = new ChunkCoordsUtils(parcelSize);

            atlasController = new ChunkAtlasController(transform, prefab, 1, chunkSize, coordsUtils, null, ChunkController.CreateChunk);
            atlasController.Initialize(default).Forget();
            selector.localScale = Vector3.one * parcelSize / 2f;
        }

        private void Update()
        {
            Debug.Log(coordsUtils.PositionToCoords(selector.position));
        }
    }
}
