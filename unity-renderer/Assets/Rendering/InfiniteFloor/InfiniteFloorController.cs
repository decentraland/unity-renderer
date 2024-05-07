using DCL;
using UnityEngine;

public class InfiniteFloorController : MonoBehaviour
{
    private static readonly int MAP_PROPERTY = Shader.PropertyToID("_Map");
    private static readonly int ESTATE_ID_PROPERTY = Shader.PropertyToID("_EstateIDMap");
    private static readonly int PLAYER_POSITION = Shader.PropertyToID("_PlayerPosition");

    [SerializeField] private Renderer renderer;

    void OnEnable()
    {
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;

        DataStore.i.HUDs.mapMainTexture.OnChange += OnMapTextureChanged;
        OnMapTextureChanged(DataStore.i.HUDs.latestDownloadedMainTexture.Get(), null);

        DataStore.i.HUDs.mapEstatesTexture.OnChange += OnEstateIdTextureChanged;
        OnMapTextureChanged(DataStore.i.HUDs.latestDownloadedMapEstatesTexture.Get(), null);

        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChanged;
        OnPlayerCoordsChanged(CommonScriptableObjects.playerCoords.Get(), Vector2Int.zero);
    }

    private void OnPlayerCoordsChanged(Vector2Int current, Vector2Int previous) { renderer.material.SetVector(PLAYER_POSITION, (Vector2)current); }

    private void OnMapTextureChanged(Texture current, Texture previous) { renderer.material.SetTexture(MAP_PROPERTY, current); }

    private void OnEstateIdTextureChanged(Texture current, Texture previous) { renderer.material.SetTexture(ESTATE_ID_PROPERTY, current); }

    void OnWorldReposition(UnityEngine.Vector3 current, UnityEngine.Vector3 previous) { transform.position = -current; }

    void OnDisable()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
        DataStore.i.HUDs.mapMainTexture.OnChange -= OnMapTextureChanged;
        DataStore.i.HUDs.mapEstatesTexture.OnChange -= OnEstateIdTextureChanged;
        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChanged;
    }
}
