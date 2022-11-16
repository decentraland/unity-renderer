using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NFTTypeIconsAndColors", menuName = "Variables/NFTTypeIconsAndColors")]
public class NFTTypeIconsAndColors : ScriptableObject
{
    [SerializeField] SerializableKeyValuePair<string, Sprite>[] nftIcons;
    [SerializeField] SerializableKeyValuePair<string, Color>[] nftColors;

    public Sprite GetTypeImage(string nftType)
    {
        for(int i=0; i< nftIcons.Length; i++)
        {
            if(nftIcons[i].key == nftType)
                return nftIcons[i].value;
        }

        return null;
    }

    public Color GetColor(string rarity)
    {
        for(int i=0; i< nftColors.Length; i++)
        {
            if(nftColors[i].key == rarity)
                return nftColors[i].value;
        }

        return Color.white;
    }
}