using UnityEngine;

[CreateAssetMenu(fileName = "NftTypeColorSupporting", menuName = "Variables/NftTypeColorSupporting")]
public class NftTypeColorSupportingSO : ScriptableObject
{
    [SerializeField] public SerializableKeyValuePair<string, bool>[] colorSupportingByNftType;

    public bool IsColorSupportedByType(string nftType)
    {
        foreach (var icon in colorSupportingByNftType)
        {
            if(icon.key == nftType)
                return icon.value;
        }
        return false;
    }
}
