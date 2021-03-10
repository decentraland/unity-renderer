using UnityEngine;

[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable")]
public class IntVariable : BaseVariableAsset<int>
{
    public override bool Equals(int other)
    {
        return other == value;
    }
}
