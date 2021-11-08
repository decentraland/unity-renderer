using DCL.Configuration;
using DCL.Helpers.NFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Note: we should make this class a part of BIWController and only fetch the NFTs if the biw is active instead of only 
// when you enter builder in world
public class BIWNFTController
{
    public event System.Action OnNFTUsageChange;
    public event System.Action<List<NFTInfo>> OnNftsFetched;

    private NFTOwner nftOwner;

    private Coroutine fechNftsCoroutine;

    private static BIWNFTController instance;

    private List<NFTInfo> nftsAlreadyInUse = new List<NFTInfo>();

    private bool desactivateNFT = false;
    private bool isInit = false;
    private bool isActive = false;
    private UserProfile userProfile;

    public static BIWNFTController i
    {
        get
        {
            if (instance == null)
            {
                instance = new BIWNFTController();
            }

            return instance;
        }
    }

    public void StartFetchingNft()
    {
        if (!isInit)
        {
            userProfile = UserProfile.GetOwnUserProfile();
            userProfile.OnUpdate += UserProfileUpdated;
            FetchNftsFromOwner();
            isInit = true;
        }
    }

    private void UserProfileUpdated(UserProfile profile)
    {
        if (!isActive)
            return;

        FetchNftsFromOwner();
    }

    public void Dispose()
    {
        if (userProfile != null)
            userProfile.OnUpdate -= UserProfileUpdated;

        if (fechNftsCoroutine != null)
            CoroutineStarter.Stop(fechNftsCoroutine);
    }

    public void StartEditMode()
    {
        isActive = true;
        ClearNFTs();
    }

    public void ExitEditMode() { isActive = false; }

    public void ClearNFTs() { nftsAlreadyInUse.Clear(); }

    public bool IsNFTInUse(string id)
    {
        if (desactivateNFT)
            return false;

        foreach (NFTInfo info in nftsAlreadyInUse)
        {
            if (info.assetContract.address == id)
                return true;
        }

        return false;
    }

    public void StopUsingNFT(string id)
    {
        if (desactivateNFT)
            return;

        foreach (NFTInfo info in nftOwner.assets)
        {
            if (info.assetContract.address != id)
                continue;
            if (!nftsAlreadyInUse.Contains(info))
                continue;

            nftsAlreadyInUse.Remove(info);
            OnNFTUsageChange?.Invoke();
        }
    }

    public List<NFTInfo> GetNfts() { return nftOwner.assets; }

    public void UseNFT(string id)
    {
        if (desactivateNFT || nftOwner.assets == null)
            return;

        foreach (NFTInfo info in nftOwner.assets)
        {
            if (info.assetContract.address != id)
                continue;
            if (nftsAlreadyInUse.Contains(info))
                continue;

            nftsAlreadyInUse.Add(info);
            OnNFTUsageChange?.Invoke();
        }
    }

    private void FetchNftsFromOwner()
    {
        if (fechNftsCoroutine != null)
            CoroutineStarter.Stop(fechNftsCoroutine);
        fechNftsCoroutine = CoroutineStarter.Start(FetchNfts());
    }

    public void NftsFeteched(NFTOwner nftOwner)
    {
        this.nftOwner = nftOwner;
        string json = JsonUtility.ToJson(nftOwner);
        desactivateNFT = false;
        OnNftsFetched?.Invoke(this.nftOwner.assets);
    }

    private IEnumerator FetchNfts()
    {
        string userId = userProfile.ethAddress;
        if (!string.IsNullOrEmpty(userId))
        {
            yield return NFTUtils.FetchNFTsFromOwner(userId, (nftOwner) =>
                {
                    NftsFeteched(nftOwner);
                },
                (error) =>
                {
                    desactivateNFT = true;
                    Debug.LogError($"error getting NFT from owner:  {error}");
                });
        }
    }
}