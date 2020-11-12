using UnityEngine;

[CreateAssetMenu(fileName = "BooleanVariable", menuName = "Variables/BooleanVariable")]
public class BooleanVariable : BaseVariable<bool>
{
    public override bool Equals(bool other)
    {
        return other == value;
    }

}