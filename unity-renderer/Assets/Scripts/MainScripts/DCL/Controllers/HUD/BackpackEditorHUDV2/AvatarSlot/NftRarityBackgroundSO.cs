using UnityEngine;

[CreateAssetMenu(fileName = "NftRarityBackground", menuName = "Variables/NftRarityBackground")]
public class NftRarityBackgroundSO : ScriptableObject
{
    [SerializeField] public SerializableKeyValuePair<string, Sprite>[] rarityIcons;

    public Sprite GetRarityImage(string nftRarity)
    {
        foreach (var icon in rarityIcons)
        {
            if(icon.key == nftRarity)
                return icon.value;
        }

        return null;
    }
}
