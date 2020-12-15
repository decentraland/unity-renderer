using UnityEngine;

[CreateAssetMenu(fileName = "BooleanVariable", menuName = "Variables/BooleanVariable")]
public class BooleanVariable : BaseVariableAsset<bool>
{
    public override bool Equals(bool other)
    {
        return other == value;
    }

}