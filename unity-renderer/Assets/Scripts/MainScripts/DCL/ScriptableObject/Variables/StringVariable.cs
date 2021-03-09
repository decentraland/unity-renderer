using UnityEngine;

[CreateAssetMenu(fileName = "StringVariable", menuName = "Variables/StringVariable")]
public class StringVariable : BaseVariableAsset<string>
{
    public override bool Equals(string other)
    {
        return value == other;
    }
}
