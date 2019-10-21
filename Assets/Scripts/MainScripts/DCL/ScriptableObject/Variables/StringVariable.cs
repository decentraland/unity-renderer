using UnityEngine;

[CreateAssetMenu(fileName = "StringVariable", menuName = "StringVariable")]
public class StringVariable : BaseVariable<string>
{
    public override bool Equals(string other)
    {
        return value == other;
    }
}
