using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NFTTypeIconsAndColors", menuName = "Variables/NFTTypeIconsAndColors")]
public class NFTTypeIconsAndColors : ScriptableObject
{
    [SerializeField] SerializableKeyValuePair<string, Sprite>[] nftIcons;
    [SerializeField] SerializableKeyValuePair<string, Color>[] nftColors;

    public Sprite GetTypeImage(string nftType)
    {
        foreach (var t in nftIcons)
        {
            if(t.key == nftType)
                return t.value;
        }

        return null;
    }

    public Color GetColor(string rarity)
    {
        foreach (var t in nftColors)
        {
            if(t.key == rarity)
                return t.value;
        }

        return Color.white;
    }
}
