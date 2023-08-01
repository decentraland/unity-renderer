using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.WearablesCatalogService;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotVisiblePersonView : MonoBehaviour
    {
        [Header("PROFILE")]
        [SerializeField] private ProfileCardComponentView profileCard;
        [SerializeField] private GameObject isGuestImage;
        [SerializeField] private Button wearablesListButton;
        [SerializeField] private Image dropdownArrow;
        [SerializeField] private Sprite arrowUp;

        [Header("WEARABLES")]
        [SerializeField] private NFTIconComponentView wearableTemplate;
        [SerializeField] private Transform wearablesListContainer;
        [SerializeField] private GameObject emptyWearablesListMessage;

        private Sprite arrowDown;
        private bool isShowingWearablesList;

        private bool hasWearables;

        private void Awake()
        {
            wearablesListButton.onClick.AddListener(ShowHideList);
            arrowDown = dropdownArrow.sprite;
        }

        public void Configure(VisiblePerson visiblePerson)
        {
            profileCard.SetProfileName(visiblePerson.userName);
            profileCard.SetProfileAddress(visiblePerson.userAddress);
            UpdateProfileIcon(visiblePerson.userAddress, profileCard);

            if (visiblePerson.isGuest)
            {
                isGuestImage.SetActive(true);
                wearablesListButton.interactable = false;
            }
            else if (visiblePerson.wearables.Length > 0)
            {
                IWearablesCatalogService wearablesService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();
                FetchWearables(visiblePerson, wearablesService);
            }
        }

        private static async void UpdateProfileIcon(string userId, IProfileCardComponentView person)
        {
            UserProfile profile = UserProfileController.userProfilesCatalog.Get(userId) ?? await UserProfileController.i.RequestFullUserProfileAsync(userId);

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(profile.face256SnapshotURL);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log(request.error);
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                person.SetProfilePicture(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            }
        }

        private async void FetchWearables(VisiblePerson person, IWearablesCatalogService wearablesService)
        {
            wearablesListContainer.gameObject.SetActive(false);

            hasWearables = person.wearables.Length > 0;

            foreach (string wearable in person.wearables)
            {
                WearableItem wearableItem = await wearablesService.RequestWearableAsync(wearable, default(CancellationToken));

                var newModel = new NFTIconComponentModel
                {
                    name = wearableItem.GetName(),
                    imageURI = wearableItem.ComposeThumbnailUrl(),
                    rarity = wearableItem.rarity,
                    nftInfo = wearableItem.GetNftInfo(),
                    marketplaceURI = wearableItem.GetMarketplaceLink(),
                    showMarketplaceButton = true,
                    showType = false,
                    type = wearableItem.data.category,
                };

                NFTIconComponentView wearableEntry = Instantiate(wearableTemplate, wearablesListContainer);
                wearableEntry.Configure(newModel);
                wearableEntry.GetComponent<Button>().onClick.AddListener(() => { Application.OpenURL(newModel.marketplaceURI); });
                wearableEntry.gameObject.SetActive(true);
            }
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
