using UnityEngine;

[CreateAssetMenu(fileName = "LongVariable", menuName = "Variables/LongVariable")]
public class LongVariable : BaseVariableAsset<long>
{
    public override bool Equals(long other)
    {
        return other == value;
    }

}
