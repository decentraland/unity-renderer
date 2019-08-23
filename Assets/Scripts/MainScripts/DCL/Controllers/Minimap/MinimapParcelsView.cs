using System.Collections.Generic;
using DCL.Configuration;
using DCL.Helpers;
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

    private void Start()
    {
        SetupIcons();
        playerCoords.onChange += DrawParcels;
        minimapmetadata.OnChange += MinimapMetadataChange;
        DrawParcels(Vector2Int.zero, Vector2Int.zero);
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

    public void DrawParcels(Vector2Int newCoords, Vector2Int oldCoords)
    {
        Recenter();

        foreach (var keyValuePair in parcelsIcon)
        {
            int x = newCoords.x + keyValuePair.Key.Item1;
            int y = newCoords.y + keyValuePair.Key.Item2;
            var tile = minimapmetadata.GetTile(x, y);

            if (tile == null)
                continue;

            keyValuePair.Value.material.SetColor("_BaseColor", tile.color);
        }
    }

    private void MinimapMetadataChange(MinimapMetadata instance)
    {
        DrawParcels(playerCoords.Get(), Vector2Int.zero);
    }

    private void Recenter()
    {
        transform.position = Utils.GridToWorldPosition(((Vector2Int)playerCoords).x, ((Vector2Int)playerCoords).y) - playerUnityToWorldOffset + pivotOffset;
    }

    private void OnDestroy()
    {
        playerCoords.onChange -= DrawParcels;
        minimapmetadata.OnChange -= MinimapMetadataChange;
    }
}