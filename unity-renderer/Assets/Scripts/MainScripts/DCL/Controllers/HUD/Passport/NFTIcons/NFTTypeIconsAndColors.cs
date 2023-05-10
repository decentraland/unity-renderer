using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NFTTypeIconsAndColors", menuName = "Variables/NFTTypeIconsAndColors")]
public class NFTTypeIconsAndColors : ScriptableObject
{
    [SerializeField] SerializableKeyValuePair<string, Sprite>[] nftIcons;
    [SerializeField] SerializableKeyValuePair<string, Color>[] nftColors;
    [SerializeField] Color defaultColor;

    public Sprite GetTypeImage(string nftType)
    {
        foreach (var icon in nftIcons)
        {
            if (icon.key == nftType)
                return icon.value;
        }

        return null;
    }

    public Color GetColor(string rarity)
    {
        foreach (var color in nftColors)
        {
            if (color.key == rarity)
                return color.value;
        }

        return defaultColor;
    }
}
