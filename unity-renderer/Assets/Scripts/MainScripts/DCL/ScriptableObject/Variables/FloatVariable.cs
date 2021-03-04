using UnityEngine;

[CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable")]
public class FloatVariable : BaseVariableAsset<float>
{
    public override bool Equals(float other)
    {
        return other == value;
    }

}
