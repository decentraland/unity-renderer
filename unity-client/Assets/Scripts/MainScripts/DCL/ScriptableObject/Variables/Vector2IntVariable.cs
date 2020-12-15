using UnityEngine;

[CreateAssetMenu(fileName = "Vector2IntVariable", menuName = "Variables/Vector2IntVariable")]
public class Vector2IntVariable : BaseVariableAsset<Vector2Int>
{
    public void Set(Vector2 value)
    {
        base.Set(new Vector2Int((int)value.x, (int)value.y));
    }

    public override bool Equals(Vector2Int other)
    {
        return value == other;
    }
}
