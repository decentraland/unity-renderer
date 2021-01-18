using DCL.Configuration;
using DCL.Helpers.NFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class BuilderInWorldNFTController
{
    public event System.Action OnNFTUsageChange;
    NFTOwner nftOwner;

    Coroutine fechNftsCoroutine;

    static BuilderInWorldNFTController instance;

    List<NFTInfo> nftsAlreadyInUse = new List<NFTInfo>();

    bool desactivateNFT = false;

    public static BuilderInWorldNFTController i
    {
        get
        {
            if (instance == null)
            {
                instance = new BuilderInWorldNFTController();
            }

            return instance;
        }
    }

    public void Initialize()
    {
        UserProfile userProfile = UserProfile.GetOwnUserProfile();
        userProfile.OnUpdate += (x) => FetchNftsFromOwner();
    }

    public void ClearNFTs()
    {
        nftsAlreadyInUse.Clear();
    }

    public bool IsNFTInUse(string id)
    {
        if (desactivateNFT)
            return false;

        foreach(NFTInfo info in nftsAlreadyInUse)
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
            if (info.assetContract.address != id) continue;
            if (!nftsAlreadyInUse.Contains(info)) continue;

            nftsAlreadyInUse.Remove(info);
            OnNFTUsageChange?.Invoke();
        }
    }

    public void UseNFT(string id)
    {
        if (desactivateNFT)
            return;

        foreach (NFTInfo info in nftOwner.assets)
        {
            if (info.assetContract.address != id) continue;
            if (nftsAlreadyInUse.Contains(info)) continue;

            nftsAlreadyInUse.Add(info);
            OnNFTUsageChange?.Invoke();

        }
    }

    public List<SceneObject> GetNFTsAsSceneObjects()
    {
        List<SceneObject> sceneObjects = new List<SceneObject>();

        if (desactivateNFT ||
            nftOwner.assets == null)
            return sceneObjects;

        foreach(NFTInfo nFTInfo in nftOwner.assets)
        {
            sceneObjects.Add(NFTInfoToSceneObject(nFTInfo));
        }
        return sceneObjects;
    }

    void FetchNftsFromOwner()
    {
        if (fechNftsCoroutine != null) CoroutineStarter.Stop(fechNftsCoroutine);
        fechNftsCoroutine = CoroutineStarter.Start(FetchNfts());
    }

    IEnumerator FetchNfts()
    {
        UserProfile userProfile = UserProfile.GetOwnUserProfile();

        string userId = userProfile.ethAddress;

        yield return NFTHelper.FetchNFTsFromOwner(userId, (nftOwner) =>
        {
            this.nftOwner = nftOwner;
            desactivateNFT = false;
        },
        (error) =>
        {
            desactivateNFT = true;
            Debug.Log($"error getting NFT from owner:  {error}");
        });
    }

    SceneObject NFTInfoToSceneObject(NFTInfo nftInfo)
    {
        SceneObject sceneObject = new SceneObject();
        sceneObject.asset_pack_id = BuilderInWorldSettings.ASSETS_COLLECTIBLES;
        sceneObject.id = nftInfo.assetContract.address;
        sceneObject.thumbnail = nftInfo.thumbnailUrl;
        sceneObject.SetBaseURL(nftInfo.originalImageUrl);
        sceneObject.name = nftInfo.name;
        sceneObject.category = nftInfo.assetContract.name;
        sceneObject.model = BuilderInWorldSettings.COLLECTIBLE_MODEL_PROTOCOL + nftInfo.assetContract.address+"/"+ nftInfo.tokenId;
        sceneObject.tags = new List<string>();
        sceneObject.contents = new Dictionary<string, string>();
        sceneObject.metrics = new SceneObject.ObjectMetrics();

        return sceneObject;
    }
}
