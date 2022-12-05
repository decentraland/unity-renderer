using UnityEngine;

public class NftPageView : MonoBehaviour
{

    [SerializeField] private NFTIconComponentView[] nftElements;

    public void SetPageElementsContent(NFTIconComponentModel[] nftModels)
    {
        for (int i = 0; i < nftModels.Length; i++)
        {
            nftElements[i].Configure(nftModels[i]);
        }
    }

}
