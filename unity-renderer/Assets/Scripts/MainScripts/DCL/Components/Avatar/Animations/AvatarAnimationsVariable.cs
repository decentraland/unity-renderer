using UnityEngine;

[CreateAssetMenu(fileName = "AvatarAnimationsVariable", menuName = "AvatarAnimationsVariable")]
public class AvatarAnimationsVariable : BaseVariableAsset<AvatarAnimation[]>
{
    public override bool Equals(AvatarAnimation[] other)
    {
        if (value.Length != other.Length) return false;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != other[i]) return false;
        }

        return true;
    }
}

[System.Serializable]
public class AvatarAnimation
{
    public AnimationClip clip;
    public string id;
}