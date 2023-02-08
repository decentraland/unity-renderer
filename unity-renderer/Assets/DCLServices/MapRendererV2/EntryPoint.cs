using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    public int chunkSize = 250;
    public int parcelSize = 5;
    public Transform selector;
    private ChunkAtlasController atlasController;

    void Awake()
    {
        atlasController = new ChunkAtlasController(transform, chunkSize, parcelSize, ChunkController.CreateChunk);
        atlasController.Initialize(default).Forget();
        selector.localScale = Vector3.one * parcelSize / 2f;
    }

    private void Update()
    {
        Debug.Log(atlasController.PositionToCoords(selector.position));
    }
}
