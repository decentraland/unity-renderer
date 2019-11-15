using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimapMetadata", menuName = "MinimapMetadata")]
public class MinimapMetadata : ScriptableObject
{
    public enum TileType
    {
        MyParcel = 0,
        MyParcelsOnSale = 1,
        MyEstates = 2,
        MyEstatesOnSale = 3,
        WithAccess = 4,
        District = 5,
        Contribution = 6,
        Roads = 7,
        Plaza = 8,
        Taken = 9,
        OnSale= 10,
        Unowned= 11, 
        Background = 12, 
        Loading = 13
    }

    [Serializable]
    public class Tile
    {
        public TileType tileType;
        public Color color;
        public string name;

        public Tile(int tileType, string name = "") : this((TileType)tileType, name) { }
        public Tile(TileType tileType, string name = "")
        {
            this.tileType = tileType;
            this.name = name;

            //This must be replaced by standard Color type when explorer send the model in a proper way
            MinimapLiteral.TryGetTileColor(this.tileType, out color);
        }
    }

    public event Action<(int, int), Tile> OnTileUpdated;
    public event Action<(int, int)> OnTileRemoved;
    
    private readonly Dictionary<(int, int), Tile> tiles = new Dictionary<(int, int), Tile>();

    public void Clear()
    {
        tiles.Clear();
    }

    public void SetTile(int x, int y, Tile tile )
    {
        if (tiles.ContainsKey((x, y)))
        {
            tiles[(x, y)] = tile;
        }
        else
        {
            tiles.Add((x,y), tile);
        }
        OnTileUpdated?.Invoke((x,y), tile);
    }

    public Tile GetTile(int x, int y)
    {
        if (!tiles.ContainsKey((x, y)))
        {
            return null;
        }

        return tiles[(x, y)];
    }

    private static MinimapMetadata minimapMetadata;
    public static MinimapMetadata GetMetadata()
    {
        if (minimapMetadata == null)
        {
            minimapMetadata = Resources.Load<MinimapMetadata>("ScriptableObjects/MinimapMetadata");
        }

        return minimapMetadata;
    }
}

public static class MinimapLiteral
{
    private static readonly Dictionary<MinimapMetadata.TileType, string> tileColorMapping = new Dictionary<MinimapMetadata.TileType, string>()
    {
        { MinimapMetadata.TileType.MyParcel, "#ff9990" },//Color on MktPlace #ff9990
        { MinimapMetadata.TileType.MyParcelsOnSale, "#ff9990" },//Color on MktPlace #ec5159
        { MinimapMetadata.TileType.MyEstates, "#ff9990" },//Color on MktPlace #ff9990
        { MinimapMetadata.TileType.MyEstatesOnSale, "#ff9990" },//Color on MktPlace #ff4053
        { MinimapMetadata.TileType.WithAccess, "#33303B" },//Color on MktPlace #ffbd33
        { MinimapMetadata.TileType.District, "#5054D4" },//Color on MktPlace #4f57cc
        { MinimapMetadata.TileType.Contribution, "#563db8" },
        { MinimapMetadata.TileType.Roads, "#525D67" },//Color on MktPlace #706c79
        { MinimapMetadata.TileType.Plaza, "#3FB86F" },//Color on MktPlace #7daa7b
        { MinimapMetadata.TileType.Taken, "#33303B" },//Color on MktPlace #3c3a45
        { MinimapMetadata.TileType.OnSale, "#33303B" },//Color on MktPlace #5ed0fa
        { MinimapMetadata.TileType.Unowned, "#33303B" },//Color on MktPlace #09080A
        { MinimapMetadata.TileType.Background, "#000000" },//Color on MktPlace #18141a
        { MinimapMetadata.TileType.Loading, "#110e13" }
    };

    private static readonly Dictionary<string, Color> cachedHTMLToColorMapping = new Dictionary<string, Color>();

    public static bool TryGetTileColor(MinimapMetadata.TileType type, out Color color)
    {
        color = Color.black;
        if(!tileColorMapping.ContainsKey(type))
            return false;

        var htmlColor = tileColorMapping[type];
        if (!cachedHTMLToColorMapping.ContainsKey(htmlColor))
        {
            if(!ColorUtility.TryParseHtmlString(tileColorMapping[type], out Color newColor))
            {
                return false;
            }
            cachedHTMLToColorMapping.Add(htmlColor, newColor);
        }

        color = cachedHTMLToColorMapping[htmlColor];
        return true;
    }
}