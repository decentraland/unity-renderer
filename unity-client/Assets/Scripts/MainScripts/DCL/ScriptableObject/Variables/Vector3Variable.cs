using UnityEngine;

[CreateAssetMenu(fileName = "Vector3Variable", menuName = "Variables/Vector3Variable")]
public class Vector3Variable : BaseVariable<Vector3>
{
    public override bool Equals(Vector3 other)
    {
        return value == other;
    }
}
