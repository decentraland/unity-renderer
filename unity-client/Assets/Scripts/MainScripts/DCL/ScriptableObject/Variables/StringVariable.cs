using UnityEngine;

[CreateAssetMenu(fileName = "StringVariable", menuName = "Variables/StringVariable")]
public class StringVariable : BaseVariable<string>
{
    public override bool Equals(string other)
    {
        return value == other;
    }
}
