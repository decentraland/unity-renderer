using UnityEngine;

[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable")]
public class IntVariable : BaseVariable<int>
{
    public override bool Equals(int other)
    {
        return other == value;
    }
}
