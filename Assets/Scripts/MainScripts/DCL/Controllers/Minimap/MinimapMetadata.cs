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

    //This must be replaced by standard Color type when explorer send the model in a proper way
    private static readonly Dictionary<TileType, string> TileColors = new Dictionary<TileType, string>()
    {
        { TileType.MyParcel, "#ff9990" },
        { TileType.MyParcelsOnSale, "#ff4053" },
        { TileType.MyEstates, "#ff9990" },
        { TileType.MyEstatesOnSale, "#ff4053" },
        { TileType.WithAccess, "#ffbd33" },
        { TileType.District, "#5054D4" },
        { TileType.Contribution, "#563db8" },
        { TileType.Roads, "#716C7A" },
        { TileType.Plaza, "#70AC76" },
        { TileType.Taken, "#3D3A46" },
        { TileType.OnSale, "#00d3ff" },
        { TileType.Unowned, "#09080A" },
        { TileType.Background, "#18141a" },
        { TileType.Loading, "#110e13" }
    };

    [Serializable]
    public class Tile
    {
        public Vector2Int position;
        public TileType tileType;
        public Color color;
        public string name;

        public Tile(Vector2Int position, int tileType, string name = "") : this(position, (TileType)tileType, name) { }
        public Tile(Vector2Int position, TileType tileType, string name = "")
        {
            this.position = position;
            this.tileType = tileType;
            this.name = name;

            //This must be replaced by standard Color type when explorer send the model in a proper way
            ColorUtility.TryParseHtmlString(TileColors[tileType], out color);
        }

    }

    [Serializable]
    public class Model
    {
        public Vector2Int bottomLeftCorner;
        public Vector2Int topRightCorner;
        public Tile[] tiles = new Tile[0];

        public int rowCount => (topRightCorner.y - bottomLeftCorner.y) + 1;
        public int colCount => (topRightCorner.x - bottomLeftCorner.x) + 1;

        public Model() { }

        public Model(Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
        {
            this.bottomLeftCorner = bottomLeftCorner;
            this.topRightCorner = topRightCorner;
            tiles = new Tile[rowCount * colCount];
        }

        public void AddTile(int x, int y, Tile tile)
        {
            int index = IndexFromCoordinates(x, y);

            if (index < 0 || index >= tiles.Length) throw new Exception($"Calculated Index: {index} for tile ({x},{y}) should be in the range [0,{tiles.Length})");

            tiles[index] = tile;
        }

        public Tile GetTile(int x, int y)
        {
            int index = IndexFromCoordinates(x, y);

            if (index < 0 || index >= tiles.Length) throw new Exception($"Calculated Index: {index} for tile ({x},{y}) should be in the range [0,{tiles.Length})");

            return tiles[IndexFromCoordinates(x, y)];
        }

        public int IndexFromCoordinates(int x, int y)
        {
            return ((x - bottomLeftCorner.x) * ((topRightCorner.y - bottomLeftCorner.y) + 1) + (y - bottomLeftCorner.y));
        }
    }

    public event Action<MinimapMetadata> OnChange = (x) => { };
    [SerializeField] private Model model;

    public void UpdateData(Model newModel)
    {
        model = newModel;
        OnChange(this);
    }

    public Tile GetTile(int x, int y) => model?.GetTile(x, y);

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