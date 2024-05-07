using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotVisiblePersonView : MonoBehaviour
    {
        public static readonly BaseHashSet<ScreenshotVisiblePersonView> Instances = new ();

        [Header("PROFILE")]
        [SerializeField] private ProfileCardComponentView profileCard;
        [SerializeField] private GameObject isGuestImage;
        [SerializeField] private Button wearablesListButton;
        [SerializeField] private Button userNameButton;
        [SerializeField] private Image dropdownArrow;
        [SerializeField] private Sprite arrowUp;

        [Header("WEARABLES")]
        [SerializeField] private NFTIconComponentView wearableTemplate;
        [SerializeField] private Transform wearablesListContainer;
        [SerializeField] private GameObject emptyWearablesListMessage;

        private readonly Dictionary<NFTIconComponentView, PoolableObject> wearables = new ();

        private Sprite arrowDown;
        private bool isShowingWearablesList;
        private bool hasWearables;

        public event Action<VisiblePerson> OnConfigureRequested;
        public event Action<NFTIconComponentModel> OnOpenWearableMarketplaceRequested;
        public event Action OnOpenProfileRequested;

        private void Awake()
        {
            wearablesListButton.onClick.AddListener(ShowHideList);
            userNameButton.onClick.AddListener(() => OnOpenProfileRequested?.Invoke());
            arrowDown = dropdownArrow.sprite;

            if (!Instances.Contains(this))
                Instances.Add(this);
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
        }

        public void Configure(VisiblePerson visiblePerson)
        {
            if (!Instances.Contains(this))
                Instances.Add(this);

            wearablesListContainer.gameObject.SetActive(false);
            OnConfigureRequested?.Invoke(visiblePerson);
        }

        public void ClearWearables()
        {
            foreach ((NFTIconComponentView _, PoolableObject poolObj) in wearables)
                poolObj.Release();

            wearables.Clear();
        }

        public void AddWearable(NFTIconComponentModel nftModel)
        {
            wearablesListContainer.gameObject.SetActive(false);
            hasWearables = true;

            Pool wearablePool = GetWearableEntryPool();
            PoolableObject poolObj = wearablePool.Get();
            NFTIconComponentView wearableEntry = poolObj.gameObject.GetComponent<NFTIconComponentView>();
            wearableEntry.transform.SetParent(wearablesListContainer, false);
            wearableEntry.Configure(nftModel);
            Button equipButton = wearableEntry.GetComponent<Button>();
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(() => OnOpenWearableMarketplaceRequested?.Invoke(nftModel));
            wearableEntry.gameObject.SetActive(true);
            wearables[wearableEntry] = poolObj;
        }

        public void SetProfileName(string userName)
        {
            profileCard.SetProfileName(userName);
        }

        public void SetProfileAddress(string address)
        {
            profileCard.SetProfileAddress(address);
        }

        public void SetProfilePicture(string url)
        {
            profileCard.SetProfilePicture(url);
        }

        public void SetGuestMode(bool isGuest)
        {
            isGuestImage.SetActive(isGuest);
            wearablesListButton.interactable = !isGuest;
        }

        private void ShowHideList()
        {
            isShowingWearablesList = !isShowingWearablesList;
            dropdownArrow.sprite = isShowingWearablesList ? arrowUp : arrowDown;

            if (hasWearables)
                wearablesListContainer.gameObject.SetActive(isShowingWearablesList);
            else
                emptyWearablesListMessage.SetActive(isShowingWearablesList);

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        private Pool GetWearableEntryPool()
        {
            const string POOL_ID = "CameraReelPictureDetailProfileWearables";
            var entryPool = PoolManager.i.GetPool(POOL_ID);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                POOL_ID,
                Instantiate(wearableTemplate).gameObject,
                maxPrewarmCount: 10,
                isPersistent: true);

            return entryPool;
        }
    }
}
