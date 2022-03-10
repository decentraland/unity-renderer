using System.Collections.Generic;
using System.Linq;
using DCL.Emotes;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarAnimationsVariable", menuName = "AvatarAnimationsVariable")]
public class AvatarAnimationsVariable : BaseVariableAsset<AvatarAnimation[]>
{
    public override bool Equals(AvatarAnimation[] other)
    {
        if (value.Length != other.Length)
            return false;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != other[i])
                return false;
        }

        return true;
    }

    //Util context menu to convert from current implementation to new one
    //If you see this in a review, dont let it go through!!!
    [ContextMenu("To Embedded Emote")]
    private void PortToEmbeddedEmote()
    {
        EmbeddedEmotesSO embeddedEmotesSo = Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes");
        Dictionary<string, EmbeddedEmote> current = embeddedEmotesSo.emotes?.ToDictionary(x => x.id, x => x) ?? new Dictionary<string, EmbeddedEmote>();
        foreach (AvatarAnimation avatarAnimation in value)
        {
            if (!current.TryGetValue(avatarAnimation.id, out EmbeddedEmote emote))
            {
                emote = new EmbeddedEmote()
                {
                    id = avatarAnimation.id,
                    description = avatarAnimation.id,
                };
                current.Add(avatarAnimation.id, emote);
            }
            if (name.ToLower().Contains("female"))
                emote.femaleAnimation = avatarAnimation.clip;
            else
                emote.maleAnimation = avatarAnimation.clip;
        }

        embeddedEmotesSo.emotes = current.Values.ToArray();
    }
}

[System.Serializable]
public class AvatarAnimation
{
    public AnimationClip clip;
    public string id;
}