using System;
using UnityEngine;

public class NftPageView : BaseComponentView
{

    public event Action<string> OnClickBuyNft;

    [SerializeField] private NFTIconComponentView[] nftElements;
    private string[] nftIds = new string[4];

    public void SetPageElementsContent(NFTIconComponentModel[] nftModels, string[] ids)
    {
        nftIds = ids;
        for (int i = 0; i < nftModels.Length; i++)
        {
            if (nftModels[i] != null)
            {
                nftElements[i].gameObject.SetActive(true);
                nftElements[i].Configure(nftModels[i]);
                nftElements[i].onMarketplaceButtonClick.RemoveAllListeners();
                int idIndex = i;
                nftElements[i].onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(idIndex));
            }
            else
            {
                nftElements[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClickOnBuyWearable(int index)
    {
        OnClickBuyNft?.Invoke(nftIds[index]);
    }

    public override void RefreshControl()
    {
    }
}
