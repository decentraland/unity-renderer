using UnityEngine;

[CreateAssetMenu(fileName = "NftTypeColorSupporting", menuName = "Variables/NftTypeColorSupporting")]
public class NftTypeColorSupportingSO : ScriptableObject
{
    [SerializeField] public SerializableKeyValuePair<string, bool>[] nftTypes;

    public bool GetTypeColorSupporting(string nftType)
    {
        foreach (var icon in nftTypes)
        {
            if(icon.key == nftType)
                return icon.value;
        }
        return false;
    }
}
