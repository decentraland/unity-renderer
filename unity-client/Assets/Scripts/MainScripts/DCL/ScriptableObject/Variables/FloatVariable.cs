using UnityEngine;

[CreateAssetMenu(fileName = "FloatVariable", menuName = "FloatVariable")]
public class FloatVariable : BaseVariable<float>
{
    public override bool Equals(float other)
    {
        return other == value;
    }

}
