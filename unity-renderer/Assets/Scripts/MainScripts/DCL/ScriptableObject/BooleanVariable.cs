using UnityEngine;

[CreateAssetMenu(fileName = "BooleanVariable", menuName = "BooleanVariable")]
public class BooleanVariable : BaseVariable<bool>
{
    public override bool Equals(bool other)
    {
        return other == value;
    }

}