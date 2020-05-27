using UnityEngine;

[CreateAssetMenu(fileName = "LongVariable", menuName = "LongVariable")]
public class LongVariable : BaseVariable<long>
{
    public override bool Equals(long other)
    {
        return other == value;
    }

}
