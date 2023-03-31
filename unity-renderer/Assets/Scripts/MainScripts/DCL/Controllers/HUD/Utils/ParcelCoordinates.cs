using UnityEngine;

public class ParcelCoordinates
{
    public int x { get; private set; }
    public int y { get; private set; }

    public ParcelCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2Int(ParcelCoordinates coords) =>
        new (coords.x, coords.y);

    public override string ToString() =>
        $"{x},{y}";
}
