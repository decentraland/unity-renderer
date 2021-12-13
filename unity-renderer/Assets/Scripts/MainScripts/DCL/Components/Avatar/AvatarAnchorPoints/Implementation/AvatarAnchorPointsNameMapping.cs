using System.Collections;
using System.Collections.Generic;
using System.Linq;

internal class AvatarAnchorPointsNameMapping : IEnumerable<KeyValuePair<AvatarAnchorPointIds, string>>
{
    private Dictionary<AvatarAnchorPointIds, string> mapping = new Dictionary<AvatarAnchorPointIds, string>()
    {
        { AvatarAnchorPointIds.LeftHand, "Avatar_LeftHand" },
        { AvatarAnchorPointIds.RightHand, "Avatar_RightHand" },
    };

    public string this[AvatarAnchorPointIds id] => mapping[id];

    public bool TryGet(string boneName, out AvatarAnchorPointIds id)
    {
        var result = mapping.FirstOrDefault(pair => pair.Value == boneName);
        id = result.Key;
        return !string.IsNullOrEmpty(result.Value);
    }

    IEnumerator<KeyValuePair<AvatarAnchorPointIds, string>> IEnumerable<KeyValuePair<AvatarAnchorPointIds, string>>.GetEnumerator()
    {
        return mapping.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return mapping.GetEnumerator();
    }
}