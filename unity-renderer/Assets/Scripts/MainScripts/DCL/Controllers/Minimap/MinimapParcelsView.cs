using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

//(Alex) In the future explorer could provide non-existent parcels as well, we could then remove this and simply add a basic logic to change the color of a representation in ParcelScene
public class MinimapParcelsView : MonoBehaviour
{
    private const string PARCEL_ICON_PREFAB = "ParcelMinimapIcon";

    private static GameObject iconPrefab;
    private readonly Dictionary<(int, int), Renderer> parcelsIcon = new Dictionary<(int, int), Renderer>();

    public int sideCount = 6;
    private static readonly Vector3 pivotOffset = (new Vector3(0.5f, 0, 0.5f) * ParcelSettings.PARCEL_SIZE);

    private Vector2IntVariable playerCoords => CommonScriptableObjects.playerCoords;
    private Vector3Variable playerUnityToWorldOffset => CommonScriptableObjects.playerUnityToWorldOffset;
    private MinimapMetadata minimapmetadata => MinimapMetadata.GetMetadata();

    private Vector3Variable.Change unityWorldOffsetDelegate;

    public static int _BaseColor = Shader.PropertyToID("_BaseColor");

    private void Start()
    {
        SetupIcons();
        playerCoords.OnChange += OnCharacterSetPosition;
        minimapmetadata.OnTileUpdated += TileUpdated;
        minimapmetadata.OnTileRemoved += TileRemoved;

        unityWorldOffsetDelegate = (current,  previous) => Recenter();
        playerUnityToWorldOffset.OnChange += unityWorldOffsetDelegate;

        DrawParcels(Vector2Int.zero);
    }

    public void SetupIcons()
    {
        if (iconPrefab == null)
        {
            iconPrefab = Resources.Load<GameObject>(PARCEL_ICON_PREFAB);
        }
        for (int i = -sideCount; i <= sideCount; i++)
        {
            for (int j = -sideCount; j <= sideCount; j++)
            {
                var icon = Instantiate(iconPrefab, transform);
                icon.transform.position = Utils.GridToWorldPosition(i, j);
                parcelsIcon.Add((i, j), icon.GetComponent<Renderer>());
            }
        }
    }

    public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
    {
        DrawParcels(newCoords);
    }

    public void DrawParcels(Vector2Int newCoords)
    {
        Recenter();

        foreach (var keyValuePair in parcelsIcon)
        {
            int x = newCoords.x + keyValuePair.Key.Item1;
            int y = newCoords.y + keyValuePair.Key.Item2;
            var tile = minimapmetadata.GetTile(x, y);
            keyValuePair.Value.material.SetColor(_BaseColor, tile?.color ?? Color.grey);
        }
    }

    private void TileUpdated((int, int) pos, MinimapMetadata.Tile tile)
    {
        DrawParcels(playerCoords.Get());
    }

    private void TileRemoved((int, int) pos)
    {
        DrawParcels(playerCoords.Get());
    }

    private void Recenter()
    {
        transform.position = Utils.GridToWorldPosition(((Vector2Int)playerCoords).x, ((Vector2Int)playerCoords).y) - playerUnityToWorldOffset + pivotOffset;
    }

    private void OnDestroy()
    {
        playerCoords.OnChange -= OnCharacterSetPosition;
        minimapmetadata.OnTileUpdated -= TileUpdated;
        minimapmetadata.OnTileRemoved -= TileRemoved;
        playerUnityToWorldOffset.OnChange -= unityWorldOffsetDelegate;
    }
}