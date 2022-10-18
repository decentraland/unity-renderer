using DCL;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    private static readonly int MAP_PROPERTY = Shader.PropertyToID("_Map");
    private static readonly int ESTATE_ID_PROPERTY = Shader.PropertyToID("_EstateIDMap");

    [SerializeField] private Renderer renderer;

    void OnEnable()
    {
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;

        DataStore.i.HUDs.mapMainTexture.OnChange += OnMapTextureChanged;
        OnMapTextureChanged(DataStore.i.HUDs.mapMainTexture.Get(), null);

        DataStore.i.HUDs.mapEstatesTexture.OnChange += OnEstateIdTextureChanged;
        OnEstateIdTextureChanged(DataStore.i.HUDs.mapEstatesTexture.Get(), null);
    }

    private void OnMapTextureChanged(Texture current, Texture previous) { renderer.material.SetTexture(MAP_PROPERTY, current); }

    private void OnEstateIdTextureChanged(Texture current, Texture previous) { renderer.material.SetTexture(ESTATE_ID_PROPERTY, current); }

    void OnWorldReposition(UnityEngine.Vector3 current, UnityEngine.Vector3 previous) { transform.position = -current; }

    void OnDisable()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
        DataStore.i.HUDs.mapMainTexture.OnChange -= OnMapTextureChanged;
        DataStore.i.HUDs.mapEstatesTexture.OnChange -= OnEstateIdTextureChanged;
    }
}