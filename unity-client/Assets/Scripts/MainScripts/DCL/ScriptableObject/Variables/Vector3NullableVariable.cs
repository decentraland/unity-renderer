using UnityEngine;

[CreateAssetMenu(fileName = "Vector3NullableVariable", menuName = "Variables/Vector3NullableVariable")]
public class Vector3NullableVariable : BaseVariable<Vector3?>
{
    public bool HasValue()
    {
        return value.HasValue;
    }

    public override bool Equals(Vector3? other)
    {
        return value == other;
    }
}