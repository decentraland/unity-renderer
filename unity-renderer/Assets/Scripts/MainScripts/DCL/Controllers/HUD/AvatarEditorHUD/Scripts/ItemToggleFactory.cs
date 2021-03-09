using System.Collections.Generic;
using UnityEngine;

public class ItemToggleFactory : ScriptableObject
{
    [System.Serializable]
    public class NftMap
    {
        public string rarity;
        public ItemToggle prefab;
    }

    public ItemToggle baseWearable;
    public NftMap[] nftList;

    internal Dictionary<string, NftMap> nftDictionary;

    public void EnsureNFTDictionary()
    {
        if (nftDictionary == null)
        {
            nftDictionary = new Dictionary<string, NftMap>();

            for (int i = 0; i < nftList.Length; i++)
            {
                NftMap nftMap = nftList[i];

                if (!nftDictionary.ContainsKey(nftMap.rarity))
                {
                    nftDictionary.Add(nftMap.rarity, nftMap);
                }
            }
        }
    }

    public ItemToggle CreateBaseWearable(Transform parent = null)
    {
        return parent == null ? Instantiate(baseWearable) : Instantiate(baseWearable, parent);
    }

    public ItemToggle CreateItemToggleFromRarity(string rarity, Transform parent = null)
    {
        EnsureNFTDictionary();

        if (!nftDictionary.ContainsKey(rarity))
        {
#if UNITY_EDITOR
            Debug.LogError("Item toggle of rarity " + rarity + " can't be instantiated because it does not exist in the factory!");
#endif
            return CreateBaseWearable(parent);
        }

        if (nftDictionary[rarity].prefab == null)
        {
            Debug.LogError("Prefab for rarity " + rarity + " is null!");
            return CreateBaseWearable(parent);
        }

        return parent == null ? Instantiate(nftDictionary[rarity].prefab) : Instantiate(nftDictionary[rarity].prefab, parent);
    }
}