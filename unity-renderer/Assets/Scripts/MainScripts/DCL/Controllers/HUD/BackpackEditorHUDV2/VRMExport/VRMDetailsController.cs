using Cysharp.Threading.Tasks;
using DCLServices.EnvironmentProvider;
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
        private readonly INFTFetchHelper nftFetchHelper;
        private IEnvironmentProviderService environmentProviderService;

        public event Action<string, UnequipWearableSource> OnWearableUnequipped;
        public event Action<string, EquipWearableSource> OnWearableEquipped;

        public VRMDetailsController(
            IVRMDetailsComponentView view,
            IUserProfileBridge userProfileBridge,
            INFTFetchHelper nftFetchHelper,
            IEnvironmentProviderService environmentProviderService)
        {
            this.view = view;
            this.userProfileBridge = userProfileBridge;
            this.nftFetchHelper = nftFetchHelper;
            this.environmentProviderService = environmentProviderService;

            view.OnWearableUnequipped += HandleWearableUnequipped;
            view.OnWearableEquipped += HandleWearableEquipped;
        }

        public void Dispose()
        {
            view.OnWearableUnequipped -= HandleWearableUnequipped;
            view.OnWearableEquipped -= HandleWearableEquipped;
        }

        private void HandleWearableEquipped(VRMItemModel vrmItemModel, EquipWearableSource equipWearableSource)
        {
            OnWearableEquipped?.Invoke(vrmItemModel.wearableUrn, equipWearableSource);
        }

        private void HandleWearableUnequipped(VRMItemModel vrmItemModel, UnequipWearableSource unEquipWearableSource)
        {
            OnWearableUnequipped?.Invoke(vrmItemModel.wearableUrn, unEquipWearableSource);
        }

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="vrmBlockingWearables"> A dictionary where the Key is the wearable urn and the Value is whether the wearable can be unequipped</param>
        public void Initialize(Dictionary<string, bool> vrmBlockingWearables)
        {
            PopulateDetails(vrmBlockingWearables).Forget();
        }

        private async UniTask PopulateDetails(Dictionary<string, bool> vrmBlockingWearables)
        {
            List<NFTDataDTO> itemsToDisplay = new ();

            // Copy the list from params since it's used for other checks elsewhere
            Dictionary<string, bool> itemsToProcess = new (vrmBlockingWearables);

            // Get cached items first, to reduce api calls.
            foreach ((string id, NFTDataDTO data) in nftDataCache)
                if (itemsToProcess.ContainsKey(id))
                {
                    itemsToDisplay.Add(data);
                    itemsToProcess.Remove(id);
                }

            // If we're still missing items, fetch them.
            if (itemsToProcess.Count > 0)
            {
                string nftItemRawData = await nftFetchHelper.GetNFTItems(new List<string>(itemsToProcess.Keys), environmentProviderService);
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

                    nftDataDto.canBeUnEquipped = itemsToProcess[nftDataDto.urn];

                    nftDataCache.Add(nftDataDto.urn, nftDataDto);
                    itemsToDisplay.Add(nftDataDto);
                }
            }

            view.FillVRMBlockingWearablesList(itemsToDisplay);
        }
    }
}
