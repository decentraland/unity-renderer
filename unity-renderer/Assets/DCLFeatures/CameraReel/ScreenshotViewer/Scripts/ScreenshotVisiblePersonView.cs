using DCLServices.CameraReelService;
using System;
using TMPro;
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
        [SerializeField] private TMP_Text userNameText;

        [Header("WEARABLES")]
        [SerializeField] private NFTIconComponentView wearableTemplate;
        [SerializeField] private Transform wearablesListContainer;
        [SerializeField] private GameObject emptyWearablesListMessage;

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

        public void AddWearable(NFTIconComponentModel nftModel)
        {
            wearablesListContainer.gameObject.SetActive(false);
            hasWearables = true;

            NFTIconComponentView wearableEntry = Instantiate(wearableTemplate, wearablesListContainer);
            wearableEntry.Configure(nftModel);
            wearableEntry.GetComponent<Button>().onClick.AddListener(() => OnOpenWearableMarketplaceRequested?.Invoke(nftModel));
            wearableEntry.gameObject.SetActive(true);
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
    }
}
