using System;
using UnityEngine;

public class NftPageView : BaseComponentView
{

    [SerializeField] private NFTIconComponentView[] nftElements;

    public void SetPageElementsContent(NFTIconComponentModel[] nftModels)
    {
        for (int i = 0; i < nftModels.Length; i++)
        {
            if (nftModels[i] != null)
            {
                nftElements[i].gameObject.SetActive(true);
                nftElements[i].Configure(nftModels[i]);
            }
            else
            {
                nftElements[i].gameObject.SetActive(false);
            }
        }
    }

    public override void RefreshControl()
    {
    }
}
