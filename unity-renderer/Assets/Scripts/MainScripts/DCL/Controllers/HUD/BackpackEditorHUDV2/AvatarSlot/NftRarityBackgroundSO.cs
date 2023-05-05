using UnityEngine;

[CreateAssetMenu(fileName = "NftRarityBackground", menuName = "Variables/NftRarityBackground")]
public class NftRarityBackgroundSO : ScriptableObject
{
    private const string DEFAULT_RARITY = "default";
    [SerializeField] public SerializableKeyValuePair<string, Sprite>[] rarityIcons;

    public Sprite GetRarityImage(string nftRarity)
    {
        nftRarity = string.IsNullOrEmpty(nftRarity) ? DEFAULT_RARITY : nftRarity;
        foreach (var icon in rarityIcons)
        {
            if(icon.key == nftRarity)
                return icon.value;
        }

        return null;
    }
}
