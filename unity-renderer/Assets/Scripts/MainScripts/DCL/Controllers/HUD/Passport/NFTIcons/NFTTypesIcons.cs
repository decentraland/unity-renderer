using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NFTTypesIcons", menuName = "Variables/NFTTypesIcons")]
public class NFTTypesIcons : ScriptableObject
{
    [SerializeField] SerializableKeyValuePair<string, Sprite>[] nftIcons;

    public Sprite GetTypeImage(string nftType)
    {
        for(int i=0; i< nftIcons.Length; i++)
        {
            if(nftIcons[i].key == nftType)
                return nftIcons[i].value;
        }

        return null;
    }
}