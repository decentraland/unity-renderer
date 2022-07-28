using System;
using System.Collections.Generic;
using UnityEngine;

public class NFTSkinFactory : ScriptableObject
{
    [System.Serializable]
    public class NftMap
    {
        public string rarity;
        public NFTItemToggleSkin skin;
    }
    
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

    public NFTItemToggleSkin GetSkinForRarity(string rarity)
    {
        EnsureNFTDictionary();
        
        if (string.IsNullOrEmpty(rarity))
            return nftDictionary["base"].skin;
        
        if (!nftDictionary.ContainsKey(rarity))
            return nftDictionary["base"].skin;
        
        return nftDictionary[rarity].skin;
    }
}