using Cysharp.Threading.Tasks;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class VRMDetailsController : IDisposable
    {
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IVRMDetailsComponentView view;
        private readonly Dictionary<string, NFTDataDTO> nftDataCache = new ();

        public event Action<string, UnequipWearableSource> OnWearableUnequipped;
        public event Action<string, EquipWearableSource> OnWearableEquipped;

        public VRMDetailsController(
            IVRMDetailsComponentView view,
            IUserProfileBridge userProfileBridge)
        {
            this.view = view;
            this.userProfileBridge = userProfileBridge;

            view.OnWearableUnequipped += HandleWearableUnequipped;
            view.OnWearableEquipped += HandleWearableEquipped;
        }

        public void Dispose()
        {
            view.OnWearableUnequipped -= HandleWearableUnequipped;
            view.OnWearableEquipped += HandleWearableEquipped;
        }

        private void HandleWearableEquipped(VRMItemModel vrmItemModel, EquipWearableSource equipWearableSource)
        {
            OnWearableEquipped?.Invoke(vrmItemModel.wearableUrn, equipWearableSource);
        }

        private void HandleWearableUnequipped(VRMItemModel vrmItemModel, UnequipWearableSource unEquipWearableSource)
        {
            OnWearableUnequipped?.Invoke(vrmItemModel.wearableUrn, unEquipWearableSource);
        }

        public void Initialize(List<string> vrmBlockingWearablesList)
        {
            PopulateDetails(vrmBlockingWearablesList).Forget();
        }

        private async UniTask PopulateDetails(List<string> vrmBlockingWearablesList)
        {
            List<NFTDataDTO> itemsToDisplay = new ();
            // Copy the list from params since it's used for other checks elsewhere
            List<string> itemsToProcess = new (vrmBlockingWearablesList);

            // Get cached items first, to reduce api calls.
            for (int i = itemsToProcess.Count - 1; i >= 0; i--)
                if (nftDataCache.TryGetValue(itemsToProcess[i], out NFTDataDTO itemData))
                {
                    itemsToDisplay.Add(itemData);
                    itemsToProcess.RemoveAt(i);
                }

            // If we're still missing items, fetch them.
            if (itemsToProcess.Count > 0)
            {
                string nftItemRawData = await WearablesFetchingHelper.GetNFTItems(itemsToProcess);
                var nftItemsData = JsonUtility.FromJson<NFTItemsDTO>(nftItemRawData);

                for (var i = 0; i < nftItemsData.data.Length; i++)
                {
                    NFTDataDTO nftDataDto = nftItemsData.data[i];
                    var creatorProfile = userProfileBridge.Get(nftDataDto.creator);

                    if (creatorProfile == null)
                    {
                        creatorProfile = await userProfileBridge.RequestFullUserProfileAsync(nftDataDto.creator);

                        if (creatorProfile == null)
                        {
                            Debug.LogError($"Creator {nftDataDto.creator} profile could not be retrieved!");
                            nftDataDto.creatorName = "User not found";
                            nftDataDto.creatorImageUrl = string.Empty;
                        }
                    }

                    if (creatorProfile != null)
                    {
                        nftDataDto.creatorName = creatorProfile.userName;
                        nftDataDto.creatorImageUrl = creatorProfile.face256SnapshotURL;
                    }

                    nftDataCache.Add(nftDataDto.urn, nftDataDto);
                    itemsToDisplay.Add(nftDataDto);
                }
            }

            view.FillVRMBlockingWearablesList(itemsToDisplay);
        }
    }

#region DTO structure to retrieve NFT item data
    [Serializable]
    public struct NFTItemsDTO
    {
        public NFTDataDTO[] data;
    }

    [Serializable]
    public struct NFTDataDTO
    {
        public string name;
        public string thumbnail;
        public WearableDataDTO data;
        public string creator; // Creator wallet
        public string creatorName;
        public string creatorImageUrl;
        public string urn;
    }

    [Serializable]
    public struct WearableDataDTO
    {
        public WearableDTO wearable;
    }

    [Serializable]
    public struct WearableDTO
    {
        public string category;
    }
#endregion
}
