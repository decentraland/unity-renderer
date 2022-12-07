using System;
using UnityEngine;

public class NftPageView : BaseComponentView
{

    [SerializeField] private NFTIconComponentView[] nftElements;

    public void SetPageElementsContent(NFTIconComponentModel[] nftModels)
    {
        for (int i = 0; i < nftModels.Length; i++)
        {
            nftElements[i].Configure(nftModels[i]);
        }
    }

    public override void RefreshControl()
    {
    }
}
