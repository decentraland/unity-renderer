using DCL.Helpers;
using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapToastView : MonoBehaviour
    {
        private static readonly int triggerLoadingComplete = Animator.StringToHash("LoadingComplete");

        public event Action<string, bool> OnFavoriteToggleClicked;
        public event Action<int, int> OnGoto;
        public event Action OnInfoClick;
        public event Action<string, bool?> OnVoteChanged;
        public event Action<Vector2Int> OnPressedLinkCopy;
        public event Action<Vector2Int, string> OnPressedTwitterButton;

        [SerializeField] internal TextMeshProUGUI sceneTitleText;
        [SerializeField] internal TextMeshProUGUI sceneOwnerText;
        [SerializeField] internal TextMeshProUGUI sceneLocationText;
        [SerializeField] internal TextMeshProUGUI playerCountText;
        [SerializeField] internal TextMeshProUGUI userVisitsText;
        [SerializeField] internal TextMeshProUGUI userRatingText;
        [SerializeField] internal RectTransform numberOfUsersContainer;
        [SerializeField] internal RectTransform userVisitsAndRatingContainer;
        [SerializeField] internal RectTransform toastContainer;
        [SerializeField] internal GameObject scenePreviewContainer;
        [SerializeField] internal RawImageFillParent scenePreviewImage;
        [SerializeField] internal Sprite scenePreviewFailImage;
        [SerializeField] internal Animator toastAnimator;
        [SerializeField] internal Image infoButtonImage;
        [SerializeField] internal Image favoriteButtonImage;
        [SerializeField] internal Sprite normalFavorite;
        [SerializeField] internal Sprite blockedFavorite;
        [SerializeField] internal Color normalImageColor;
        [SerializeField] internal Color disabledImageColor;

        [SerializeField] internal Button goToButton;
        [SerializeField] internal Button infoButton;
        [SerializeField] internal Button shareButton;
        [SerializeField] internal FavoriteButtonComponentView favoriteToggle;
        [SerializeField] internal GameObject favoriteLoading;
        [SerializeField] internal ButtonComponentView upvoteButton;
        [SerializeField] internal ButtonComponentView downvoteButton;
        [SerializeField] internal GameObject upvoteOff;
        [SerializeField] internal GameObject upvoteOn;
        [SerializeField] internal GameObject downvoteOff;
        [SerializeField] internal GameObject downvoteOn;
        [SerializeField] internal PlaceCopyContextualMenu placeCopyContextualMenu;

        [field: SerializeField]
        [Tooltip("Distance in units")]
        internal float distanceToCloseView { get; private set; } = 500;

        Vector2Int location;
        RectTransform rectTransform;
        MinimapMetadata minimapMetadata;
        private MinimapMetadata.MinimapSceneInfo sceneInfo;

        AssetPromise_Texture texturePromise;
        string currentImageUrl;
        private bool placeIsUpvote;
        private bool placeIsDownvote;
        private string placeId;

        public void Open(Vector2Int parcel, Vector2 worldPosition)
        {
            if (gameObject.activeInHierarchy)
                Close();

            var sceneInfo = minimapMetadata.GetSceneInfo(parcel.x, parcel.y);
            if (sceneInfo == null)
                WebInterface.RequestScenesInfoAroundParcel(parcel, 15);

            Populate(parcel, worldPosition, sceneInfo);
        }

        private void Awake()
        {
            favoriteToggle.OnFavoriteChange += (uuid, isFavorite) => OnFavoriteToggleClicked?.Invoke(uuid, isFavorite);
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(()=>OnInfoClick?.Invoke());
            minimapMetadata = MinimapMetadata.GetMetadata();
            rectTransform = transform as RectTransform;
            if(upvoteButton != null)
                upvoteButton.onClick.AddListener(() => ChangeVote(true));

            if(downvoteButton != null)
                downvoteButton.onClick.AddListener(() => ChangeVote(false));

            shareButton.onClick.RemoveAllListeners();
            shareButton.onClick.AddListener(OpenShareContextMenu);

            placeCopyContextualMenu.OnPlaceLinkCopied += OnLinkCopied;
            placeCopyContextualMenu.OnTwitter += OnOpenTwitter;

            placeCopyContextualMenu.Hide(true);
        }

        private void OpenShareContextMenu()
        {
            placeCopyContextualMenu.Show();
        }

        private void OnOpenTwitter()
        {
            OnPressedTwitterButton?.Invoke(location, sceneInfo.name);
        }

        private void OnLinkCopied()
        {
            OnPressedLinkCopy?.Invoke(location);
        }

        private void Start()
        {
            gameObject.SetActive(false);
            goToButton.onClick.AddListener(OnGotoClick);
        }

        private void OnDestroy()
        {
            if (texturePromise != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                texturePromise = null;
            }
            if(upvoteButton != null)
                upvoteButton.onClick.RemoveAllListeners();

            if(downvoteButton != null)
                downvoteButton.onClick.RemoveAllListeners();
        }

        public void Populate(Vector2Int coordinates, Vector2 worldPosition, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!gameObject.activeSelf)
                AudioScriptableObjects.dialogOpen.Play(true);

            this.sceneInfo = sceneInfo;
            bool sceneInfoExists = sceneInfo != null;

            gameObject.SetActive(true);
            scenePreviewImage.gameObject.SetActive(false);
            location = coordinates;

            PositionToast(worldPosition);

            sceneLocationText.text = $"{coordinates.x}, {coordinates.y}";

            sceneOwnerText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.owner));
            sceneTitleText.text = "Untitled Scene";

            bool useDefaultThumbnail =
                !sceneInfoExists || string.IsNullOrEmpty(sceneInfo.previewImageUrl);

            if (useDefaultThumbnail)
            {
                DisplayThumbnail(scenePreviewFailImage.texture);
                currentImageUrl = "";
            }

            if (sceneInfoExists)
            {
                sceneTitleText.text = sceneInfo.name;
                sceneOwnerText.text = $"Created by: {sceneInfo.owner}";

                if (currentImageUrl == sceneInfo.previewImageUrl)
                {
                    DisplayThumbnail(texturePromise?.asset?.texture);
                    return;
                }

                if (texturePromise != null)
                {
                    AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                    texturePromise = null;
                }

                if (!string.IsNullOrEmpty(sceneInfo.previewImageUrl))
                {
                    texturePromise = new AssetPromise_Texture(sceneInfo.previewImageUrl, storeTexAsNonReadable: false);
                    texturePromise.OnSuccessEvent += (textureAsset) => { DisplayThumbnail(textureAsset.texture); };
                    texturePromise.OnFailEvent += (textureAsset, error) => { DisplayThumbnail(scenePreviewFailImage.texture); };
                    AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                }

                currentImageUrl = sceneInfoExists ? sceneInfo.previewImageUrl : "";
            }
        }

        private void PositionToast(Vector2 worldPosition)
        {
            if (toastContainer == null || rectTransform == null)
                return;

            toastContainer.position = worldPosition;

            bool useBottom = toastContainer.localPosition.y > 0;

            bool shouldOffsetHorizontally = Mathf.Abs(toastContainer.localPosition.x) > rectTransform.rect.width / 4;
            bool useLeft = false;

            if (shouldOffsetHorizontally)
                useLeft = toastContainer.localPosition.x > 0;

            // By setting the pivot accordingly BEFORE we position the toast, we can have it always visible in an easier way
            toastContainer.pivot = new Vector2(shouldOffsetHorizontally ? (useLeft ? 1 : 0) : 0.5f, useBottom ? 1 : 0);
            toastContainer.position = worldPosition;
        }

        public void SetPlaceId(string UUID)
        {
            placeId = UUID;
        }

        public void SetVoteButtons(bool isUpvoted, bool isDownvoted)
        {
            placeIsUpvote = isUpvoted;
            placeIsDownvote = isDownvoted;
            upvoteOn.SetActive(isUpvoted);
            upvoteOff.SetActive(!isUpvoted);
            downvoteOn.SetActive(isDownvoted);
            downvoteOff.SetActive(!isDownvoted);
        }

        private void ChangeVote(bool upvote)
        {
            if (upvote)
            {
                OnVoteChanged?.Invoke(placeId, placeIsUpvote ? (bool?)null : true);
                placeIsUpvote = !placeIsUpvote;
                placeIsDownvote = false;
            }
            else
            {
                OnVoteChanged?.Invoke(placeId, placeIsDownvote ? (bool?)null : false);
                placeIsUpvote = false;
                placeIsDownvote = !placeIsDownvote;
            }
            SetVoteButtons(placeIsUpvote, placeIsDownvote);
        }

        public void Close()
        {
            if (gameObject.activeSelf)
                AudioScriptableObjects.dialogClose.Play(true);

            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
            OnGoto?.Invoke(location.x, location.y);
            Close();
        }

        private void DisplayThumbnail(Texture2D texture)
        {
            scenePreviewImage.texture = texture;
            toastAnimator.SetTrigger(triggerLoadingComplete);
        }

        public void SetFavoriteLoading(bool isLoading)
        {
            favoriteLoading.SetActive(isLoading);
        }

        public void SetCurrentFavoriteStatus(string uuid, bool isFavorite)
        {
            favoriteToggle.Configure(new FavoriteButtonComponentModel()
            {
                placeUUID = uuid,
                isFavorite = isFavorite
            });
        }

        public void SetIsAPlace(bool isAPlace)
        {
            favoriteToggle.SetInteractable(isAPlace);
            favoriteButtonImage.sprite = isAPlace ? normalFavorite : blockedFavorite;
            favoriteButtonImage.color = isAPlace ? normalImageColor : disabledImageColor;
            SetInfoButtonEnabled(isAPlace);

            if(!isAPlace)
                favoriteLoading.SetActive(false);
        }

        public void SetInfoButtonEnabled(bool isActive)
        {
            infoButton.interactable = isActive;
            infoButtonImage.color = isActive ? normalImageColor : disabledImageColor;
        }

        public void SetPlayerCount(int players)
        {
            numberOfUsersContainer.gameObject.SetActive(players > 0);
            playerCountText.text = players.ToString();
        }

        public void SetUserVisits(int userVisitsCount)
        {
            userVisitsText.text = userVisitsCount.ToString();
        }

        public void SetUserRating(float? userRatingCount)
        {
            userRatingText.text = userRatingCount != null ? $"{userRatingCount.Value * 100:0}%" : "-%";
        }

        public void RebuildLayouts()
        {
            if (numberOfUsersContainer != null)
                Utils.ForceRebuildLayoutImmediate(numberOfUsersContainer);

            if (userVisitsAndRatingContainer != null)
                Utils.ForceRebuildLayoutImmediate(userVisitsAndRatingContainer);
        }
    }
}
